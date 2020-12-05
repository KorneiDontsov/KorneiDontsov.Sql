namespace KorneiDontsov.Sql.Postgres.Example {
	using System;

	public class Post {
		public String? author { get; set; }

		public String content { get; set; } = null!;

		public DateTimeOffset timestamp { get; set; }
	}
}
