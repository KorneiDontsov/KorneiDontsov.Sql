namespace KorneiDontsov.Sql {
	using System;

	public sealed class WithoutDbMigrationAttribute: Attribute, IDbMigrationEndpointMetadata {
		public static WithoutDbMigrationAttribute shared { get; } =
			new WithoutDbMigrationAttribute();

		/// <inheritdoc />
		public Boolean isRequired => false;
	}
}
