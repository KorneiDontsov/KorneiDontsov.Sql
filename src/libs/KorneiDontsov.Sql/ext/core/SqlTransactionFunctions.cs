namespace KorneiDontsov.Sql {
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	public static class SqlTransactionFunctions {
		const String setRwSql = "set transaction read write";

		const String setRoSql = "set transaction read only";

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public static ValueTask SetRwAsync
			(this ISqlTransaction transaction, CancellationToken cancellationToken = default) =>
			transaction.ExecuteAsync(setRwSql, cancellationToken);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public static ValueTask SetRoAsync
			(this ISqlTransaction transaction, CancellationToken cancellationToken = default) =>
			transaction.ExecuteAsync(setRoSql, cancellationToken);

		public static ValueTask SetAccessAsync
			(this ISqlTransaction transaction, SqlAccess access, CancellationToken cancellationToken = default) =>
			transaction.ExecuteAsync(
				access switch { SqlAccess.Rw => setRwSql, SqlAccess.Ro => setRoSql },
				cancellationToken);
	}
}
