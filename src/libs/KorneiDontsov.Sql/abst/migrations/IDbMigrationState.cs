namespace KorneiDontsov.Sql.Migrations {
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	public interface IDbMigrationState {
		/// <exception cref = "OperationCanceledException" />
		ValueTask<DbMigrationResult> WhenCompleted (CancellationToken cancellationToken = default);
	}
}
