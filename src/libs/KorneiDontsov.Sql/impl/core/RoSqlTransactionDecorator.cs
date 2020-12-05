namespace KorneiDontsov.Sql {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Threading;
	using System.Threading.Tasks;

	sealed class RoSqlTransactionDecorator: IManagedRoSqlTransaction {
		readonly IManagedSqlTransaction transaction;

		public RoSqlTransactionDecorator (IManagedSqlTransaction transaction) =>
			this.transaction = transaction;

		/// <inheritdoc />
		public IsolationLevel initialIsolationLevel =>
			transaction.initialIsolationLevel;

		/// <inheritdoc />
		public Int32 defaultQueryTimeout =>
			transaction.defaultQueryTimeout;

		/// <inheritdoc />
		public ValueTask DisposeAsync () =>
			transaction.DisposeAsync();

		/// <inheritdoc />
		public void OnCommitted (Action action) => transaction.OnCommitted(action);

		/// <inheritdoc />
		public void OnCommitted (Func<ValueTask> action) => transaction.OnCommitted(action);

		/// <inheritdoc />
		public ValueTask CommitAsync (CancellationToken cancellationToken = default) =>
			transaction.CommitAsync(cancellationToken);

		/// <inheritdoc />
		public ValueTask RollbackAsync (CancellationToken cancellationToken = default) =>
			transaction.RollbackAsync(cancellationToken);

		/// <inheritdoc />
		public ValueTask ExecuteAsync
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null,
			 Affect affect = Affect.Any) =>
			transaction.ExecuteAsync(sql, cancellationToken, args, queryTimeout, affect);

		/// <inheritdoc />
		public ValueTask<Int32> QueryAffectedRowsCount
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryAffectedRowsCount(sql, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<IEnumerable<dynamic>> QueryRows
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryRows(sql, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<IEnumerable<T>> QueryRows<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryRows<T>(sql, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, TOutput>
			(String sql, String splitOn,
			 Func<T1, T2, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryRows(sql, splitOn, map, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, TOutput>
			(String sql, String splitOn,
			 Func<T1, T2, T3, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryRows(sql, splitOn, map, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, TOutput>
			(String sql, String splitOn,
			 Func<T1, T2, T3, T4, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryRows(sql, splitOn, map, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, TOutput>
			(String sql, String splitOn,
			 Func<T1, T2, T3, T4, T5, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryRows(sql, splitOn, map, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, T6, TOutput>
			(String sql, String splitOn,
			 Func<T1, T2, T3, T4, T5, T6, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryRows(sql, splitOn, map, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, T6, T7, TOutput>
			(String sql, String splitOn,
			 Func<T1, T2, T3, T4, T5, T6, T7, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryRows(sql, splitOn, map, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<dynamic> QueryFirstRow
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryFirstRow(sql, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<T> QueryFirstRow<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryFirstRow<T>(sql, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<dynamic> QueryFirstRowOrDefault
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryFirstRowOrDefault(sql, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<T> QueryFirstRowOrDefault<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryFirstRowOrDefault<T>(sql, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<dynamic> QuerySingleRow
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QuerySingleRow(sql, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<T> QuerySingleRow<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QuerySingleRow<T>(sql, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<dynamic> QuerySingleRowOrDefault
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QuerySingleRowOrDefault(sql, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<T> QuerySingleRowOrDefault<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QuerySingleRowOrDefault<T>(sql, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<dynamic> QueryScalar
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryScalar(sql, cancellationToken, args, queryTimeout);

		/// <inheritdoc />
		public ValueTask<T> QueryScalar<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) =>
			transaction.QueryScalar<T>(sql, cancellationToken, args, queryTimeout);
	}
}
