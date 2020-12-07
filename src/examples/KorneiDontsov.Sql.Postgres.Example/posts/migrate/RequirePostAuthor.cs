namespace KorneiDontsov.Sql.Postgres.Example.Posts {
	using KorneiDontsov.Sql.Migrations;
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	[MigrationPretestAlwaysFails]
	public class RequirePostAuthor: IDbMigration, ISqlTest {
		/// <inheritdoc />
		public async ValueTask<String?> Test (ISqlTransaction transaction, CancellationToken cancellationToken) {
			var sql =
				@"select jsonb_agg(jsonb_build_object('timestamp', timestamp, 'content', content))
				  from posts
				  where author is null";
			return await transaction.QueryFirstRowOrDefault<String?>(sql, cancellationToken);
		}

		/// <inheritdoc />
		public async ValueTask Exec (IRwSqlTransaction transaction, CancellationToken cancellationToken) {
			var sql =
				@"update posts set author = 'unknown' where author is null;
				  alter table posts alter author set not null";
			await transaction.ExecuteAsync(sql, cancellationToken);
		}
	}
}
