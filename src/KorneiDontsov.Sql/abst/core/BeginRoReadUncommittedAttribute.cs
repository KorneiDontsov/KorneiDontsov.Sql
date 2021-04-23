namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginRoReadUncommittedAttribute: Attribute, IBeginSqlTransactionEndpointMetadata {
		/// <inheritdoc />
		public SqlAccess? access => SqlAccess.Ro;

		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.ReadUncommitted;
	}
}
