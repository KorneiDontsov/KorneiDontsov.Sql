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

		public sealed class SerializationFailure: SqlException {
			public SerializationFailure (String? message = null, Exception? innerException = null):
				base(message, innerException) { }
		}

		public sealed class UniqueViolation: SqlException {
			public UniqueViolation (String? message = null, Exception? innerException = null):
				base(message, innerException) { }
		}
	}
}
