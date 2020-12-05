namespace KorneiDontsov.Sql {
	using System;

	public class SqlException: Exception {
		public SqlException (String? message = null, Exception? innerException = null):
			base(message, innerException) { }

		public sealed class AssertionFailure: SqlException {
			public AssertionFailure (String? message = null, Exception? innerException = null):
				base(message, innerException) { }
		}

		public sealed class MigrationFailure: SqlException {
			public MigrationFailure (String? message = null, Exception? innerException = null):
				base(message, innerException) { }
		}

		public sealed class ConflictFailure: SqlException {
			public SqlConflict conflict { get; }

			public ConflictFailure (SqlConflict conflict, Exception? innerException = null):
				base(null, innerException) =>
				this.conflict = conflict;

			public override String Message => $"Sql conflict '{conflict}' occurred.";
		}
	}
}
