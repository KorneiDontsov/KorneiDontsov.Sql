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

	class SqlMiddleware: IMiddleware {
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

		Task WriteInternalServerError (HttpContext context, String devInfoFormat) =>
			context.WriteTextResponse(
				Status500InternalServerError,
				environment.IsDevelopment() ? devInfoFormat : "Internal error occurred.");

		Task WriteInternalServerError (HttpContext context, String devInfoFormat, params Object[] args) =>
			context.WriteTextResponse(
				Status500InternalServerError,
				environment.IsDevelopment() ? String.Format(devInfoFormat, args) : "Internal error occurred.");

		async Task InvokeWithSqlScope
			(HttpContext context,
			 RequestDelegate next,
			 SqlScope sqlScope,
			 IBeginSqlTransactionEndpointMetadata? beginMetadata,
			 IReadOnlyList<ICommitOnEndpointMetadata>? commitOnMetadata,
			 IReadOnlyList<IConflictOnEndpointMetadata>? conflictOnMetadata) {
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
				if(beginMetadata is {})
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
					       && (commitOnMetadata is null or {Count: 0} && context.Response.StatusCode is >= 200 and < 300
					           || commitOnMetadata is {} && ShouldCommit(context.Response.StatusCode, commitOnMetadata))
						? transaction.CommitAsync(context.RequestAborted)
						: default);
				}
				catch(SqlException.ConflictFailure ex)
					when(sqlScope.transaction is {}
					     && (conflictOnMetadata is null or {Count: 0} && ex.conflict is SqlConflict.SerializationFailure
					         || conflictOnMetadata is {} && ShouldConflict(ex.conflict, conflictOnMetadata))) {
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
				DbMigrationResult.Canceled =>
					WriteInternalServerError(context, "Database migration is canceled. Service is stopping."),

				DbMigrationResult.Failed failedResult =>
					WriteInternalServerError(
						context,
						"Database migration failed. Service is stopping.\n{0}",
						failedResult.info)
			};

		/// <inheritdoc />
		public Task InvokeAsync (HttpContext context, RequestDelegate next) {
			var metadata = context.GetEndpoint()?.Metadata;

			var dbMigrationResult =
				dbMigrationState is {}
				&& metadata?.GetMetadata<IDbMigrationEndpointMetadata>()?.isRequired is not false
					? dbMigrationState.result
					: DbMigrationResult.ok;
			if(dbMigrationResult is not DbMigrationResult.Ok)
				return InvokeWhereDbMigrationResultIsNotOk(context, dbMigrationResult);
			else if(! (context.RequestServices.GetService<SqlScope>() is { }
				sqlScope))
				return next(context);
			else {
				var beginMetadata = metadata?.GetMetadata<IBeginSqlTransactionEndpointMetadata>();
				var commitOnMetadata = metadata?.GetOrderedMetadata<ICommitOnEndpointMetadata>();
				var conflictOnMetadata = metadata?.GetOrderedMetadata<IConflictOnEndpointMetadata>();
				return InvokeWithSqlScope(context, next, sqlScope, beginMetadata, commitOnMetadata, conflictOnMetadata);
			}
		}
	}
}
