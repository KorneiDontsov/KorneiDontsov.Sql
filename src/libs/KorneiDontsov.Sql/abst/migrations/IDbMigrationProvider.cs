namespace KorneiDontsov.Sql.Migrations {
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	public interface IDbMigrationProvider {
		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<IAsyncDisposable> Lock (String migrationPlanId, CancellationToken cancellationToken = default) =>
			new ValueTask<IAsyncDisposable>(AsyncDisposableStub.shared);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<(Int32 index, String id)?> MaybeLastMigrationInfo
			(IRwSqlTransaction transaction, String migrationPlanId, CancellationToken cancellationToken = default);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask SetLastMigrationInfo
			(IRwSqlTransaction transaction,
			 String migrationPlanId,
			 Int32 migrationIndex,
			 String migrationId,
			 CancellationToken cancellationToken = default);
	}
}
