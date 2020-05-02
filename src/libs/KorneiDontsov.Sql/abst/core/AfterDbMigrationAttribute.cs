namespace KorneiDontsov.Sql {
	using System;

	public sealed class AfterDbMigrationAttribute: Attribute, IDbMigrationEndpointMetadata {
		/// <inheritdoc />
		public Boolean isRequired => true;
	}
}
