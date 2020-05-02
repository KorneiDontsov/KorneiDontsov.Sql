namespace KorneiDontsov.Sql {
	using System;

	public sealed class ConfigureDbMigrationAttribute: Attribute, IDbMigrationEndpointMetadata {
		/// <inheritdoc />
		public Boolean isRequired { get; }

		public ConfigureDbMigrationAttribute (Boolean isRequired) =>
			this.isRequired = isRequired;
	}
}
