namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginAttribute: Attribute, IBeginSqlTransactionEndpointMetadata {
		/// <inheritdoc />
		public SqlAccess? access { get; }

		/// <inheritdoc />
		public IsolationLevel isolationLevel { get; }

		public BeginAttribute (IsolationLevel isolationLevel, SqlAccess? access = null) {
			this.isolationLevel = isolationLevel;
			this.access = access;
		}
	}
}
