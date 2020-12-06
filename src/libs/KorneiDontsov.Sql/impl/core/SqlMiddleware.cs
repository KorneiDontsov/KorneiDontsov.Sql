namespace KorneiDontsov.Sql {
	using KorneiDontsov.Sql.Migrations;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using static Microsoft.AspNetCore.Http.StatusCodes;

	sealed class SqlMiddleware: IMiddleware {
		IHostEnvironment environment { get; }
		ILogger logger { get; }
		IDbMigrationState? dbMigrationState { get; }

		public SqlMiddleware
			(IHostEnvironment environment,
			 ILogger<SqlMiddleware> logger,
			 IDbMigrationState? dbMigrationState = null) {
			this.environment = environment;
			this.logger = logger;
			this.dbMigrationState = dbMigrationState;
		}

		Task WriteServerError (HttpContext context, String devInfoFormat) =>
			context.WriteTextResponse(
				Status500InternalServerError,
				environment.IsDevelopment() ? devInfoFormat : "Internal error occurred.");

		Task WriteFormattedServerError (HttpContext context, FormattableString devInfo) =>
			context.WriteTextResponse(
				Status500InternalServerError,
				environment.IsDevelopment() ? devInfo.ToString() : "Internal error occurred.");

		async Task InvokeWithSqlScope
			(HttpContext context, RequestDelegate next, SqlScope sqlScope, EndpointMetadataCollection? metadata) {
			static IReadOnlyList<T>? MayGetOrderedMetadata<T> (EndpointMetadataCollection? metadata) where T: class =>
				metadata?.GetOrderedMetadata<T>() is {} d && d.Count > 0 ? d : null;

			static Boolean ShouldCommit (Int32 statusCode, IReadOnlyList<ICommitOnEndpointMetadata> metadata) {
				foreach(var datum in metadata)
					if(statusCode == datum.statusCode)
						return true;
				return false;
			}

			static Boolean ShouldConflict
				(SqlConflict conflict, IReadOnlyList<IConflictOnEndpointMetadata> conflictOnMetadata) {
				foreach(var datum in conflictOnMetadata)
					if(conflict == datum.conflict)
						return true;
				return false;
			}

			try {
				if(metadata?.GetMetadata<IBeginSqlTransactionEndpointMetadata>() is {} beginMetadata)
					_ = beginMetadata.access switch {
						SqlAccess.Rw =>
							await sqlScope.GetOrCreateOrUpgradeRwSqlTransaction(
								beginMetadata.isolationLevel,
								context.RequestAborted),
						SqlAccess.Ro =>
							await sqlScope.GetOrCreateOrUpgradeRoSqlTransaction(
								beginMetadata.isolationLevel,
								context.RequestAborted),
						null =>
							await sqlScope.GetOrCreateSqlTransaction(
								beginMetadata.isolationLevel,
								context.RequestAborted)
					};
				try {
					await next(context);

					await (sqlScope.transaction is {} transaction
					       && context.Response.StatusCode is var statusCode
					       && MayGetOrderedMetadata<ICommitOnEndpointMetadata>(metadata) is var commitOn
					       && (commitOn is null && statusCode >= 200 && statusCode < 300
					           || commitOn is {} && ShouldCommit(statusCode, commitOn))
						? transaction.CommitAsync(context.RequestAborted)
						: default);
				}
				catch(SqlException.ConflictFailure ex)
					when(sqlScope.transaction is {}
					     && MayGetOrderedMetadata<IConflictOnEndpointMetadata>(metadata) is var conflictOn
					     && (conflictOn is null && ex.conflict is SqlConflict.SerializationFailure
					         || conflictOn is {} && ShouldConflict(ex.conflict, conflictOn))) {
					var log =
						"Failed to complete {requestProtocol} {requestMethod} {requestPath} with "
						+ "serialization failure.";
					logger.LogWarning(
						ex, log, context.Request.Protocol, context.Request.Method, context.Request.Path);

					await context.WriteTextResponse(
						Status409Conflict,
						"Data access conflict occurred. Try again.");
				}
			}
			catch(OperationCanceledException) when(context.RequestAborted.IsCancellationRequested) { }
		}

		Task InvokeWhereDbMigrationResultIsNotOk (HttpContext context, DbMigrationResult? dbMigrationResult) =>
			dbMigrationResult switch {
				null =>
					context.WriteTextResponse(
						Status503ServiceUnavailable,
						environment.IsDevelopment()
							? "The service is migrating database. It takes time."
							: "The service is starting."),
				DbMigrationResult.Canceled _ =>
					WriteServerError(context, "Database migration is canceled. Service is stopping."),
				DbMigrationResult.Failed f =>
					WriteFormattedServerError(context, $"Database migration failed. Service is stopping.\n{f.info}")
			};

		/// <inheritdoc />
		public Task InvokeAsync (HttpContext context, RequestDelegate next) {
			var metadata = context.GetEndpoint()?.Metadata;

			var dbMigrationResult =
				dbMigrationState is {}
				&& metadata?.GetMetadata<IDbMigrationEndpointMetadata>()?.isRequired != false
					? dbMigrationState.result
					: DbMigrationResult.ok;
			if(! (dbMigrationResult is DbMigrationResult.Ok))
				return InvokeWhereDbMigrationResultIsNotOk(context, dbMigrationResult);
			else if(! (context.RequestServices.GetService<SqlScope>() is {} sqlScope))
				return next(context);
			else
				return InvokeWithSqlScope(context, next, sqlScope, metadata);
		}
	}
}
