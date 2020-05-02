namespace KorneiDontsov.Sql {
	using Dapper;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Threading;
	using System.Threading.Tasks;

	public sealed class SqlQueryBuilder {
		readonly ISqlProvider sqlProvider;
		String? sql;
		DynamicParameters? args;
		CancellationToken cancellationToken;
		Int32? queryTimeout;

		public SqlQueryBuilder (ISqlProvider sqlProvider) =>
			this.sqlProvider = sqlProvider;

		public SqlQueryBuilder (ISqlProvider sqlProvider, Object args) {
			this.sqlProvider = sqlProvider;
			this.args = new DynamicParameters(args);
		}

		public SqlQueryBuilder (ISqlProvider sqlProvider, String sql) {
			this.sqlProvider = sqlProvider;
			this.sql = sql;
		}

		public SqlQueryBuilder Query (String sql) {
			this.sql = sql;
			return this;
		}

		public SqlQueryBuilder With (Object args) {
			if(this.args is {} argsCollection)
				argsCollection.AddDynamicParams(args);
			else
				this.args = new DynamicParameters(args);
			return this;
		}

		public SqlQueryBuilder With (String name, Object? value = null, DbType? dbType = null, Int32? size = null) {
			args ??= new DynamicParameters();
			args.Add(name, value, dbType, direction: null);
			return this;
		}

		// ReSharper disable once MethodOverloadWithOptionalParameter

		public SqlQueryBuilder With
			(String name,
			 Object? value = null,
			 DbType? dbType = null,
			 Int32? size = null,
			 Byte? precision = null,
			 Byte? scale = null) {
			args ??= new DynamicParameters();
			args.Add(name, value, dbType, direction: null, size: size, precision: precision, scale: scale);
			return this;
		}

		static Exception NoQueryError () =>
			new InvalidOperationException("No query sql.");

		public SqlQueryBuilder With (CancellationToken cancellationToken = default, Int32? queryTimeout = null) {
			this.cancellationToken = cancellationToken;
			this.queryTimeout = queryTimeout;
			return this;
		}

		public ValueTask Async () =>
			sqlProvider.ExecuteAsync(sql ?? throw NoQueryError(), cancellationToken, args, queryTimeout);

		// ReSharper disable once MethodOverloadWithOptionalParameter
		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask Async (CancellationToken? cancellationToken = null, Int32? queryTimeout = null) =>
			sqlProvider.ExecuteAsync(
				sql ?? throw NoQueryError(),
				cancellationToken ?? this.cancellationToken,
				args,
				queryTimeout ?? this.queryTimeout);

		public ValueTask QueryAsync (String sql) =>
			sqlProvider.ExecuteAsync(this.sql = sql, cancellationToken, args, queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<Int32> AffectedRowsCount () =>
			sqlProvider.QueryAffectedRowsCount(
				sql ?? throw NoQueryError(),
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<IEnumerable<dynamic>> Rows () =>
			sqlProvider.QueryRows(
				sql ?? throw NoQueryError(),
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<IEnumerable<T>> Rows<T> () =>
			sqlProvider.QueryRows<T>(
				sql ?? throw NoQueryError(),
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<IEnumerable<TOutput>> Rows<T1, T2, TOutput>
			(String splitOn, Func<T1, T2, TOutput> map) =>
			sqlProvider.QueryRows(
				sql ?? throw NoQueryError(),
				splitOn,
				map,
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<IEnumerable<TOutput>> Rows<T1, T2, T3, TOutput>
			(String splitOn, Func<T1, T2, T3, TOutput> map) =>
			sqlProvider.QueryRows(
				sql ?? throw NoQueryError(),
				splitOn,
				map,
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<IEnumerable<TOutput>> Rows<T1, T2, T3, T4, TOutput>
			(String splitOn, Func<T1, T2, T3, T4, TOutput> map) =>
			sqlProvider.QueryRows(
				sql ?? throw NoQueryError(),
				splitOn,
				map,
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<IEnumerable<TOutput>> Rows<T1, T2, T3, T4, T5, TOutput>
			(String splitOn, Func<T1, T2, T3, T4, T5, TOutput> map) =>
			sqlProvider.QueryRows(
				sql ?? throw NoQueryError(),
				splitOn,
				map,
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<IEnumerable<TOutput>> Rows<T1, T2, T3, T4, T5, T6, TOutput>
			(String splitOn, Func<T1, T2, T3, T4, T5, T6, TOutput> map) =>
			sqlProvider.QueryRows(
				sql ?? throw NoQueryError(),
				splitOn,
				map,
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<IEnumerable<TOutput>> Rows<T1, T2, T3, T4, T5, T6, T7, TOutput>
			(String splitOn, Func<T1, T2, T3, T4, T5, T6, T7, TOutput> map) =>
			sqlProvider.QueryRows(
				sql ?? throw NoQueryError(),
				splitOn,
				map,
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<dynamic> FirstRow () =>
			sqlProvider.QueryFirstRow(
				sql ?? throw NoQueryError(),
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<T> FirstRow<T> () =>
			sqlProvider.QueryFirstRow<T>(
				sql ?? throw NoQueryError(),
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<dynamic> FirstRowOrDefault () =>
			sqlProvider.QueryFirstRow(
				sql ?? throw NoQueryError(),
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<T> FirstRowOrDefault<T> () =>
			sqlProvider.QueryFirstRow<T>(
				sql ?? throw NoQueryError(),
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<dynamic> SingleRow () =>
			sqlProvider.QueryFirstRow(
				sql ?? throw NoQueryError(),
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<T> SingleRow<T> () =>
			sqlProvider.QueryFirstRow<T>(
				sql ?? throw NoQueryError(),
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<dynamic> SingleRowOrDefault () =>
			sqlProvider.QueryFirstRow(
				sql ?? throw NoQueryError(),
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<T> SingleRowOrDefault<T> () =>
			sqlProvider.QueryFirstRow<T>(
				sql ?? throw NoQueryError(),
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<dynamic> Scalar () =>
			sqlProvider.QueryScalar(
				sql ?? throw NoQueryError(),
				cancellationToken,
				args,
				queryTimeout);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		public ValueTask<T> Scalar<T> () =>
			sqlProvider.QueryScalar<T>(
				sql ?? throw NoQueryError(),
				cancellationToken,
				args,
				queryTimeout);
	}
}
