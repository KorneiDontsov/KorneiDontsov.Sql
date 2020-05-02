namespace KorneiDontsov.Sql.Postgres {
	using KorneiDontsov.Sql.Migrations;
	using Microsoft.Extensions.DependencyInjection;

	public static class Module {
		public static IServiceCollection AddPostgres (this IServiceCollection services) =>
			services.AddSql<PostgresDbProvider>()
				.AddSingleton<PostgresDbProviderSettings>();

		public static IServiceCollection AddPostgresMigration<TPlan> (this IServiceCollection services)
			where TPlan: class, IDbMigrationPlan =>
			services.AddDbMigration<PostgresDbMigrationProvider, TPlan>();
	}
}
