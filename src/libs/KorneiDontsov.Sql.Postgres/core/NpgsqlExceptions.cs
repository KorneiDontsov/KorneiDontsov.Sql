namespace KorneiDontsov.Sql.Postgres {
	using Npgsql;
	using System;

	static class NpgsqlExceptions {
		public static SqlException? Match (Exception ex) =>
			ex switch {
				PostgresException pgEx =>
					pgEx.SqlState switch {
						PostgresErrorCodes.SerializationFailure =>
							new SqlException.ConflictFailure(SqlConflict.SerializationFailure, pgEx),
						PostgresErrorCodes.UniqueViolation =>
							new SqlException.ConflictFailure(SqlConflict.UniqueViolation, pgEx),
						_ => new SqlException(innerException: pgEx)
					},
				NpgsqlException _ => new SqlException(innerException: ex),
				_ => null
			};
	}
}
