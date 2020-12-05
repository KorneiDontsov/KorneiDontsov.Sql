namespace KorneiDontsov.Sql {
	using KorneiDontsov.Sql.Migrations;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using static Microsoft.AspNetCore.Http.StatusCodes;

	class SqlMiddleware: IMiddleware {
		IDbProvider dbProvider { get; }
		SqlScopeState sqlScopeState { get; }
		IHostEnvironment environment { get; }
		ILogger logger { get; }
		IDbMigrationState? dbMigrationState { get; }

		public SqlMiddleware
			(IDbProvider dbProvider,
			 SqlScopeState sqlScopeState,
			 IHostEnvironment environment,
			 ILogger<SqlMiddleware> logger,
			 IDbMigrationState? dbMigrationState = null) {
			this.dbProvider = dbProvider;
			this.sqlScopeState = sqlScopeState;
			this.environment = environment;
			this.logger = logger;
			this.dbMigrationState = dbMigrationState;
		}

		/// <inheritdoc />
		public async Task InvokeAsync (HttpContext context, RequestDelegate next) {
			var mbEndpoint = context.GetEndpoint();

			DbMigrationResult? dbMigrationResult;

			if(dbMigrationState is null)
				dbMigrationResult = null;
			else {
				var dbMigrationIsRequired =
					mbEndpoint?.Metadata.GetMetadata<IDbMigrationEndpointMetadata?>()?.isRequired
					?? true;
				if(! dbMigrationIsRequired)
					dbMigrationResult = null;
				else
					dbMigrationResult = await dbMigrationState.WhenCompleted(context.RequestAborted);
			}

			switch(dbMigrationResult) {
				case DbMigrationResult.Canceled _:
					await context.WriteTextResponse(
						Status503ServiceUnavailable,
						environment.IsDevelopment()
							? "Database migration is canceled. Service is stopping."
							: "Service is stopping.");
					break;

				case DbMigrationResult.Failed failedResult:
					await context.WriteTextResponse(
						Status500InternalServerError,
						environment.IsDevelopment()
							? $"Database migration failed. Service is stopping.\n{failedResult.info}"
							: "Internal error occurred. Service is stopping.");
					break;

				case DbMigrationResult.Succeeded _:
				case null:
					var mbBeginMetadata = mbEndpoint?.Metadata.GetMetadata<IBeginSqlTransactionEndpointMetadata?>();
					var mbCommitOnMetadata =
						mbEndpoint?.Metadata.GetOrderedMetadata<ICommitOnEndpointMetadata>()
							.Select(data => data.statusCode)
							.ToHashSet();

					if(mbBeginMetadata is {}
					   && ! (mbBeginMetadata.access is SqlAccess.Ro)
					   && (mbCommitOnMetadata is null || mbCommitOnMetadata.Count is 0)) {
						var log =
							"Denied to execute endpoint {requestMethod} {requestPath}: "
							+ "endpoint requests read-write sql transaction to begin but it doesn't specify "
							+ "commit conditions. Probably, one or more attributes [CommitOn] are missed.";
						logger.LogError(log, context.Request.Method, context.Request.Path);

						await context.WriteTextResponse(
							Status500InternalServerError,
							environment.IsDevelopment()
								? "Endpoint configuration error."
								: "Internal error occurred.");
					}
					else {
						context.RequestAborted.ThrowIfCancellationRequested();

						try {
							sqlScopeState.transaction ??=
								mbBeginMetadata switch {
									{ access: SqlAccess.Rw } =>
										await dbProvider.BeginRw(mbBeginMetadata.isolationLevel),
									{ access: SqlAccess.Ro } =>
										await dbProvider.BeginRo(mbBeginMetadata.isolationLevel),
									{ access: null } =>
										await dbProvider.Begin(SqlAccess.Ro, mbBeginMetadata.isolationLevel),
									null => null
								};

							await next(context);

							if(sqlScopeState.transaction is {} transaction
							   && mbCommitOnMetadata?.Contains(context.Response.StatusCode) is true)
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
					break;

				default:
					throw new Exception($"{dbMigrationResult.GetType()} is not known.");
			}
		}
	}
}
