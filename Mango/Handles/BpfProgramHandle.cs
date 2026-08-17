using System.Runtime.InteropServices;

namespace Mango.Handles;

internal sealed class BpfProgramHandle : SafeHandle
{
    public BpfProgramHandle() : base(IntPtr.Zero, ownsHandle:false) {}

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle() => true;
}