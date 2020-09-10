namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginRepeatableReadAttribute: Attribute, IBeginSqlTransactionEndpointMetadata {
		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.RepeatableRead;
	}
}
