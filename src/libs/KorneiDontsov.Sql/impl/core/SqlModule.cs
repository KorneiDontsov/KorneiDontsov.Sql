namespace KorneiDontsov.Sql {
	using Microsoft.AspNetCore.Builder;
	using Microsoft.Extensions.DependencyInjection;
	using System.Data;

	public static class SqlModule {
		const IsolationLevel defaultIsolationLevel = IsolationLevel.ReadCommitted;

		public static IServiceCollection AddSql<TDbProvider> (this IServiceCollection services)
			where TDbProvider: class, IDbProvider =>
			services.AddSingleton<IDbProvider, TDbProvider>()
				.AddSingleton<ISqlProvider>(
					sp => sp.GetRequiredService<IDbProvider>().Using(defaultIsolationLevel, SqlAccess.Rw))
				.AddScoped<SqlScope>()
				.AddScoped<SqlMiddleware>()
				.AddScoped<IRwSqlTransaction>(
					serviceProvider => {
						var scope = serviceProvider.GetRequiredService<SqlScope>();
						return scope.transaction as IManagedRwSqlTransaction
						       ?? scope.GetOrCreateRwSqlTransaction(serviceProvider, defaultIsolationLevel);
					})
				.AddScoped<IRoSqlTransaction>(
					serviceProvider => {
						var scope = serviceProvider.GetRequiredService<SqlScope>();
						return scope.transaction as IManagedRoSqlTransaction
						       ?? scope.GetOrCreateRoSqlTransaction(serviceProvider, defaultIsolationLevel);
					})
				.AddScoped<ISqlTransaction>(
					serviceProvider => {
						var scope = serviceProvider.GetRequiredService<SqlScope>();
						return scope.transaction
						       ?? scope.GetOrCreateSqlTransaction(serviceProvider, defaultIsolationLevel);
					});

		public static IApplicationBuilder UseSql (this IApplicationBuilder webApp) =>
			webApp.UseMiddleware<SqlMiddleware>();
	}
}
