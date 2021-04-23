namespace KorneiDontsov.Sql.Postgres {
	using Npgsql;
	using System;

	static class NpgsqlExceptions {
		public static SqlException? MatchToSqlException (Exception ex) =>
			ex switch {
				PostgresException { SqlState: PostgresErrorCodes.SerializationFailure } =>
					new SqlException.ConflictFailure(SqlConflict.SerializationFailure, ex),
				PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } =>
					new SqlException.ConflictFailure(SqlConflict.UniqueViolation, ex),
				NpgsqlException { InnerException: TimeoutException } => new SqlException.Timeout(innerException: ex),
				PostgresException or NpgsqlException => new SqlException(innerException: ex),
				_ => null
			};
	}
}
