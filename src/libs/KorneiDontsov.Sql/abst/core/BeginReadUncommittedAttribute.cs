namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginReadUncommittedAttribute: Attribute, IBeginIsolationLevelEndpointMetadata {
		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.ReadUncommitted;
	}
}
