namespace KorneiDontsov.Sql {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Threading;
	using System.Threading.Tasks;

	public interface ISqlProvider {
		SqlAccess initialAccess { get; }

		IsolationLevel initialIsolationLevel { get; }

		Int32 defaultQueryTimeout { get; }

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask ExecuteAsync
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<Int32> QueryAffectedRowsCount
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<IEnumerable<dynamic>> QueryRows
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<IEnumerable<T>> QueryRows<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, TOutput>
			(String sql,
			 String splitOn,
			 Func<T1, T2, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, TOutput>
			(String sql,
			 String splitOn,
			 Func<T1, T2, T3, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, TOutput>
			(String sql,
			 String splitOn,
			 Func<T1, T2, T3, T4, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, TOutput>
			(String sql,
			 String splitOn,
			 Func<T1, T2, T3, T4, T5, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, T6, TOutput>
			(String sql,
			 String splitOn,
			 Func<T1, T2, T3, T4, T5, T6, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, T6, T7, TOutput>
			(String sql,
			 String splitOn,
			 Func<T1, T2, T3, T4, T5, T6, T7, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<dynamic> QueryFirstRow
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<T> QueryFirstRow<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<dynamic> QueryFirstRowOrDefault
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<T> QueryFirstRowOrDefault<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<dynamic> QuerySingleRow
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<T> QuerySingleRow<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<dynamic> QuerySingleRowOrDefault
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<T> QuerySingleRowOrDefault<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<dynamic> QueryScalar
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<T> QueryScalar<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null);
	}
}
