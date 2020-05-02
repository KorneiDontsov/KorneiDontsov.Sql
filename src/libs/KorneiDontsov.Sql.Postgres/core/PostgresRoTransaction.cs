namespace KorneiDontsov.Sql.Postgres {
	using Npgsql;
	using System;
	using System.Data;

	class PostgresRoTransaction: PostgresTransaction, IManagedRoSqlTransaction {
		public PostgresRoTransaction
			(NpgsqlConnection npgsqlConnection,
			 NpgsqlTransaction npgsqlTransaction,
			 IsolationLevel initialIsolationLevel,
			 Int32 defaultQueryTimeout):
			base(npgsqlConnection, npgsqlTransaction, SqlAccess.Ro, initialIsolationLevel, defaultQueryTimeout) { }
	}
}
