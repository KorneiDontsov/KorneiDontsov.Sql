namespace KorneiDontsov.Sql {
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	public interface IManagedSqlTransaction: ISqlTransaction, IAsyncDisposable {
		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask CommitAsync (CancellationToken cancellationToken = default);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask RollbackAsync (CancellationToken cancellationToken = default);
	}
}
