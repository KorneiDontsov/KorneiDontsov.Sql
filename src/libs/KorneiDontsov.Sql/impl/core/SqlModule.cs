namespace KorneiDontsov.Sql {
	using Microsoft.AspNetCore.Builder;
	using Microsoft.Extensions.DependencyInjection;
	using System;

	public static class SqlModule {
		public static IServiceCollection AddSql<TDbProvider> (this IServiceCollection services)
			where TDbProvider: class, IDbProvider =>
			services
				.AddSingleton<IDbProvider, TDbProvider>()
				.AddSingleton<ISqlProvider>(
					sp => sp.GetRequiredService<IDbProvider>().UsingRwReadCommitted())
				.AddScoped<SqlScopeState>()
				.AddScoped<SqlMiddleware>()
				.AddScoped<IRwSqlTransaction>(
					sp => {
						var services = sp.GetRequiredService<SqlScopeState>();
						switch(services.transaction) {
							case null:
								var dbProvider = sp.GetRequiredService<IDbProvider>();
								var newTransaction = dbProvider.BeginRwSerializable().GetAwaiter().GetResult();
								services.transaction = newTransaction;
								return newTransaction;

							case IManagedRwSqlTransaction rwTransaction:
								return rwTransaction;

							case IManagedRoSqlTransaction _:
								var msg =
									"Failed to begin read-write sql transaction "
									+ "because DI scope already has read-only sql transaction.";
								throw new InvalidOperationException(msg);

							case {} transaction:
								transaction.SetRwAsync().AsTask().GetAwaiter().GetResult();
								var transactionDecorator = new RwSqlTransactionDecorator(transaction);
								services.transaction = transactionDecorator;
								return transactionDecorator;
						}
					})
				.AddScoped<IRoSqlTransaction>(
					sp => {
						var services = sp.GetRequiredService<SqlScopeState>();
						switch(services.transaction) {
							case null:
								var dbProvider = sp.GetRequiredService<IDbProvider>();
								var newTransaction = dbProvider.BeginRoSerializable().GetAwaiter().GetResult();
								services.transaction = newTransaction;
								return newTransaction;

							case IManagedRoSqlTransaction roTransaction:
								return roTransaction;

							case IManagedRwSqlTransaction _:
								var msg =
									"Failed to begin read-only sql transaction "
									+ "because DI scope already has read-write sql transaction.";
								throw new InvalidOperationException(msg);

							case {} transaction:
								transaction.SetRoAsync().AsTask().GetAwaiter().GetResult();
								var transactionDecorator = new RoSqlTransactionDecorator(transaction);
								services.transaction = transactionDecorator;
								return transactionDecorator;
						}
					})
				.AddScoped<ISqlTransaction>(
					sp => {
						var services = sp.GetRequiredService<SqlScopeState>();
						if(services.transaction is {} transaction)
							return transaction;
						else {
							var dbProvider = sp.GetRequiredService<IDbProvider>();
							var newTransaction = dbProvider.BeginSerializable(SqlAccess.Ro).GetAwaiter().GetResult();
							services.transaction = newTransaction;
							return newTransaction;
						}
					});

		public static IApplicationBuilder UseSql (this IApplicationBuilder webApp) =>
			webApp.UseMiddleware<SqlMiddleware>();
	}
}
