namespace KorneiDontsov.Sql.Postgres.Example.DbMigrations {
	using KorneiDontsov.Sql.Migrations;
	using System.Threading;
	using System.Threading.Tasks;

	public class AddHelloWorldPost: IDbMigration {
		/// <inheritdoc />
		public async ValueTask Exec (IRwSqlTransaction transaction, CancellationToken cancellationToken) {
			var sql = "insert into posts(content) values ('Hello, world!')";
			await transaction.ExecuteAsync(sql, cancellationToken);

			// simulate that migration takes time.
			await Task.Delay(1000, cancellationToken);
		}
	}
}
