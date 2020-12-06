namespace KorneiDontsov.Sql.Postgres {
	using Npgsql;
	using System;

	static class NpgsqlExceptions {
		public static SqlException? MatchToSqlException (Exception ex) =>
			ex switch {
				PostgresException pgEx =>
					pgEx.SqlState switch {
						PostgresErrorCodes.SerializationFailure =>
							new SqlException.ConflictFailure(SqlConflict.SerializationFailure, ex),
						PostgresErrorCodes.UniqueViolation =>
							new SqlException.ConflictFailure(SqlConflict.UniqueViolation, ex),
						_ => new SqlException(innerException: ex)
					},
				NpgsqlException _ => new SqlException(innerException: ex),
				_ => null
			};
	}
}
