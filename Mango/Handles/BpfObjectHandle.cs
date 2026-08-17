using System.Runtime.InteropServices;
using Mango.Interops;

namespace Mango.Handles;

internal sealed class BpfObjectHandle : SafeHandle
{
    public BpfObjectHandle() : base(IntPtr.Zero, ownsHandle:true) {}

    public override bool IsInvalid => handle == IntPtr.Zero;

    /// <summary>
    /// On handle release. => call bpf_program__close()
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    protected override bool ReleaseHandle()
    {
        NativeMethods.bpf_object__close(handle);
        return true;
    }
}