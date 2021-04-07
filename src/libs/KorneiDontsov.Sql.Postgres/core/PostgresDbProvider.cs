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

		interface IBeginTrait<TTransaction> {
			TTransaction CreateTransaction
				(NpgsqlConnection npgsqlConnection,
				 NpgsqlTransaction npgsqlTransaction,
				 IsolationLevel isolationLevel,
				 SqlAccess? access,
				 Int32 defaultQueryTimeout);
		}

		async ValueTask<TTransaction> Begin<TTrait, TTransaction>
			(IsolationLevel isolationLevel,
			 CancellationToken cancellationToken,
			 SqlAccess? access,
			 Int32? defaultQueryTimeout)
			where TTrait: struct, IBeginTrait<TTransaction>
			where TTransaction: ISqlTransaction {
			var npgsqlConnection = new NpgsqlConnection(connectionString);
			try {
				await npgsqlConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
				NpgsqlTransaction npgsqlTransaction =
					await npgsqlConnection.BeginTransactionAsync(isolationLevel, cancellationToken)
						.ConfigureAwait(false);

				var finalTimeout = defaultQueryTimeout ?? settings.defaultQueryTimeout;
				var transaction =
					default(TTrait).CreateTransaction(
						npgsqlConnection,
						npgsqlTransaction,
						isolationLevel,
						access ?? settings.defaultAccess,
						finalTimeout);

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

		readonly struct BeginTrait: IBeginTrait<IManagedSqlTransaction> {
			public IManagedSqlTransaction CreateTransaction
				(NpgsqlConnection npgsqlConnection,
				 NpgsqlTransaction npgsqlTransaction,
				 IsolationLevel isolationLevel,
				 SqlAccess? access,
				 Int32 defaultQueryTimeout) =>
				new PostgresTransaction(
					npgsqlConnection,
					npgsqlTransaction,
					isolationLevel,
					access,
					defaultQueryTimeout);
		}

		/// <inheritdoc />
		public ValueTask<IManagedSqlTransaction> Begin
			(IsolationLevel isolationLevel,
			 CancellationToken cancellationToken = default,
			 SqlAccess? access = null,
			 Int32? defaultQueryTimeout = null) =>
			Begin<BeginTrait, IManagedSqlTransaction>(
				isolationLevel,
				cancellationToken,
				access,
				defaultQueryTimeout);

		readonly struct BeginRwTrait: IBeginTrait<IManagedRwSqlTransaction> {
			public IManagedRwSqlTransaction CreateTransaction
				(NpgsqlConnection npgsqlConnection,
				 NpgsqlTransaction npgsqlTransaction,
				 IsolationLevel isolationLevel,
				 SqlAccess? access,
				 Int32 defaultQueryTimeout) =>
				new PostgresRwTransaction(
					npgsqlConnection,
					npgsqlTransaction,
					isolationLevel,
					defaultQueryTimeout);
		}

		/// <inheritdoc />
		public ValueTask<IManagedRwSqlTransaction> BeginRw
			(IsolationLevel isolationLevel,
			 CancellationToken cancellationToken = default,
			 Int32? defaultQueryTimeout = null) =>
			Begin<BeginRwTrait, IManagedRwSqlTransaction>(
				isolationLevel,
				cancellationToken,
				SqlAccess.Rw,
				defaultQueryTimeout);

		readonly struct BeginRoTrait: IBeginTrait<IManagedRoSqlTransaction> {
			public IManagedRoSqlTransaction CreateTransaction
				(NpgsqlConnection npgsqlConnection,
				 NpgsqlTransaction npgsqlTransaction,
				 IsolationLevel isolationLevel,
				 SqlAccess? access,
				 Int32 defaultQueryTimeout) =>
				new PostgresRoTransaction(
					npgsqlConnection,
					npgsqlTransaction,
					isolationLevel,
					defaultQueryTimeout);
		}

		/// <inheritdoc />
		public ValueTask<IManagedRoSqlTransaction> BeginRo
			(IsolationLevel isolationLevel,
			 CancellationToken cancellationToken = default,
			 Int32? defaultQueryTimeout = null) =>
			Begin<BeginRoTrait, IManagedRoSqlTransaction>(
				isolationLevel,
				cancellationToken,
				SqlAccess.Ro,
				defaultQueryTimeout);

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
