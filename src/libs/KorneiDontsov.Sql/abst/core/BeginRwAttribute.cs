namespace KorneiDontsov.Sql {
	using System;

	public sealed class BeginRwAttribute: Attribute, IBeginAccessEndpointMetadata {
		/// <inheritdoc />
		public SqlAccess access => SqlAccess.Rw;
	}
}
