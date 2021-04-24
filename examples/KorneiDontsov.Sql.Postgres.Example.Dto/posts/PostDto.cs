namespace KorneiDontsov.Sql.Postgres.Example {
	using System;

	public record PostDto {
		public String? author { get; set; }

		public String content { get; set; } = null!;
	}
}
