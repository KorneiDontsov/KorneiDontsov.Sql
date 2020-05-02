namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginSerializableAttribute: Attribute, IBeginIsolationLevelEndpointMetadata {
		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.Serializable;
	}
}
