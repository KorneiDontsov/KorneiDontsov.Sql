namespace KorneiDontsov.Sql.Postgres.Example {
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;
	using System;

	public static class Program {
		public static void Main (String[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging(log => log.SetMinimumLevel(LogLevel.Information).AddConsole())
				.ConfigureWebHostDefaults(web => web.UseStartup<Startup>())
				.Build()
				.Run();
	}
}
