namespace KorneiDontsov.Sql.Migrations {
	using System.Threading;

	public interface IDbMigrationState {
		/// <summary>
		///     Result of database migration if it completed; otherwise, null.
		/// </summary>
		DbMigrationResult? result { get; }

		/// <summary>
		///     Triggers when database migration completes.
		/// </summary>
		CancellationToken onCompleted { get; }
	}
}
