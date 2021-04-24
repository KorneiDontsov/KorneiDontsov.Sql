namespace KorneiDontsov.Sql.Postgres.Tests {
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Configuration.Json;
	using Microsoft.Extensions.FileProviders;
	using System;

	static class App {
		public static IConfiguration configuration { get; } =
			new ConfigurationRoot(
				new IConfigurationProvider[] {
					new JsonConfigurationProvider(
						new() {
							FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory),
							Path = "appsettings.json",
							Optional = false,
							ReloadOnChange = false
						})
				});
	}
}
