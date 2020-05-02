namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginRepeatableReadAttribute: Attribute, IBeginIsolationLevelEndpointMetadata {
		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.RepeatableRead;
	}
}
