namespace KorneiDontsov.Sql.Postgres {
	using Npgsql;
	using System;
	using System.Collections.Immutable;
	using System.Data;
	using System.Runtime.CompilerServices;
	using System.Threading;
	using System.Threading.Tasks;

	class PostgresTransaction: PostgresConnection, IManagedSqlTransaction {
		NpgsqlTransaction npgsqlTransaction { get; }

		public PostgresTransaction
			(NpgsqlConnection npgsqlConnection,
			 NpgsqlTransaction npgsqlTransaction,
			 IsolationLevel initialIsolationLevel,
			 SqlAccess? initialAccess,
			 Int32 defaultQueryTimeout)
			: base(npgsqlConnection, npgsqlTransaction, initialIsolationLevel, initialAccess, defaultQueryTimeout) =>
			this.npgsqlTransaction = npgsqlTransaction;

		protected async override sealed ValueTask DoDisposeAsync () {
			await npgsqlTransaction.DisposeAsync().ConfigureAwait(false);
			await base.DoDisposeAsync().ConfigureAwait(false);
		}

		SpinLock commitLock = new();
		Boolean isCommitted;
		ImmutableArray<Delegate> onCommittedActions = ImmutableArray<Delegate>.Empty;

		void OnDoCommitted (Delegate action) {
			var locked = false;
			try {
				commitLock.Enter(ref locked);

				if(isCommitted) {
					[MethodImpl(MethodImplOptions.NoInlining)]
					static void ThrowAlreadyCommitted () =>
						throw new InvalidOperationException("Already committed.");

					ThrowAlreadyCommitted();
				}
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
				foreach(var onCommittedAction in onCommittedActions) {
					[MethodImpl(MethodImplOptions.AggressiveInlining)]
					static ValueTask InvokeAction (Delegate action) {
						if(action is Action syncAction) {
							syncAction();
							return default;
						}
						else
							return Unsafe.As<Func<ValueTask>>(action)();
					}

					await InvokeAction(onCommittedAction).ConfigureAwait(false);
				}
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
	}
}
