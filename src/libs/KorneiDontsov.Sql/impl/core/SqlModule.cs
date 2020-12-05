namespace KorneiDontsov.Sql {
	using Microsoft.AspNetCore.Builder;
	using Microsoft.Extensions.DependencyInjection;
	using System;
	using System.Data;

	public static class SqlModule {
		const IsolationLevel defaultIsolationLevel = IsolationLevel.ReadCommitted;

		static IServiceCollection AddSqlScoped<TService>
			(this IServiceCollection services, Func<SqlScope, TService> create) where TService: class =>
			services.AddScoped<TService?>(
				sp => sp.GetService<SqlScope>() is {} sqlScope ? create(sqlScope) : null);

		public static IServiceCollection AddSql<TDbProvider> (this IServiceCollection services)
			where TDbProvider: class, IDbProvider =>
			services.AddSingleton<IDbProvider, TDbProvider>()
				.AddSingleton<ISqlProvider>(
					sp => sp.GetRequiredService<IDbProvider>().Using(defaultIsolationLevel, SqlAccess.Rw))
				.AddSingleton<SqlMiddleware>()
				.AddScoped<SqlScope>()
				.AddSqlScoped<IRwSqlTransaction>(
					sqlScope => sqlScope.GetOrCreateOrUpgradeRwSqlTransaction(defaultIsolationLevel).Wait())
				.AddSqlScoped<IRoSqlTransaction>(
					sqlScope =>
						sqlScope.GetOrCreateOrUpgradeRoSqlTransaction(defaultIsolationLevel).Wait())
				.AddSqlScoped<ISqlTransaction>(
					sqlScope => sqlScope.GetOrCreateOrUpgradeSqlTransaction(defaultIsolationLevel).Wait());

		public static IApplicationBuilder UseSql (this IApplicationBuilder webApp) =>
			webApp.UseMiddleware<SqlMiddleware>();
	}
}
