namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginRwReadCommittedAttribute: Attribute, IBeginAccessEndpointMetadata, IBeginIsolationLevelEndpointMetadata {
		/// <inheritdoc />
		public SqlAccess access => SqlAccess.Rw;

		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.ReadCommitted;
	}
}
