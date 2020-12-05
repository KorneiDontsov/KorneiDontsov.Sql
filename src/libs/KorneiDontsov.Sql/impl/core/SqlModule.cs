namespace KorneiDontsov.Sql {
	using Microsoft.AspNetCore.Builder;
	using Microsoft.Extensions.DependencyInjection;

	public static class SqlModule {
		public static IServiceCollection AddSql<TDbProvider> (this IServiceCollection services)
			where TDbProvider: class, IDbProvider =>
			services.AddSingleton<IDbProvider, TDbProvider>()
				.AddSingleton<ISqlProvider>(
					sp => sp.GetRequiredService<IDbProvider>().UsingRwReadCommitted())
				.AddScoped<SqlScope>()
				.AddScoped<SqlMiddleware>()
				.AddScoped<IRwSqlTransaction>(
					serviceProvider => {
						var scope = serviceProvider.GetRequiredService<SqlScope>();
						return scope.transaction as IManagedRwSqlTransaction
						       ?? scope.GetOrCreateRwSqlTransaction(serviceProvider);
					})
				.AddScoped<IRoSqlTransaction>(
					serviceProvider => {
						var scope = serviceProvider.GetRequiredService<SqlScope>();
						return scope.transaction as IManagedRoSqlTransaction
						       ?? scope.GetOrCreateRoSqlTransaction(serviceProvider);
					})
				.AddScoped<ISqlTransaction>(
					serviceProvider => {
						var scope = serviceProvider.GetRequiredService<SqlScope>();
						return scope.transaction ?? scope.GetOrCreateSqlTransaction(serviceProvider);
					});

		public static IApplicationBuilder UseSql (this IApplicationBuilder webApp) =>
			webApp.UseMiddleware<SqlMiddleware>();
	}
}
