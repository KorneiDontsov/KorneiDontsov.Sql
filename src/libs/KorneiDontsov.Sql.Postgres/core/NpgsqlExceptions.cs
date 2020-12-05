namespace KorneiDontsov.Sql.Postgres {
	using Npgsql;
	using System;

	static class NpgsqlExceptions {
		public static void Handle (Exception ex) {
			switch(ex) {
				case PostgresException pgEx:
					throw pgEx.SqlState switch {
						PostgresErrorCodes.SerializationFailure =>
							new SqlException.ConflictFailure(SqlConflict.SerializationFailure, pgEx),
						PostgresErrorCodes.UniqueViolation =>
							new SqlException.ConflictFailure(SqlConflict.UniqueViolation, pgEx),
						_ => new SqlException(innerException: pgEx)
					};
				case NpgsqlException _:
					throw new SqlException(innerException: ex);
			}
		}
	}
}
