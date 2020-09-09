namespace KorneiDontsov.Sql {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Threading;
	using System.Threading.Tasks;

	public readonly struct SqlProvider: ISqlProvider {
		IDbProvider provider { get; }

		/// <inheritdoc />
		public SqlAccess initialAccess { get; }

		/// <inheritdoc />
		public IsolationLevel initialIsolationLevel { get; }

		public SqlProvider
			(IDbProvider provider, SqlAccess initialAccess, IsolationLevel initialIsolationLevel) {
			this.provider = provider;
			this.initialAccess = initialAccess;
			this.initialIsolationLevel = initialIsolationLevel;
		}

		/// <inheritdoc />
		public Int32 defaultQueryTimeout => provider.defaultQueryTimeout;

		ValueTask<IManagedSqlTransaction> BeginTransaction (CancellationToken cancellationToken) =>
			provider.Begin(initialAccess, initialIsolationLevel, cancellationToken);

		/// <inheritdoc />
		public async ValueTask ExecuteAsync
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null,
			 Affect affect = Affect.Any) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				await transaction.ExecuteAsync(sql, cancellationToken, args, queryTimeout, affect)
					.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<Int32> QueryAffectedRowsCount
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryAffectedRowsCount(sql, cancellationToken, args, queryTimeout)
						.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<dynamic>> QueryRows
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryRows(sql, cancellationToken, args, queryTimeout).ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<T>> QueryRows<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryRows<T>(sql, cancellationToken, args, queryTimeout)
						.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, TOutput>
			(String sql,
			 String splitOn,
			 Func<T1, T2, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryRows(sql, splitOn, map, cancellationToken, args, queryTimeout)
						.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, TOutput>
			(String sql,
			 String splitOn,
			 Func<T1, T2, T3, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryRows(sql, splitOn, map, cancellationToken, args, queryTimeout)
						.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, TOutput>
			(String sql,
			 String splitOn,
			 Func<T1, T2, T3, T4, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryRows(sql, splitOn, map, cancellationToken, args, queryTimeout)
						.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, TOutput>
			(String sql,
			 String splitOn,
			 Func<T1, T2, T3, T4, T5, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryRows(sql, splitOn, map, cancellationToken, args, queryTimeout)
						.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, T6, TOutput>
			(String sql,
			 String splitOn,
			 Func<T1, T2, T3, T4, T5, T6, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryRows(sql, splitOn, map, cancellationToken, args, queryTimeout)
						.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<IEnumerable<TOutput>> QueryRows<T1, T2, T3, T4, T5, T6, T7, TOutput>
			(String sql,
			 String splitOn,
			 Func<T1, T2, T3, T4, T5, T6, T7, TOutput> map,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryRows(sql, splitOn, map, cancellationToken, args, queryTimeout)
						.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<dynamic> QueryFirstRow
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryFirstRow(sql, cancellationToken, args, queryTimeout)
						.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<T> QueryFirstRow<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryFirstRow<T>(sql, cancellationToken, args, queryTimeout)
						.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<dynamic> QueryFirstRowOrDefault
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryFirstRowOrDefault(sql, cancellationToken, args, queryTimeout)
						.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<T> QueryFirstRowOrDefault<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryFirstRowOrDefault<T>(sql, cancellationToken, args, queryTimeout)
						.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<dynamic> QuerySingleRow
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QuerySingleRow(sql, cancellationToken, args, queryTimeout).ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<T> QuerySingleRow<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QuerySingleRow<T>(sql, cancellationToken, args, queryTimeout).ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<dynamic> QuerySingleRowOrDefault
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QuerySingleRowOrDefault(sql, cancellationToken, args, queryTimeout)
						.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<T> QuerySingleRowOrDefault<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QuerySingleRowOrDefault<T>(sql, cancellationToken, args, queryTimeout)
						.ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<dynamic> QueryScalar
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryScalar(sql, cancellationToken, args, queryTimeout).ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async ValueTask<T> QueryScalar<T>
			(String sql,
			 CancellationToken cancellationToken = default,
			 Object? args = null,
			 Int32? queryTimeout = null) {
			var transaction = await BeginTransaction(cancellationToken).ConfigureAwait(false);
			try {
				var result = await
					transaction.QueryScalar<T>(sql, cancellationToken, args, queryTimeout).ConfigureAwait(false);
				await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
				return result;
			}
			finally {
				await transaction.DisposeAsync().ConfigureAwait(false);
			}
		}
	}
}
