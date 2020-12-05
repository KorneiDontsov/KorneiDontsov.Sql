namespace KorneiDontsov.Sql {
	using KorneiDontsov.Sql.Migrations;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;
	using System;
	using System.Collections.Immutable;
	using System.Linq;
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
			 ImmutableHashSet<Int32> commitOnStatusCodes) {
			var createdTransaction =
				beginMetadata is null
					? null
					: beginMetadata.access switch {
						SqlAccess.Rw =>
							await sqlScope.GetOrCreateOrUpgradeRwSqlTransaction(
								beginMetadata.isolationLevel,
								context.RequestAborted),
						SqlAccess.Ro =>
							await sqlScope.GetOrCreateOrUpgradeRoSqlTransaction(
								beginMetadata.isolationLevel,
								context.RequestAborted),
						null =>
							await sqlScope.GetOrCreateOrUpgradeSqlTransaction(
								beginMetadata.isolationLevel,
								context.RequestAborted)
					};
			if(createdTransaction?.initialIsolationLevel != beginMetadata?.isolationLevel) {
				var log =
					"Failed to begin {IsolationLevel} transaction, because the request scope already has "
					+ "{ActualIsolationLevel} transaction.";
				logger.LogCritical(log, beginMetadata!.isolationLevel, createdTransaction!.initialIsolationLevel);

				await WriteInternalServerError(context, "SQL Isolation level conflict error.");
			}
			else
				try {
					await next(context);
					if(sqlScope.transaction is {} transaction
					   && commitOnStatusCodes.Contains(context.Response.StatusCode))
						await transaction.CommitAsync(context.RequestAborted);
				}
				catch(SqlException.SerializationFailure ex) {
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

		Task InvokeWhereDbMigrationResultIsOk (HttpContext context, RequestDelegate next, Endpoint? endpoint) {
			var mbBeginMetadata = endpoint?.Metadata.GetMetadata<IBeginSqlTransactionEndpointMetadata?>();
			var commitOnStatusCodes =
				endpoint?.Metadata.GetOrderedMetadata<ICommitOnEndpointMetadata>()
					.Select(data => data.statusCode)
					.ToImmutableHashSet()
				?? ImmutableHashSet<Int32>.Empty;

			if(mbBeginMetadata?.access is SqlAccess.Rw && commitOnStatusCodes.IsEmpty) {
				var log =
					"Denied to execute endpoint {requestMethod} {requestPath}: "
					+ "endpoint requests read-write sql transaction to begin but it doesn't specify "
					+ "commit conditions. Probably, one or more attributes [CommitOn] are missed.";
				logger.LogCritical(log, context.Request.Method, context.Request.Path);

				return WriteInternalServerError(context, "Read-write SQL transaction configuration error.");
			}
			else if(context.RequestServices.GetService<SqlScope>() is {} sqlScope)
				return InvokeWithSqlScope(context, next, sqlScope, mbBeginMetadata, commitOnStatusCodes);
			else
				return next(context);
		}

		Task DbMigrationResultIsNotOk (HttpContext context, DbMigrationResult dbMigrationResult) =>
			dbMigrationResult switch {
				DbMigrationResult.Canceled =>
					WriteInternalServerError(context, "Database migration is canceled. Service is stopping."),
				DbMigrationResult.Failed failedResult =>
					WriteInternalServerError(
						context,
						"Database migration failed. Service is stopping.\n{0}",
						failedResult.info)
			};

		/// <inheritdoc />
		public async Task InvokeAsync (HttpContext context, RequestDelegate next) {
			var endpoint = context.GetEndpoint();
			DbMigrationResult dbMigrationResult =
				dbMigrationState is {}
				&& endpoint?.Metadata.GetMetadata<IDbMigrationEndpointMetadata>()?.isRequired is not false
					? await dbMigrationState.WhenCompleted(context.RequestAborted)
					: DbMigrationResult.ok;
			if(dbMigrationResult is DbMigrationResult.Ok)
				await InvokeWhereDbMigrationResultIsOk(context, next, endpoint);
			else
				await DbMigrationResultIsNotOk(context, dbMigrationResult);
		}
	}
}
