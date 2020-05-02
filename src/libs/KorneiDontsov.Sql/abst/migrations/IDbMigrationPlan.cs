namespace KorneiDontsov.Sql.Migrations {
	using System;

	public interface IDbMigrationPlan {
		String migrationPlanId { get; }

		void Configure (IDbMigrationCollection migrations);
	}
}
