namespace KorneiDontsov.Sql.Postgres {
	using Dapper;
	using Npgsql;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Common;
	using System.Diagnostics.CodeAnalysis;
	using System.Runtime.CompilerServices;
	using System.Threading;
	using System.Threading.Tasks;

	class PostgresConnection: ISqlProvider, IAsyncDisposable {
		NpgsqlConnection npgsqlConnection { get; }
		NpgsqlTransaction? npgsqlTransaction { get; }

		/// <inheritdoc />
		public IsolationLevel initialIsolationLevel { get; }

		/// <inheritdoc />
		public SqlAccess? initialAccess { get; }

		/// <inheritdoc />
		public Int32 defaultQueryTimeout { get; }

		public PostgresConnection
			(NpgsqlConnection npgsqlConnection,
			 NpgsqlTransaction? npgsqlTransaction,
			 IsolationLevel initialIsolationLevel,
			 SqlAccess? initialAccess,
			 Int32 defaultQueryTimeout) {
			this.npgsqlConnection = npgsqlConnection;
			this.npgsqlTransaction = npgsqlTransaction;
			this.initialIsolationLevel = initialIsolationLevel;
			this.initialAccess = initialAccess;
			this.defaultQueryTimeout = defaultQueryTimeout;
		}

		protected virtual ValueTask DoDisposeAsync () =>
			npgsqlConnection.DisposeAsync();

		volatile Int32 isDisposedFlag;

		/// <inheritdoc />
		public ValueTask DisposeAsync () =>
			Interlocked.CompareExchange(ref isDisposedFlag, 1, 0) is 0 ? DoDisposeAsync() : default;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void MustNotBeDisposed () {
			if(isDisposedFlag is not 0) {
				[MethodImpl(MethodImplOptions.NoInlining)]
				static void ThrowDisposed () =>
					throw new ObjectDisposedException(nameof(PostgresTransaction));

				ThrowDisposed();
			}
		}

		CommandDefinition CreateCommand
			(String sql, CancellationToken cancellationToken, Object? args, Int32? queryTimeout) =>
			new(
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
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				var affectedRowsCount = await npgsqlConnection.ExecuteAsync(cmd).ConfigureAwait(false);
				var assertionFailure =
					affect switch {
						Affect.SingleRow when affectedRowsCount != 1 =>
							$"Expected query to affect single row, but affected {affectedRowsCount} rows.",
						Affect.AtLeastOneRow when affectedRowsCount < 1 =>
							$"Expected query to affect at least one row, but affected {affectedRowsCount} rows.",
						_ =>
							null
					};
				if(assertionFailure is { }) {
					var msg =
						assertionFailure
						+ $"{Environment.NewLine}Connection string: {npgsqlConnection.ConnectionString}"
						+ $"{Environment.NewLine}Query:{Environment.NewLine}"
						+ sql;
					throw new SqlException.AssertionFailure(msg);
				}
			}
			catch(SqlException) { throw; }
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<Int32> QueryAffectedRowsCount
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.ExecuteAsync(cmd).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<dynamic>> QueryRows
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync(cmd).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<T>> QueryRows<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync<T>(cmd).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, TOutput>
			(String sql,
			 String splitOn, Func<T1, T2, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync(cmd, map, splitOn).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, TOutput>
			(String sql,
			 String splitOn, Func<T1, T2, T3, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync(cmd, map, splitOn).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, TOutput>
			(String sql,
			 String splitOn, Func<T1, T2, T3, T4, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync(cmd, map, splitOn).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, TOutput>
			(String sql,
			 String splitOn, Func<T1, T2, T3, T4, T5, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync(cmd, map, splitOn).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, T6, TOutput>
			(String sql,
			 String splitOn, Func<T1, T2, T3, T4, T5, T6, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync(cmd, map, splitOn).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, T6, T7, TOutput>
			(String sql,
			 String splitOn, Func<T1, T2, T3, T4, T5, T6, T7, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryAsync(cmd, map, splitOn).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<dynamic> QueryFirstRow
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryFirstAsync(cmd).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<T> QueryFirstRow<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryFirstAsync<T>(cmd).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<dynamic?> QueryFirstRowOrDefault
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryFirstOrDefaultAsync(cmd).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		[return: MaybeNull]
		public async ValueTask<T> QueryFirstRowOrDefault<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QueryFirstOrDefaultAsync<T>(cmd).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<dynamic> QuerySingleRow
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QuerySingleAsync(cmd).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<T> QuerySingleRow<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QuerySingleAsync<T>(cmd).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<dynamic?> QuerySingleRowOrDefault
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QuerySingleOrDefaultAsync(cmd).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		[return: MaybeNull]
		public async ValueTask<T> QuerySingleRowOrDefault<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.QuerySingleOrDefaultAsync<T>(cmd).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<dynamic> QueryScalar
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.ExecuteScalarAsync(cmd).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public async ValueTask<T> QueryScalar<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			MustNotBeDisposed();

			var cmd = CreateCommand(sql, cancellationToken, args, queryTimeout);
			try {
				return await npgsqlConnection.ExecuteScalarAsync<T>(cmd).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
		}

		/// <inheritdoc />
		public TConnection GetConnection<TConnection> () where TConnection: class {
			if(typeof(TConnection) == typeof(NpgsqlConnection)
			   || typeof(TConnection) == typeof(DbConnection)
			   || typeof(TConnection) == typeof(IDbConnection))
				return Unsafe.As<TConnection>(npgsqlConnection);
			else
				throw new NotSupportedException(
					$"{typeof(TConnection)} is not known. Only {typeof(NpgsqlConnection)} is supported.");
		}
	}
}
