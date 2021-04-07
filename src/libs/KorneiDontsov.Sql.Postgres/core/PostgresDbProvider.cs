namespace KorneiDontsov.Sql.Postgres {
	using Npgsql;
	using System;
	using System.Data;
	using System.Data.Common;
	using System.Runtime.CompilerServices;
	using System.Threading;
	using System.Threading.Tasks;

	sealed class PostgresDbProvider: IDbProvider {
		PostgresDbProviderSettings settings { get; }
		String connectionString { get; }

		public PostgresDbProvider (PostgresDbProviderSettings settings) {
			this.settings = settings;

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
					throw new($"Password source {settings.passwordSource.GetType()} is not expected.");
			}

			connectionString = connectionStringBuilder.ConnectionString;
		}

		/// <inheritdoc />
		public String databaseName => settings.database;

		/// <inheritdoc />
		public String username => settings.username;

		/// <inheritdoc />
		public Int32 defaultQueryTimeout => settings.defaultQueryTimeout;

		/// <inheritdoc />
		public async ValueTask<IManagedSqlTransaction> Begin
			(IsolationLevel isolationLevel,
			 CancellationToken cancellationToken = default,
			 SqlAccess? access = null,
			 Int32? defaultQueryTimeout = null) {
			var npgsqlConnection = new NpgsqlConnection(connectionString);
			try {
				await npgsqlConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
				NpgsqlTransaction npgsqlTransaction =
					await npgsqlConnection.BeginTransactionAsync(isolationLevel, cancellationToken)
						.ConfigureAwait(false);

				var timeout = defaultQueryTimeout ?? settings.defaultQueryTimeout;
				var transaction =
					new PostgresTransaction(
						npgsqlConnection,
						npgsqlTransaction,
						access ?? settings.defaultAccess,
						isolationLevel,
						timeout);

				if(access is { } accessValue && accessValue != settings.defaultAccess)
					await transaction.SetAccessAsync(accessValue, cancellationToken).ConfigureAwait(false);

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
			var npgsqlConnection = new NpgsqlConnection(connectionString);
			try {
				await npgsqlConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
				NpgsqlTransaction npgsqlTransaction =
					await npgsqlConnection.BeginTransactionAsync(isolationLevel, cancellationToken)
						.ConfigureAwait(false);

				var timeout = defaultQueryTimeout ?? settings.defaultQueryTimeout;
				var transaction =
					new PostgresRwTransaction(npgsqlConnection, npgsqlTransaction, isolationLevel, timeout);

				if(settings.defaultAccess is not SqlAccess.Rw)
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
			var npgsqlConnection = new NpgsqlConnection(connectionString);
			try {
				await npgsqlConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
				NpgsqlTransaction npgsqlTransaction =
					await npgsqlConnection.BeginTransactionAsync(isolationLevel, cancellationToken)
						.ConfigureAwait(false);

				var timeout = defaultQueryTimeout ?? settings.defaultQueryTimeout;
				var transaction =
					new PostgresRoTransaction(npgsqlConnection, npgsqlTransaction, isolationLevel, timeout);

				if(settings.defaultAccess is not SqlAccess.Ro)
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
