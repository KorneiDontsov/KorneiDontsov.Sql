namespace KorneiDontsov.Sql {
	using System;

	class SqlScopeState: ISqlRetryEvent {
		public IManagedSqlTransaction? transaction;

		event Action? EventRetry;

		/// <inheritdoc />
		public void AddHandler (Action handler) =>
			EventRetry += handler;

		/// <inheritdoc />
		public void RemoveHandler (Action handler) =>
			EventRetry -= handler;

		public void InvokeRetryEvent () =>
			EventRetry?.Invoke();
	}
}
