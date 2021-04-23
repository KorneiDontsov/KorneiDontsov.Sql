namespace KorneiDontsov.Sql.Migrations {
	using System;

	public interface IDbMigrationPlan {
		/// <summary>
		///     The name of schema that migration system used to store migration data.
		///     Migration system does not create this schema automatically: it should exists in database
		///     before first launch.
		/// </summary>
		String migrationSchema => "public";

		void Configure (IDbMigrationCollection migrations);
	}
}
