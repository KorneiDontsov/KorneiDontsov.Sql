namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginRwReadUncommittedAttribute: Attribute, IBeginSqlTransactionEndpointMetadata {
		/// <inheritdoc />
		public SqlAccess? access => SqlAccess.Rw;

		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.ReadUncommitted;
	}
}
