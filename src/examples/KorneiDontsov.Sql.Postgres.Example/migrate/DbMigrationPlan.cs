namespace KorneiDontsov.Sql.Postgres.Example.DbMigrations {
	using KorneiDontsov.Sql.Migrations;
	using System;

	public class DbMigrationPlan: IDbMigrationPlan {
		/// <inheritdoc />
		public String migrationPlanId => "public";

		/// <inheritdoc />
		public void Configure (IDbMigrationCollection migrations) =>
			migrations
				.Add<CreatePostsTable>()
				.Add<AddHelloWorldPost>()
				.Add<RequirePostAuthor>();
	}
}
