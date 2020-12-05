namespace KorneiDontsov.Sql {
	using KorneiDontsov.Sql.Migrations;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;
	using System;
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

		async Task DoInvokeWithSqlScope
			(HttpContext context,
			 RequestDelegate next,
			 SqlScope sqlScope,
			 IBeginSqlTransactionEndpointMetadata? beginMetadata,
			 Func<Int32, Boolean> commitOn) {
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
					if(sqlScope.transaction is {} transaction && commitOn(context.Response.StatusCode))
						await transaction.CommitAsync(context.RequestAborted);
				}
				catch(SqlException.ConflictFailure ex) when(ex.conflict is SqlConflict.SerializationFailure) {
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

		Task InvokeWhereDbMigrationResultIsOk (HttpContext context, RequestDelegate next, Endpoint? endpoint) {
			if(context.RequestServices.GetService<SqlScope>() is {} sqlScope) {
				var beginMetadata = endpoint?.Metadata.GetMetadata<IBeginSqlTransactionEndpointMetadata?>();
				var commitOn =
					endpoint?.Metadata.GetOrderedMetadata<ICommitOnEndpointMetadata>() is {Count: > 0} metadata
						? new Func<Int32, Boolean>(
							statusCode => {
								foreach(var datum in metadata)
									if(statusCode == datum.statusCode)
										return true;
								return false;
							})
						: statusCode => statusCode is >= 200 and < 300;
				return DoInvokeWithSqlScope(context, next, sqlScope, beginMetadata, commitOn);
			}
			else
				return next(context);
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
			var endpoint = context.GetEndpoint();
			var dbMigrationResult =
				dbMigrationState is {}
				&& endpoint?.Metadata.GetMetadata<IDbMigrationEndpointMetadata>()?.isRequired is not false
					? dbMigrationState.result
					: DbMigrationResult.ok;
			return dbMigrationResult is DbMigrationResult.Ok
				? InvokeWhereDbMigrationResultIsOk(context, next, endpoint)
				: InvokeWhereDbMigrationResultIsNotOk(context, dbMigrationResult);
		}
	}
}
