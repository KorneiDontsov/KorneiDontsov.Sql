namespace KorneiDontsov.Sql.Migrations {
	using Microsoft.Extensions.DependencyInjection;

	public static class DbMigrationModule {
		public static IServiceCollection AddDbMigration
			<TProvider, TPlan> (this IServiceCollection services)
			where TProvider: class, IDbMigrationProvider
			where TPlan: class, IDbMigrationPlan =>
			services
				.AddTransient<IDbMigrationProvider, TProvider>()
				.AddTransient<IDbMigrationPlan, TPlan>()
				.AddSingleton<DbMigrationState>()
				.AddSingleton<IDbMigrationState>(sp => sp.GetRequiredService<DbMigrationState>())
				.AddHostedService<DbMigrationService>();
	}
}
