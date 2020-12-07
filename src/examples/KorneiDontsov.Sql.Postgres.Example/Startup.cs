namespace KorneiDontsov.Sql.Postgres.Example {
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;

	public class Startup {
		public void ConfigureServices (IServiceCollection services) {
			services.AddPostgres().AddPostgresMigration<DbMigrationPlan>();
			services.AddControllers();
			services.AddHealthChecks();
		}

		public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
			if(env.IsDevelopment()) app.UseDeveloperExceptionPage();

			app.UseRouting()
				.UseSql()
				.UseEndpoints(
					endpoints => {
						endpoints.MapControllers();
						endpoints.MapHealthChecks("/health").WithoutDbMigration();
					});
		}
	}
}
