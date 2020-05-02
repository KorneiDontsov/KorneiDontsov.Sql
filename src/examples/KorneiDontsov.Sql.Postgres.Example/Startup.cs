namespace KorneiDontsov.Sql.Postgres.Example {
	using KorneiDontsov.Sql.Migrations;
	using KorneiDontsov.Sql.Postgres.Example.DbMigrations;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;

	public class Startup {
		public void ConfigureServices (IServiceCollection services) {
			services
				.AddPostgres()
				.AddPostgresMigration<DbMigrationPlan>();
			services.AddControllers();
			services.AddHealthChecks();
		}

		public void Configure (IApplicationBuilder webApp, IWebHostEnvironment env) {
			if(env.IsDevelopment()) webApp.UseDeveloperExceptionPage();

			webApp
				.UseRouting()
				.UseSql()
				.UseEndpoints(
					endpoints => {
						endpoints.MapControllers();
						endpoints.MapHealthChecks("/health").WithoutDbMigration();
						endpoints.MapDbMigrationResultLongPolls("/db-migration-result");
					});
		}
	}
}
