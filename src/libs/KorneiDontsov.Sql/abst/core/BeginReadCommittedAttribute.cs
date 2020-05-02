namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginReadCommittedAttribute: Attribute, IBeginIsolationLevelEndpointMetadata {
		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.ReadCommitted;
	}
}
