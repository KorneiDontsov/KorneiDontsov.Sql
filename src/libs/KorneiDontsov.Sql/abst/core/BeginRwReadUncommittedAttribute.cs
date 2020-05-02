namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginRwReadUncommittedAttribute: Attribute, IBeginAccessEndpointMetadata, IBeginIsolationLevelEndpointMetadata {
		/// <inheritdoc />
		public SqlAccess access => SqlAccess.Rw;

		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.ReadUncommitted;
	}
}
