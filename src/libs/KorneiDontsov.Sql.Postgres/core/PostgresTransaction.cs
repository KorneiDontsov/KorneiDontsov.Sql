namespace KorneiDontsov.Sql.Postgres {
	using Dapper;
	using Npgsql;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Threading;
	using System.Threading.Tasks;

	class PostgresTransaction: IManagedSqlTransaction {
		NpgsqlConnection npgsqlConnection { get; }

		NpgsqlTransaction npgsqlTransaction { get; }

		/// <inheritdoc />
		public SqlAccess initialAccess { get; }

		/// <inheritdoc />
		public IsolationLevel initialIsolationLevel { get; }

		/// <inheritdoc />
		public Int32 defaultQueryTimeout { get; }

		public PostgresTransaction
			(NpgsqlConnection npgsqlConnection,
			 NpgsqlTransaction npgsqlTransaction,
			 SqlAccess initialAccess,
			 IsolationLevel initialIsolationLevel,
			 Int32 defaultQueryTimeout) {
			this.npgsqlConnection = npgsqlConnection;
			this.npgsqlTransaction = npgsqlTransaction;
			this.initialAccess = initialAccess;
			this.initialIsolationLevel = initialIsolationLevel;
			this.defaultQueryTimeout = defaultQueryTimeout;
		}

		Boolean isDisposed;

		/// <inheritdoc />
		public async ValueTask DisposeAsync () {
			if(! isDisposed) {
				isDisposed = true;
				await npgsqlTransaction.DisposeAsync();
				await npgsqlConnection.DisposeAsync();
			}
		}

		/// <inheritdoc />
		public async ValueTask CommitAsync (CancellationToken cancellationToken = default) {
			try {
				await npgsqlTransaction.CommitAsync(cancellationToken);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask RollbackAsync (CancellationToken cancellationToken = default) {
			try {
				await npgsqlTransaction.RollbackAsync(cancellationToken);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		CommandDefinition CreateCommand
			(String sql, CancellationToken cancellationToken, Object? args, Int32? queryTimeout) =>
			new CommandDefinition(
				sql,
				args,
				npgsqlTransaction,
				queryTimeout ?? defaultQueryTimeout,
				cancellationToken: cancellationToken);

		/// <inheritdoc />
		public async ValueTask ExecuteAsync
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null,
			 Affect affect = Affect.Any) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				var affectedRowsCount = await npgsqlConnection.ExecuteAsync(cmd);
				var assertionFailure =
					affect switch {
						Affect.SingleRow when affectedRowsCount != 1 =>
							$"Expected query to affect single row, but affected {affectedRowsCount} rows.",
						Affect.AtLeastOneRow when affectedRowsCount < 1 =>
							$"Expected query to affect at least one row, but affected {affectedRowsCount} rows.",
						_ =>
							null
					};
				if(assertionFailure is {}) {
					var msg =
						assertionFailure
						+ $"{Environment.NewLine}Connection string: {npgsqlConnection.ConnectionString}"
						+ $"{Environment.NewLine}Query:{Environment.NewLine}"
						+ sql;
					throw new SqlException.AssertionFailure(msg);
				}
			}
			catch(SqlException) {
				throw;
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<Int32> QueryAffectedRowsCount
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.ExecuteAsync(cmd);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<dynamic>> QueryRows
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync(cmd);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<T>> QueryRows<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync<T>(cmd);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, TOutput>
			(String sql,
			 String splitOn, Func<T1, T2, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync(cmd, map, splitOn);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, TOutput>
			(String sql,
			 String splitOn, Func<T1, T2, T3, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync(cmd, map, splitOn);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, TOutput>
			(String sql,
			 String splitOn, Func<T1, T2, T3, T4, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync(cmd, map, splitOn);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, TOutput>
			(String sql,
			 String splitOn, Func<T1, T2, T3, T4, T5, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync(cmd, map, splitOn);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, T6, TOutput>
			(String sql,
			 String splitOn, Func<T1, T2, T3, T4, T5, T6, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync(cmd, map, splitOn);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, T6, T7, TOutput>
			(String sql,
			 String splitOn, Func<T1, T2, T3, T4, T5, T6, T7, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync(cmd, map, splitOn);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<dynamic> QueryFirstRow
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryFirstAsync(cmd);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<T> QueryFirstRow<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryFirstAsync<T>(cmd);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<dynamic> QueryFirstRowOrDefault
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryFirstOrDefaultAsync(cmd);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<T> QueryFirstRowOrDefault<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryFirstOrDefaultAsync<T>(cmd);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<dynamic> QuerySingleRow
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QuerySingleAsync(cmd);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<T> QuerySingleRow<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QuerySingleAsync<T>(cmd);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<dynamic> QuerySingleRowOrDefault
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QuerySingleOrDefaultAsync(cmd);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<T> QuerySingleRowOrDefault<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QuerySingleOrDefaultAsync<T>(cmd);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<dynamic> QueryScalar
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.ExecuteScalarAsync(cmd);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}

		/// <inheritdoc />
		public async ValueTask<T> QueryScalar<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.ExecuteScalarAsync<T>(cmd);
			}
			catch(Exception ex) {
				NpgsqlExceptions.Handle(ex);
				throw;
			}
		}
	}
}
