using System;
using System.Threading.Tasks;

class AsyncDisposableStub: IAsyncDisposable {
	public static AsyncDisposableStub shared { get; } = new AsyncDisposableStub();

	/// <inheritdoc />
	public ValueTask DisposeAsync () => default;
}
