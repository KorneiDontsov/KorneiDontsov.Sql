namespace KorneiDontsov.Sql.Postgres {
	using Dapper;
	using Npgsql;
	using System;
	using System.Collections.Generic;
	using System.Collections.Immutable;
	using System.Data;
	using System.Data.Common;
	using System.Diagnostics.CodeAnalysis;
	using System.Runtime.CompilerServices;
	using System.Threading;
	using System.Threading.Tasks;
	using static System.Threading.Interlocked;

	class PostgresTransaction: IManagedSqlTransaction {
		NpgsqlConnection npgsqlConnection { get; }

		NpgsqlTransaction npgsqlTransaction { get; }

		/// <inheritdoc />
		public IsolationLevel initialIsolationLevel { get; }

		/// <inheritdoc />
		public SqlAccess? initialAccess { get; }

		/// <inheritdoc />
		public Int32 defaultQueryTimeout { get; }

		public PostgresTransaction
			(NpgsqlConnection npgsqlConnection,
			 NpgsqlTransaction npgsqlTransaction,
			 IsolationLevel initialIsolationLevel,
			 SqlAccess? initialAccess,
			 Int32 defaultQueryTimeout) {
			this.npgsqlConnection = npgsqlConnection;
			this.npgsqlTransaction = npgsqlTransaction;
			this.initialIsolationLevel = initialIsolationLevel;
			this.initialAccess = initialAccess;
			this.defaultQueryTimeout = defaultQueryTimeout;
		}

		volatile Int32 isDisposedFlag;

		/// <inheritdoc />
		public async ValueTask DisposeAsync () {
			if(CompareExchange(ref isDisposedFlag, 1, 0) is 0) {
				await npgsqlTransaction.DisposeAsync().ConfigureAwait(false);
				await npgsqlConnection.DisposeAsync().ConfigureAwait(false);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		void ThrowDisposed () =>
			throw new ObjectDisposedException(nameof(PostgresTransaction));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void MustNotBeDisposed () {
			if(isDisposedFlag != 0) ThrowDisposed();
		}

		SpinLock commitLock = new SpinLock();
		Boolean isCommitted;
		ImmutableArray<Delegate> onCommittedActions = ImmutableArray<Delegate>.Empty;

		[MethodImpl(MethodImplOptions.NoInlining)]
		void ThrowAlreadyCommitted () =>
			throw new InvalidOperationException("Already committed.");

		void OnDoCommitted (Delegate action) {
			var locked = false;
			try {
				commitLock.Enter(ref locked);

				if(isCommitted) ThrowAlreadyCommitted();
				onCommittedActions = onCommittedActions.Add(action);
			}
			finally {
				if(locked) commitLock.Exit();
			}
		}

		/// <inheritdoc />
		public void OnCommitted (Action action) {
			MustNotBeDisposed();
			OnDoCommitted(action);
		}

		/// <inheritdoc />
		public void OnCommitted (Func<ValueTask> action) {
			MustNotBeDisposed();
			OnDoCommitted(action);
		}

		static ValueTask InvokeAction (Delegate action) {
			if(action is Action syncAction) {
				syncAction();
				return default;
			}
			else
				return Unsafe.As<Func<ValueTask>>(action)();
		}

		/// <inheritdoc />
		public async ValueTask CommitAsync (CancellationToken cancellationToken = default) {
			MustNotBeDisposed();

			try {
				await npgsqlTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }

			var locked = false;
			try {
				commitLock.Enter(ref locked);

				isCommitted = true;
			}
			finally {
				if(locked) commitLock.Exit();
			}

			try {
				foreach(var onCommittedAction in onCommittedActions)
					await InvokeAction(onCommittedAction).ConfigureAwait(false);
			}
			catch(Exception ex) { throw new SqlException.AfterCommitFailure(innerException: ex); }
		}

		/// <inheritdoc />
		public async ValueTask RollbackAsync (CancellationToken cancellationToken = default) {
			MustNotBeDisposed();

			try {
				await npgsqlTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
			}
			catch(Exception ex) when(NpgsqlExceptions.MatchToSqlException(ex) is { } sqlEx) { throw sqlEx; }
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
