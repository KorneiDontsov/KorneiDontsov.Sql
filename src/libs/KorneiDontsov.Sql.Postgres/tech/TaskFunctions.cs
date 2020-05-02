using System.Threading;
using System.Threading.Tasks;

public static class TaskFunctions {
	public static Task WithCancellation (this Task task, CancellationToken cancellationToken = default) {
		if(task.IsCompleted || ! cancellationToken.CanBeCanceled)
			return task;
		else if(cancellationToken.IsCancellationRequested)
			return Task.FromCanceled(cancellationToken);
		else
			return Task.WhenAny(task, Task.Delay(Timeout.Infinite, cancellationToken)).Unwrap();
	}
}
