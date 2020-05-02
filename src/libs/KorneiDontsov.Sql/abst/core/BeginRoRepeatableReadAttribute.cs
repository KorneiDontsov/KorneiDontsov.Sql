namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginRoRepeatableReadAttribute: Attribute, IBeginAccessEndpointMetadata, IBeginIsolationLevelEndpointMetadata {
		/// <inheritdoc />
		public SqlAccess access => SqlAccess.Ro;

		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.RepeatableRead;
	}
}
