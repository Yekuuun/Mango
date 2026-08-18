using Mango.Handles;

namespace Mango;

/// <summary>
/// A live attachment between a <see cref="BpfProgram"/> and its hook,
/// created by <see cref="BpfProgram.Attach"/>. Disposing detaches it.
/// </summary>
public sealed class BpfLink : IDisposable
{
    private readonly BpfLinkHandle _handle;

    internal BpfLink(BpfLinkHandle handle) => _handle = handle;

    public void Dispose() => _handle.Dispose();
}
