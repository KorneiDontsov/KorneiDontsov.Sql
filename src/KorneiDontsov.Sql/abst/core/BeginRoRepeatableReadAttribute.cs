namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginRoRepeatableReadAttribute: Attribute, IBeginSqlTransactionEndpointMetadata {
		/// <inheritdoc />
		public SqlAccess? access => SqlAccess.Ro;

		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.RepeatableRead;
	}
}
