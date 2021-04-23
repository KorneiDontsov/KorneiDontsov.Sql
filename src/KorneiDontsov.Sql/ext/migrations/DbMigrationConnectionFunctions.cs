namespace KorneiDontsov.Sql.Migrations {
	using Arcanum.Routes;
	using System;

	public static class DbMigrationConnectionFunctions {
		public static IDbMigrationCollection Add<TMigration>
			(this IDbMigrationCollection migrations, params Object[] parameters) where TMigration: IDbMigration {
			migrations.Add(new DbMigrationPresenter.Class(typeof(TMigration), parameters));
			return migrations;
		}

		public static IDbMigrationCollection Add<TMigration> (this IDbMigrationCollection migrations)
			where TMigration: IDbMigration {
			migrations.Add(new DbMigrationPresenter.Class(typeof(TMigration)));
			return migrations;
		}

		/// <param name = "scriptLocation"> Location of the script in the project. Must be in format '*.sql'. </param>
		/// <param name = "migrationId"> Migration id. By default it's the script name. </param>
		public static IDbMigrationCollection Add
			(this IDbMigrationCollection migrations,
			 Route scriptLocation,
			 String? locationNamespace = null,
			 String? migrationId = null) {
			migrations.Add(new DbMigrationPresenter.EmbeddedScript(scriptLocation, locationNamespace, migrationId));
			return migrations;
		}
	}
}
