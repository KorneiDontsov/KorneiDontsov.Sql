namespace KorneiDontsov.Sql {
	using System;
	using System.Data;
	using System.Threading;

	public static class SqlQueryBuilderFunctions {
		public static SqlQueryBuilder Exec (this ISqlProvider sqlProvider) =>
			new SqlQueryBuilder(sqlProvider);

		public static SqlQueryBuilder Query (this ISqlProvider sqlProvider, String sql) =>
			new SqlQueryBuilder(sqlProvider, sql);

		public static SqlQueryBuilder With (this ISqlProvider sqlProvider, CancellationToken cancellationToken) =>
			new SqlQueryBuilder(sqlProvider, cancellationToken);

		public static SqlQueryBuilder With (this ISqlProvider sqlProvider, Object args) =>
			new SqlQueryBuilder(sqlProvider, args);

		public static SqlQueryBuilder With
			(this ISqlProvider sqlProvider,
			 String name,
			 Object? value = null,
			 DbType? dbType = null,
			 Int32? size = null) =>
			sqlProvider.Exec().With(name, value, dbType, size);

		// ReSharper disable once MethodOverloadWithOptionalParameter
		public static SqlQueryBuilder With
			(this ISqlProvider sqlProvider,
			 String name,
			 Object? value = null,
			 DbType? dbType = null,
			 Int32? size = null,
			 Byte? precision = null,
			 Byte? scale = null) =>
			sqlProvider.Exec().With(name, value, dbType, size, precision, scale);
	}
}
