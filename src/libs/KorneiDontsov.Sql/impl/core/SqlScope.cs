﻿namespace KorneiDontsov.Sql {
	using Microsoft.Extensions.DependencyInjection;
	using System;
	using System.Threading.Tasks;

	class SqlScope: IAsyncDisposable {
		public IManagedSqlTransaction? transaction;

		/// <inheritdoc />
		public ValueTask DisposeAsync () =>
			transaction?.DisposeAsync() ?? default;

		public IManagedRwSqlTransaction GetOrCreateRwSqlTransaction (IServiceProvider serviceProvider) {
			switch(transaction) {
				case IManagedRwSqlTransaction rwTransaction:
					return rwTransaction;

				case IManagedRoSqlTransaction _:
					var msg =
						"Failed to begin read-write sql transaction "
						+ "because DI scope already has read-only sql transaction.";
					throw new InvalidOperationException(msg);

				case {} flexTransaction:
					using(NoSyncContext.On())
						flexTransaction.SetRwAsync().Wait();
					var transactionDecorator = new RwSqlTransactionDecorator(flexTransaction);
					transaction = transactionDecorator;
					return transactionDecorator;

				case null:
					var dbProvider = serviceProvider.GetRequiredService<IDbProvider>();
					IManagedRwSqlTransaction newTransaction;
					using(NoSyncContext.On())
						newTransaction = dbProvider.BeginRwSerializable().Wait();
					transaction = newTransaction;
					return newTransaction;
			}
		}

		public IManagedRoSqlTransaction GetOrCreateRoSqlTransaction (IServiceProvider serviceProvider) {
			switch(transaction) {
				case IManagedRoSqlTransaction roTransaction:
					return roTransaction;

				case IManagedRwSqlTransaction _:
					var msg =
						"Failed to begin read-only sql transaction "
						+ "because DI scope already has read-write sql transaction.";
					throw new InvalidOperationException(msg);

				case {} flexTransaction:
					using(NoSyncContext.On())
						flexTransaction.SetRoAsync().Wait();
					var transactionDecorator = new RoSqlTransactionDecorator(flexTransaction);
					transaction = transactionDecorator;
					return transactionDecorator;

				case null:
					var dbProvider = serviceProvider.GetRequiredService<IDbProvider>();
					IManagedRoSqlTransaction newTransaction;
					using(NoSyncContext.On())
						newTransaction = dbProvider.BeginRoSerializable().Wait();
					transaction = newTransaction;
					return newTransaction;
			}
		}

		public IManagedSqlTransaction GetOrCreateSqlTransaction (IServiceProvider serviceProvider) {
			if(transaction is {} theTransaction)
				return theTransaction;
			else {
				var dbProvider = serviceProvider.GetRequiredService<IDbProvider>();
				using(NoSyncContext.On())
					return transaction = dbProvider.BeginSerializable(SqlAccess.Ro).Wait();
			}
		}
	}
}
