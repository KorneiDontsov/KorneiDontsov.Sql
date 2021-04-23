namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginReadCommittedAttribute: Attribute, IBeginSqlTransactionEndpointMetadata {
		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.ReadCommitted;
	}
}
