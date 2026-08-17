using System.Runtime.InteropServices;
using Mango.Interops;

namespace Mango.Handles;

internal sealed class BpfLinkHandle : SafeHandle
{
    public BpfLinkHandle() : base(IntPtr.Zero, ownsHandle: true) {}

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        return NativeMethods.bpf_link__destroy(handle) == 0;
    }
}
