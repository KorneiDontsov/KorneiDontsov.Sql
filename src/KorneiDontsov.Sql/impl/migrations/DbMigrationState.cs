namespace KorneiDontsov.Sql.Migrations {
	using System;
	using System.Threading;

	sealed class DbMigrationState: IDbMigrationState, IDisposable {
		readonly CancellationTokenSource onCompletedSource = new();

		DbMigrationResult? resultValue;

		/// <inheritdoc />
		public DbMigrationResult? result => resultValue;

		/// <inheritdoc />
		public CancellationToken onCompleted => onCompletedSource.Token;

		public void Complete (DbMigrationResult result) {
			resultValue = result;
			onCompletedSource.Cancel();
		}

		/// <inheritdoc />
		public void Dispose () => onCompletedSource.Dispose();
	}
}
