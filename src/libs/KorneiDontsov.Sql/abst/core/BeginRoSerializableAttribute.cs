namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginRoSerializableAttribute: Attribute, IBeginSqlTransactionEndpointMetadata {
		/// <inheritdoc />
		public SqlAccess? access => SqlAccess.Ro;

		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.Serializable;
	}
}
