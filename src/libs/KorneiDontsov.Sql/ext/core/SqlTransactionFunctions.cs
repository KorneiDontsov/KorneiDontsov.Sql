namespace KorneiDontsov.Sql {
	using System;
	using System.Data;
	using System.Threading;
	using System.Threading.Tasks;

	public static class SqlTransactionFunctions {
		const String setRwSql = "set transaction read write";

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public static ValueTask SetRwAsync (this ISqlTransaction transaction, CancellationToken ct = default) =>
			transaction.ExecuteAsync(setRwSql, ct);

		const String setRoSql = "set transaction read only";

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public static ValueTask SetRoAsync (this ISqlTransaction transaction, CancellationToken ct = default) =>
			transaction.ExecuteAsync(setRoSql, ct);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public static ValueTask SetAccessAsync
			(this ISqlTransaction transaction, SqlAccess access, CancellationToken ct = default) {
			var sql = access switch { SqlAccess.Rw => setRwSql, SqlAccess.Ro => setRoSql };
			return transaction.ExecuteAsync(sql, ct);
		}

		const String setReadUncommittedSql = "set transaction isolation level read uncommitted";

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public static ValueTask SetReadUncommitted (this ISqlTransaction transaction, CancellationToken ct = default) =>
			transaction.ExecuteAsync(setReadUncommittedSql, ct);

		const String setReadCommittedSql = "set transaction isolation level read committed";

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public static ValueTask SetReadCommitted (this ISqlTransaction transaction, CancellationToken ct = default) =>
			transaction.ExecuteAsync(setReadCommittedSql, ct);

		const String setRepeatableReadSql = "set transaction isolation level repeatable read";

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public static ValueTask SetRepeatableRead (this ISqlTransaction transaction, CancellationToken ct = default) =>
			transaction.ExecuteAsync(setRepeatableReadSql, ct);

		const String setSerializableSql = "set transaction isolation level serializable";

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public static ValueTask SetSerializable (this ISqlTransaction transaction, CancellationToken ct = default) =>
			transaction.ExecuteAsync(setSerializableSql, ct);

		/// <param name = "isolationLevel">
		///     Only SQL standard isolation levels are supported, except
		///     <see cref = "IsolationLevel.Snapshot" />.
		/// </param>
		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public static ValueTask SetIsolationLevel
			(this ISqlTransaction transaction, IsolationLevel isolationLevel, CancellationToken ct = default) {
			var sql =
				isolationLevel switch {
					IsolationLevel.ReadUncommitted => setReadUncommittedSql,
					IsolationLevel.ReadCommitted => setReadCommittedSql,
					IsolationLevel.RepeatableRead => setRepeatableReadSql,
					IsolationLevel.Serializable => setSerializableSql,
					IsolationLevel.Unspecified =>
						throw new ArgumentException(
							"Transaction has no specified isolation level.",
							nameof(transaction)),
					IsolationLevel.Snapshot =>
						throw new ArgumentException(
							"Snapshot isolation level cannot be set by this method.",
							nameof(transaction)),
					_ =>
						throw new ArgumentException(
							$"Isolation level {isolationLevel} is not part of SQL standard.",
							nameof(transaction))
				};
			return transaction.ExecuteAsync(sql, ct);
		}
	}
}
