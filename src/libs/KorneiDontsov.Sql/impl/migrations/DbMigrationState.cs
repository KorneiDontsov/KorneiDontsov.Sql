namespace KorneiDontsov.Sql.Migrations {
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	class DbMigrationState: IDbMigrationState {
		public TaskCompletionSource<DbMigrationResult> whenCompleted { get; } =
			new TaskCompletionSource<DbMigrationResult>(TaskCreationOptions.RunContinuationsAsynchronously);

		/// <inheritdoc />
		public ValueTask<DbMigrationResult> WhenCompleted (CancellationToken cancellationToken = default) {
			static async ValueTask<T> WithCancellation<T> (Task<T> task, CancellationToken ct) {
				var awaitedTask = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, ct));
				if(awaitedTask == task)
					return await task;
				else {
					await awaitedTask;
					throw new OperationCanceledException(ct);
				}
			}

			if(whenCompleted.Task.IsCompleted || ! cancellationToken.CanBeCanceled)
				return new ValueTask<DbMigrationResult>(whenCompleted.Task);
			else if(cancellationToken.IsCancellationRequested)
				return new ValueTask<DbMigrationResult>(Task.FromCanceled<DbMigrationResult>(cancellationToken));
			else
				return WithCancellation(whenCompleted.Task, cancellationToken);
		}
	}
}
