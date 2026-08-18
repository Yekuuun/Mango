using Mango.Handles;
using Mango.Interops;
using Mango.Models;

namespace Mango;

/// <summary>
/// A BPF program contained within a loaded <see cref="BpfObject"/>. Owned
/// by its parent object — never disposed independently.
/// </summary>
public sealed class BpfProgram
{
    private readonly BpfProgramHandle _handle;

    internal BpfProgram(BpfProgramHandle handle) => _handle = handle;

    public string Name => NativeMethods.bpf_program__name(_handle);

    public int Fd => NativeMethods.bpf_program__fd(_handle);

    public BpfProgramType Type => NativeMethods.bpf_program__type(_handle);

    /// <summary>
    /// Whether this program is loaded by default during
    /// <see cref="BpfObject.Load"/>. Must be set before the object is loaded.
    /// </summary>
    public bool Autoload
    {
        get => NativeMethods.bpf_program__autoload(_handle);
        set
        {
            var returnCode = NativeMethods.bpf_program__set_autoload(_handle, value);
            
            if (returnCode != 0)
                throw new InvalidOperationException(BpfError.FromCode(returnCode).ToString());
        }
    }

    /// <summary>
    /// Attaches this program via libbpf's generic auto-detection (kprobe,
    /// uprobe, tracepoint, raw tracepoint, and typed tracing programs).
    /// </summary>
    public BpfResult<BpfLink> Attach()
    {
        var handle = NativeMethods.bpf_program__attach(_handle);
        return handle.IsInvalid ? BpfResult<BpfLink>.Failure(BpfError.FromLastError()) : BpfResult<BpfLink>.Success(new BpfLink(handle));
    }
}
