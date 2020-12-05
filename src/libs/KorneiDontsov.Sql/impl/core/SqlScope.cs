namespace KorneiDontsov.Sql {
	using System;
	using System.Data;
	using System.Threading;
	using System.Threading.Tasks;

	sealed class SqlScope: IAsyncDisposable {
		readonly IDbProvider dbProvider;

		public IManagedSqlTransaction? transaction;

		public SqlScope (IDbProvider dbProvider) => this.dbProvider = dbProvider;

		/// <inheritdoc />
		public ValueTask DisposeAsync () =>
			transaction?.DisposeAsync() ?? default;

		async ValueTask<IManagedRwSqlTransaction> CreateRwSqlTransaction
			(IsolationLevel isolationLevel, CancellationToken ct = default) {
			var newTransaction = await dbProvider.BeginRw(isolationLevel, ct).ConfigureAwait(false);
			transaction = newTransaction;
			return newTransaction;
		}

		async ValueTask<IManagedRoSqlTransaction> CreateRoSqlTransaction
			(IsolationLevel isolationLevel, CancellationToken ct = default) {
			var newTransaction = await dbProvider.BeginRo(isolationLevel, ct).ConfigureAwait(false);
			transaction = newTransaction;
			return newTransaction;
		}

		async ValueTask<IManagedSqlTransaction> CreateSqlTransaction
			(IsolationLevel isolationLevel, CancellationToken ct = default) {
			var newTransaction = await dbProvider.Begin(SqlAccess.Ro, isolationLevel, ct).ConfigureAwait(false);
			transaction = newTransaction;
			return newTransaction;
		}

		async ValueTask<IManagedRwSqlTransaction> UpgradeToRwSqlTransaction
			(IManagedSqlTransaction baseTransaction, CancellationToken ct = default) {
			await baseTransaction.SetRwAsync(ct).ConfigureAwait(false);
			var upgradedTransaction = new RwSqlTransactionDecorator(baseTransaction);
			transaction = upgradedTransaction;
			return upgradedTransaction;
		}

		async ValueTask<IManagedRoSqlTransaction> UpgradeToRoSqlTransaction
			(IManagedSqlTransaction baseTransaction, CancellationToken ct = default) {
			await baseTransaction.SetRoAsync(ct).ConfigureAwait(false);
			var upgradedTransaction = new RoSqlTransactionDecorator(baseTransaction);
			transaction = upgradedTransaction;
			return upgradedTransaction;
		}

		public ValueTask<IManagedRwSqlTransaction> GetOrCreateOrUpgradeRwSqlTransaction
			(IsolationLevel isolationLevel, CancellationToken ct = default) =>
			transaction switch {
				IManagedRwSqlTransaction rwTransaction =>
					new ValueTask<IManagedRwSqlTransaction>(rwTransaction),
				null =>
					CreateRwSqlTransaction(isolationLevel, ct),
				IManagedRoSqlTransaction =>
					throw new InvalidOperationException("Not allowed to change read-only access to read-write."),
				{} baseTransaction =>
					UpgradeToRwSqlTransaction(baseTransaction, ct)
			};

		public ValueTask<IManagedRoSqlTransaction> GetOrCreateOrUpgradeRoSqlTransaction
			(IsolationLevel isolationLevel, CancellationToken ct = default) =>
			transaction switch {
				IManagedRoSqlTransaction roTransaction =>
					new ValueTask<IManagedRoSqlTransaction>(roTransaction),
				null =>
					CreateRoSqlTransaction(isolationLevel, ct),
				IManagedRwSqlTransaction =>
					throw new InvalidOperationException("Not allowed to change read-write access to read-only."),
				{} baseTransaction =>
					UpgradeToRoSqlTransaction(baseTransaction, ct)
			};

		public ValueTask<IManagedSqlTransaction> GetOrCreateSqlTransaction
			(IsolationLevel isolationLevel, CancellationToken ct = default) =>
			transaction switch {
				{} anyTransaction => new ValueTask<IManagedSqlTransaction>(anyTransaction),
				null => CreateSqlTransaction(isolationLevel, ct)
			};
	}
}
