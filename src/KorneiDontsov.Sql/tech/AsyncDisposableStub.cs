using System;
using System.Threading.Tasks;

sealed class AsyncDisposableStub: IAsyncDisposable {
	public static AsyncDisposableStub shared { get; } = new AsyncDisposableStub();

	/// <inheritdoc />
	public ValueTask DisposeAsync () => default;
}
