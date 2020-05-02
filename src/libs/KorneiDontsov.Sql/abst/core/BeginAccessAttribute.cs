namespace KorneiDontsov.Sql {
	using System;

	public sealed class BeginAccessAttribute: Attribute, IBeginAccessEndpointMetadata {
		/// <inheritdoc />
		public SqlAccess access { get; }

		public BeginAccessAttribute (SqlAccess access) =>
			this.access = access;
	}
}
