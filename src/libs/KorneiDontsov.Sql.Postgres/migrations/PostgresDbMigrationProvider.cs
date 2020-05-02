namespace KorneiDontsov.Sql.Postgres {
	using JetBrains.Annotations;
	using KorneiDontsov.Sql.Migrations;
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	class PostgresDbMigrationProvider: IDbMigrationProvider {
		IDbProvider dbProvider { get; }

		public PostgresDbMigrationProvider (IDbProvider dbProvider) =>
			this.dbProvider = dbProvider;

		ValueTask CreateMigrationsTableIfNotExists
			(ISqlProvider sqlProvider, String schemaName, CancellationToken cancellationToken) {
			var sql =
				"create schema if not exists \"{0}\";" +
				"create table if not exists \"{0}\".migrations(index integer not null unique, id text not null unique)";
			return sqlProvider.ExecuteAsync(String.Format(sql, schemaName), cancellationToken);
		}

		/// <inheritdoc />
		public async ValueTask<IAsyncDisposable> Lock
			(String migrationPlanId,
			 CancellationToken cancellationToken = default) {
			var schemaName = migrationPlanId.ToLower();
			await dbProvider.UsingRwReadCommitted().ExecuteAsync(
				$"create table if not exists \"{schemaName}\".migration_sync()",
				cancellationToken);

			var transaction = await dbProvider.BeginRwReadCommitted(cancellationToken);
			await transaction.ExecuteAsync(
				$"lock \"{schemaName}\".migration_sync in access exclusive mode",
				cancellationToken);
			return transaction;
		}

		[UsedImplicitly(ImplicitUseTargetFlags.Members)]
		class MigrationRow {
			public Int32 index { get; set; }

			public String id { get; } = null!;
		}

		/// <inheritdoc />
		public async ValueTask<(Int32 index, String id)?> MaybeLastMigrationInfo
			(IRwSqlTransaction transaction, String migrationPlanId, CancellationToken cancellationToken = default) {
			var schemaName = migrationPlanId.ToLower();

			await CreateMigrationsTableIfNotExists(transaction, schemaName, cancellationToken);

			var sql = $"select index, id from \"{schemaName}\".migrations order by index desc limit 1";
			if(await transaction.QueryFirstRowOrDefault<MigrationRow?>(sql, cancellationToken) is {} row)
				return (row.index, row.id);
			else
				return null;
		}

		/// <inheritdoc />
		public async ValueTask SetLastMigrationInfo
			(IRwSqlTransaction transaction,
			 String migrationPlanId,
			 Int32 migrationIndex,
			 String migrationId,
			 CancellationToken cancellationToken = default) {
			var schemaName = migrationPlanId.ToLower();

			await CreateMigrationsTableIfNotExists(transaction, schemaName, cancellationToken);

			var sql = $"insert into \"{schemaName}\".migrations(index, id) values (@index, @id)";
			await transaction.ExecuteAsync(sql, cancellationToken, new { index = migrationIndex, id = migrationId });
		}
	}
}
