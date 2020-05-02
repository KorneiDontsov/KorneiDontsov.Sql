namespace KorneiDontsov.Sql.Migrations {
	using System;

	public static class DbMigrationConnectionFunctions {
		public static IDbMigrationCollection Add<TMigration>
			(this IDbMigrationCollection migrations, params Object[] parameters)
			where TMigration: IDbMigration {
			migrations.Add(typeof(TMigration), parameters);
			return migrations;
		}

		public static IDbMigrationCollection Add<TMigration> (this IDbMigrationCollection migrations)
			where TMigration: IDbMigration =>
			migrations.Add<TMigration>(parameters: Array.Empty<Object>());
	}
}
