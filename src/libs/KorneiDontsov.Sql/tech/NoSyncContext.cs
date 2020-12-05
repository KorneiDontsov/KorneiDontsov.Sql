using System;
using System.Threading;

readonly struct NoSyncContext: IDisposable {
	readonly SynchronizationContext? overridenSyncContext;

	public NoSyncContext (SynchronizationContext? overridenSyncContext) =>
		this.overridenSyncContext = overridenSyncContext;

	public static NoSyncContext On () {
		var result = new NoSyncContext(SynchronizationContext.Current);
		SynchronizationContext.SetSynchronizationContext(null);
		return result;
	}

	/// <inheritdoc />
	public void Dispose () =>
		SynchronizationContext.SetSynchronizationContext(overridenSyncContext);
}
