namespace KorneiDontsov.Sql.Postgres {
	using Npgsql;
	using System;

	static class NpgsqlExceptions {
		public static void Handle (Exception ex) {
			switch(ex) {
				case PostgresException pgEx:
					switch(pgEx.SqlState) {
						case PostgresErrorCodes.SerializationFailure:
							throw new SqlException.SerializationFailure(ex.Message, pgEx);
						case PostgresErrorCodes.UniqueViolation:
							throw new SqlException.UniqueViolation(ex.Message, pgEx);
					}
					break;
				case NpgsqlException _:
					throw new SqlException(innerException: ex);
			}
		}
	}
}
