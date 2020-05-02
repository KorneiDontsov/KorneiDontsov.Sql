namespace KorneiDontsov.Sql {
	using System;

	public sealed class BeginRoAttribute: Attribute, IBeginAccessEndpointMetadata {
		/// <inheritdoc />
		public SqlAccess access => SqlAccess.Ro;
	}
}
