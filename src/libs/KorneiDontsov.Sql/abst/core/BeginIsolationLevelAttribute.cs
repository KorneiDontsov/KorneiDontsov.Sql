namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginIsolationLevelAttribute: Attribute, IBeginIsolationLevelEndpointMetadata {
		/// <inheritdoc />
		public IsolationLevel isolationLevel { get; }

		public BeginIsolationLevelAttribute (IsolationLevel isolationLevel) =>
			this.isolationLevel = isolationLevel;
	}
}
