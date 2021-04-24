namespace KorneiDontsov.Sql.Postgres.Example {
	using KorneiDontsov.Sql.Migrations;
	using KorneiDontsov.Sql.Postgres.Example.Posts;

	public class DbMigrationPlan: IDbMigrationPlan {
		public void Configure (IDbMigrationCollection migrations) =>
			migrations
				.Add<CreatePostsTable>()
				.Add("posts/migrate/AddHelloWorldPost.sql")
				.Add<RequirePostAuthor>();
	}
}
