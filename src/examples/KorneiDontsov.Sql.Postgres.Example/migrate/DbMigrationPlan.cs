namespace KorneiDontsov.Sql.Postgres.Example.DbMigrations {
	using KorneiDontsov.Sql.Migrations;

	public class DbMigrationPlan: IDbMigrationPlan {
		/// <inheritdoc />
		public void Configure (IDbMigrationCollection migrations) =>
			migrations
				.Add<CreatePostsTable>()
				.Add<AddHelloWorldPost>()
				.Add<RequirePostAuthor>();
	}
}
