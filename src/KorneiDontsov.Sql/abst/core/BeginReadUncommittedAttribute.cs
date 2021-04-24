namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginReadUncommittedAttribute: Attribute, IBeginSqlTransactionEndpointMetadata {
		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.ReadUncommitted;
	}
}
