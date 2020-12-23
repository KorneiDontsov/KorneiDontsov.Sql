namespace KorneiDontsov.Sql.Postgres {
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;
	using Npgsql;
	using System;
	using System.Data;
	using System.Data.Common;
	using System.Net.Sockets;
	using System.Runtime.CompilerServices;
	using System.Threading;
	using System.Threading.Tasks;

	sealed class PostgresDbProvider: IDbProvider {
		PostgresDbProviderSettings settings { get; }
		IHostApplicationLifetime appLifetime { get; }
		ILogger logger { get; }
		String connectionString { get; }

		public PostgresDbProvider
			(PostgresDbProviderSettings settings,
			 IHostApplicationLifetime appLifetime,
			 ILogger<PostgresDbProvider> logger) {
			this.settings = settings;
			this.appLifetime = appLifetime;
			this.logger = logger;

			var connectionStringBuilder =
				new NpgsqlConnectionStringBuilder {
					Database = settings.database,
					Host = settings.host,
					Port = settings.port,
					Username = settings.username,
					CommandTimeout = settings.defaultQueryTimeout,
					SearchPath = settings.searchPath,
					Timeout = settings.connectionTimeout,
					Pooling = true,
					MinPoolSize = settings.minPoolSize,
					MaxPoolSize = settings.maxPoolSize,
					ConnectionIdleLifetime = settings.connectionIdleLifetime,
					ConnectionPruningInterval = settings.connectionPruningInterval
				};
			switch(settings.passwordSource) {
				case PostgresPasswordSource.Text text:
					connectionStringBuilder.Password = text.value;
					break;
				case PostgresPasswordSource.PgPassFile file:
					connectionStringBuilder.Passfile = file.path;
					break;
				default:
					throw new Exception($"Password source {settings.passwordSource.GetType()} is not expected.");
			}

			connectionString = connectionStringBuilder.ConnectionString;
		}

		async Task PingDbUntilItsReady (String connectionString) {
			try {
				while(! appLifetime.ApplicationStopped.IsCancellationRequested) {
					var npgsqlConnection = new NpgsqlConnection(connectionString);
					try {
						await npgsqlConnection.OpenAsync(appLifetime.ApplicationStopped).ConfigureAwait(false);
						break;
					}
					catch(NpgsqlException ex)
						when(ex.InnerException is TimeoutException
						     || ex.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionRefused }
						     || ex.InnerException is SocketException { SocketErrorCode: SocketError.TimedOut }) {
						var log = "Database is not yet ready for use.\n{connectionString}";
						logger.LogInformation(ex, log, connectionString);

						await Task.Delay(500, appLifetime.ApplicationStopped).ConfigureAwait(false);
					}
					finally { await npgsqlConnection.DisposeAsync().ConfigureAwait(false); }
				}
			}
			catch(OperationCanceledException) { }
		}

		SqlException DbIsNotReadyException (Exception innerException) {
			var msg =
				"Failed to await until database is ready to accept connections. "
				+ $"Connection string: {connectionString}.";
			return new SqlException(msg, innerException);
		}

		async Task HandlePingDbTask (Task pingDbTask, CancellationToken ct) {
			var whenCompletedOrCanceled =
				await Task.WhenAny(pingDbTask, Task.Delay(Timeout.Infinite, ct)).ConfigureAwait(false);
			if(whenCompletedOrCanceled != pingDbTask)
				await whenCompletedOrCanceled.ConfigureAwait(false);
			else if(! pingDbTask.IsCompletedSuccessfully)
				throw DbIsNotReadyException(pingDbTask.Exception!);
		}

		volatile Task? pingDbTask = null;

		Task WhenDbIsReady (CancellationToken ct) =>
			// ReSharper disable once NonAtomicCompoundOperator
			pingDbTask ??= PingDbUntilItsReady(connectionString) switch {
				{ IsCompletedSuccessfully: true } => Task.CompletedTask,
				{ IsCompleted: false } task => HandlePingDbTask(task, ct),
				var task => throw DbIsNotReadyException(task.Exception!)
			};

		Task WhenDbIsReadyIfRequired (CancellationToken ct) =>
			settings.awaitUntilDbIsReady switch {
				false => Task.CompletedTask,
				true => WhenDbIsReady(ct)
			};

		/// <inheritdoc />
		public String databaseName => settings.database;

		/// <inheritdoc />
		public String username => settings.username;

		/// <inheritdoc />
		public Int32 defaultQueryTimeout => settings.defaultQueryTimeout;

		/// <inheritdoc />
		public async ValueTask<IManagedSqlTransaction> Begin
			(SqlAccess access,
			 IsolationLevel isolationLevel,
			 CancellationToken cancellationToken = default,
			 Int32? defaultQueryTimeout = null) {
			await WhenDbIsReadyIfRequired(cancellationToken).ConfigureAwait(false);

			var npgsqlConnection = new NpgsqlConnection(connectionString);
			try {
				await npgsqlConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
				NpgsqlTransaction npgsqlTransaction =
					await npgsqlConnection.BeginTransactionAsync(isolationLevel, cancellationToken)
						.ConfigureAwait(false);

				var timeout = defaultQueryTimeout ?? settings.defaultQueryTimeout;
				var transaction =
					new PostgresTransaction(npgsqlConnection, npgsqlTransaction, access, isolationLevel, timeout);

				await transaction.SetAccessAsync(access, cancellationToken).ConfigureAwait(false);
				npgsqlConnection = null;
				return transaction;
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
			finally {
				await (npgsqlConnection?.DisposeAsync() ?? default).ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<IManagedRwSqlTransaction> BeginRw
			(IsolationLevel isolationLevel,
			 CancellationToken cancellationToken = default,
			 Int32? defaultQueryTimeout = null) {
			await WhenDbIsReadyIfRequired(cancellationToken).ConfigureAwait(false);

			var npgsqlConnection = new NpgsqlConnection(connectionString);
			try {
				await npgsqlConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
				NpgsqlTransaction npgsqlTransaction =
					await npgsqlConnection.BeginTransactionAsync(isolationLevel, cancellationToken)
						.ConfigureAwait(false);

				var timeout = defaultQueryTimeout ?? settings.defaultQueryTimeout;
				var transaction =
					new PostgresRwTransaction(npgsqlConnection, npgsqlTransaction, isolationLevel, timeout);

				await transaction.SetRwAsync(cancellationToken).ConfigureAwait(false);
				npgsqlConnection = null;
				return transaction;
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
			finally {
				await (npgsqlConnection?.DisposeAsync() ?? default).ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<IManagedRoSqlTransaction> BeginRo
			(IsolationLevel isolationLevel,
			 CancellationToken cancellationToken = default,
			 Int32? defaultQueryTimeout = null) {
			await WhenDbIsReadyIfRequired(cancellationToken).ConfigureAwait(false);

			var npgsqlConnection = new NpgsqlConnection(connectionString);
			try {
				await npgsqlConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
				NpgsqlTransaction npgsqlTransaction =
					await npgsqlConnection.BeginTransactionAsync(isolationLevel, cancellationToken)
						.ConfigureAwait(false);

				var timeout = defaultQueryTimeout ?? settings.defaultQueryTimeout;
				var transaction =
					new PostgresRoTransaction(npgsqlConnection, npgsqlTransaction, isolationLevel, timeout);

				await transaction.SetRoAsync(cancellationToken).ConfigureAwait(false);
				npgsqlConnection = null;
				return transaction;
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
			finally {
				await (npgsqlConnection?.DisposeAsync() ?? default).ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public TConnection CreateConnection<TConnection> () where TConnection: class {
			if(typeof(TConnection) == typeof(NpgsqlConnection)
			   || typeof(TConnection) == typeof(DbConnection)
			   || typeof(TConnection) == typeof(IDbConnection))
				return Unsafe.As<TConnection>(new NpgsqlConnection(connectionString));
			else
				throw new NotSupportedException(
					$"{typeof(TConnection)} is not known. Only {typeof(NpgsqlConnection)} is supported.");
		}
	}
}
