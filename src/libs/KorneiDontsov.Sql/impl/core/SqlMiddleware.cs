namespace KorneiDontsov.Sql {
	using KorneiDontsov.Sql.Migrations;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;
	using System;
	using System.Data;
	using System.Net.Mime;
	using System.Threading;
	using System.Threading.Tasks;
	using static System.Threading.Interlocked;

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
					context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
					context.Response.ContentType = MediaTypeNames.Text.Plain;
					await context.Response.WriteAsync(
						environment.IsDevelopment()
							? "Database migration is canceled. Service is stopping."
							: "Service is stopping.");
					break;

				case DbMigrationResult.Failed failedResult:
					context.Response.StatusCode = StatusCodes.Status500InternalServerError;
					context.Response.ContentType = MediaTypeNames.Text.Plain;
					await context.Response.WriteAsync(
						environment.IsDevelopment()
							? "Internal error occurred. Service is stopping."
							: $"Database migration failed. Service is stopping.\n{failedResult.info}");
					break;

				case DbMigrationResult.Succeeded _:
				case null:
					var mbAccess = mbEndpoint?.Metadata.GetMetadata<IBeginAccessEndpointMetadata?>()?.access;
					var mbIsoLevel = mbEndpoint?.Metadata.GetMetadata<IBeginIsolationLevelEndpointMetadata?>()
						?.isolationLevel;

					while(true) {
						context.RequestAborted.ThrowIfCancellationRequested();

						try {
							sqlScopeState.transaction ??=
								(mbAccess, mbIsoLevel) switch {
									(SqlAccess.Rw, _) => await dbProvider.BeginRw(
										mbIsoLevel ?? IsolationLevel.Serializable),
									(SqlAccess.Ro, _) => await dbProvider.BeginRo(
										mbIsoLevel ?? IsolationLevel.Serializable),
									(null, {} isolationLevel) => await dbProvider.Begin(SqlAccess.Ro, isolationLevel),
									(null, null) => null
								};
							await next(context);
							await (sqlScopeState.transaction?.CommitAsync(context.RequestAborted) ?? default);
							break;
						}
						catch(SqlException.SerializationFailure ex) {
							var log =
								"Failed to complete {requestProtocol} {requestMethod} {requestPath} with serialization failure."
								+ " Going to try again.";
							logger.LogWarning(
								ex, log, context.Request.Protocol, context.Request.Method, context.Request.Path);

							sqlScopeState.InvokeRetryEvent();
						}
						finally {
							await (Exchange(ref sqlScopeState.transaction, null)?.DisposeAsync() ?? default);
						}
					}
					break;

				default:
					throw new Exception($"{dbMigrationResult.GetType()} is not known.");
			}
		}
	}
}
