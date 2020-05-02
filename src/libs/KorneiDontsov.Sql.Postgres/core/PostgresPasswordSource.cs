namespace KorneiDontsov.Sql.Postgres {
	using System;

	public abstract class PostgresPasswordSource {
		PostgresPasswordSource () { }

		public sealed class Text: PostgresPasswordSource {
			public String value { get; }

			public Text (String value) =>
				this.value = value;
		}

		public sealed class PgPassFile: PostgresPasswordSource {
			public String path { get; }

			public PgPassFile (String path) =>
				this.path = path;
		}
	}
}
