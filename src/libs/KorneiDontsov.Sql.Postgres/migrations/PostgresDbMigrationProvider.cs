namespace KorneiDontsov.Sql.Postgres {
	using KorneiDontsov.Sql.Migrations;
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Threading;
	using System.Threading.Tasks;

	class PostgresDbMigrationProvider: IDbMigrationProvider {
		IDbProvider dbProvider { get; }

		public PostgresDbMigrationProvider (IDbProvider dbProvider) =>
			this.dbProvider = dbProvider;

		ValueTask CreateMigrationsTableIfNotExists
			(ISqlProvider sqlProvider, String schemaName, CancellationToken cancellationToken) {
			var sql =
				"create table if not exists \"{0}\".migrations(index integer not null unique, id text not null unique)";
			return sqlProvider.ExecuteAsync(String.Format(sql, schemaName), cancellationToken);
		}

		/// <inheritdoc />
		public async ValueTask<IAsyncDisposable> Lock
			(String migrationSchema,
			 CancellationToken cancellationToken = default) {
			var schemaName = migrationSchema.ToLower();
			await dbProvider.UsingRwReadCommitted()
				.ExecuteAsync(
					$"create table if not exists \"{schemaName}\".migration_sync()",
					cancellationToken)
				.ConfigureAwait(false);

			var transaction = await dbProvider.BeginRwReadCommitted(cancellationToken).ConfigureAwait(false);
			await transaction.ExecuteAsync(
					$"lock \"{schemaName}\".migration_sync in access exclusive mode",
					cancellationToken)
				.ConfigureAwait(false);
			return transaction;
		}

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		class MigrationRow {
			public Int32 index { get; set; }

			public String id { get; } = null!;
		}

		/// <inheritdoc />
		public async ValueTask<(Int32 index, String id)?> MaybeLastMigrationInfo
			(IRwSqlTransaction transaction, String migrationSchema, CancellationToken cancellationToken = default) {
			var schemaName = migrationSchema.ToLower();

			await CreateMigrationsTableIfNotExists(transaction, schemaName, cancellationToken).ConfigureAwait(false);

			var sql = $"select index, id from \"{schemaName}\".migrations order by index desc limit 1";
			var row =
				await transaction.QueryFirstRowOrDefault<MigrationRow?>(sql, cancellationToken).ConfigureAwait(false);
			if(row is {})
				return (row.index, row.id);
			else
				return null;
		}

		/// <inheritdoc />
		public async ValueTask SetLastMigrationInfo
			(IRwSqlTransaction transaction,
			 String migrationSchema,
			 Int32 migrationIndex,
			 String migrationId,
			 CancellationToken cancellationToken = default) {
			var schemaName = migrationSchema.ToLower();

			await CreateMigrationsTableIfNotExists(transaction, schemaName, cancellationToken).ConfigureAwait(false);

			var sql = $"insert into \"{schemaName}\".migrations(index, id) values (@index, @id)";
			await transaction.ExecuteAsync(sql, cancellationToken, new { index = migrationIndex, id = migrationId })
				.ConfigureAwait(false);
		}
	}
}
