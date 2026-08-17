using System.Runtime.InteropServices;

namespace Mango.Handles;

internal sealed class BpfMapHandle : SafeHandle
{
    public BpfMapHandle() : base(IntPtr.Zero, ownsHandle: false) {}

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle() => true;
}
