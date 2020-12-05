namespace KorneiDontsov.Sql.Postgres {
	using Npgsql;
	using System;
	using System.Data;

	sealed class PostgresRwTransaction: PostgresTransaction, IManagedRwSqlTransaction {
		public PostgresRwTransaction
			(NpgsqlConnection npgsqlConnection,
			 NpgsqlTransaction npgsqlTransaction,
			 IsolationLevel initialIsolationLevel,
			 Int32 defaultQueryTimeout):
			base(npgsqlConnection, npgsqlTransaction, SqlAccess.Rw, initialIsolationLevel, defaultQueryTimeout) { }
	}
}
