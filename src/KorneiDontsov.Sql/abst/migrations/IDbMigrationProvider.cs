﻿namespace KorneiDontsov.Sql.Migrations {
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	public interface IDbMigrationProvider {
		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<IAsyncDisposable> Lock (String migrationSchema, CancellationToken cancellationToken = default) =>
			new(AsyncDisposableStub.shared);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<(Int32 index, String id)?> MaybeLastMigrationInfo
			(IRwSqlTransaction transaction, String migrationSchema, CancellationToken cancellationToken = default);

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask SetLastMigrationInfo
			(IRwSqlTransaction transaction,
			 String migrationSchema,
			 Int32 migrationIndex,
			 String migrationId,
			 CancellationToken cancellationToken = default);
	}
}
