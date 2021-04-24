namespace KorneiDontsov.Sql.Postgres {
	using KorneiDontsov.Sql.Migrations;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.DependencyInjection.Extensions;

	public static class Module {
		public static IServiceCollection AddPostgres (this IServiceCollection services) {
			services.TryAddSingleton<PostgresDbProviderSettings>();
			return services.AddSql<PostgresDbProvider>();
		}

		public static IServiceCollection AddPostgresMigration<TPlan> (this IServiceCollection services)
			where TPlan: class, IDbMigrationPlan =>
			services.AddDbMigration<PostgresDbMigrationProvider, TPlan>();
	}
}
