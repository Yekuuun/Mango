using System.Runtime.InteropServices;
using Mango.Interops;

namespace Mango.Handles;

internal sealed class BpfRingBufferHandle : SafeHandle
{
    public BpfRingBufferHandle() : base(IntPtr.Zero, ownsHandle: true) {}

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeMethods.ring_buffer__free(handle);
        return true;
    }
}
