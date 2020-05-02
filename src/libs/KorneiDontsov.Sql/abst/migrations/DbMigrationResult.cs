namespace KorneiDontsov.Sql.Migrations {
	using System;

	public abstract class DbMigrationResult {
		DbMigrationResult () { }

		public sealed class Succeeded: DbMigrationResult { }

		public static Succeeded succeeded { get; } = new Succeeded();

		public sealed class Failed: DbMigrationResult {
			public String info { get; }

			public Failed (String info) =>
				this.info = info;
		}

		public sealed class Canceled: DbMigrationResult { }

		public static Canceled canceled { get; } = new Canceled();
	}
}
