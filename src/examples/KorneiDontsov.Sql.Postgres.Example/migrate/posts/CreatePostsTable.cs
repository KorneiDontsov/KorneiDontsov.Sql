namespace KorneiDontsov.Sql.Postgres.Example.DbMigrations {
	using KorneiDontsov.Sql.Migrations;
	using System.Threading;
	using System.Threading.Tasks;

	public class CreatePostsTable: IDbMigration {
		/// <inheritdoc />
		public async ValueTask Exec (IRwSqlTransaction transaction, CancellationToken cancellationToken) {
			var sql = "create table posts(author text, content text not null, timestamp timestamptz default now())";
			await transaction.ExecuteAsync(sql, cancellationToken);
		}
	}
}
