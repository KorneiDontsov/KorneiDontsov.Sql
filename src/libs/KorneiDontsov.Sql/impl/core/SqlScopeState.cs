namespace KorneiDontsov.Sql {
	using System;
	using System.Threading.Tasks;

	class SqlScopeState: IAsyncDisposable {
		public IManagedSqlTransaction? transaction;

		/// <inheritdoc />
		public ValueTask DisposeAsync () =>
			transaction?.DisposeAsync() ?? default;
	}
}
