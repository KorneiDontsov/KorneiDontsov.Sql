namespace KorneiDontsov.Sql.Postgres {
	using Npgsql;

	public static class PostgresProviderFunctions {
		public static NpgsqlConnection GetNpgsqlConnection (this ISqlProvider sqlProvider) =>
			sqlProvider switch {
				PostgresConnection connection => connection.GetNpgsqlConnection(),
				_ => sqlProvider.GetConnection<NpgsqlConnection>()
			};

		public static NpgsqlConnection CreateNpgsqlConnection (this IDbProvider dbProvider) =>
			dbProvider switch {
				PostgresDbProvider provider => provider.CreateNpgsqlConnection(),
				_ => dbProvider.CreateConnection<NpgsqlConnection>()
			};
	}
}
