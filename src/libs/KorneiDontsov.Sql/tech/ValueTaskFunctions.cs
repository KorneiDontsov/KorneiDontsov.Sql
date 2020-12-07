using System.Threading.Tasks;

static class ValueTaskFunctions {
	public static void Wait (this ValueTask task) {
		if(task.IsCompleted)
			task.GetAwaiter().GetResult();
		else
			task.AsTask().GetAwaiter().GetResult();
	}

	public static T Wait<T> (this ValueTask<T> task) =>
		task.IsCompleted
			? task.Result
			: task.AsTask().GetAwaiter().GetResult();
}
