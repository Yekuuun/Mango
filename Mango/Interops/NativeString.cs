using System.Runtime.InteropServices;

namespace Mango.Interops;

/// <summary>
/// Conversion helpers for NUL-terminated UTF-8 strings owned by libbpf.
/// </summary>
internal static class NativeString
{
    /// <summary>
    /// Copies a NUL-terminated UTF-8 string out of a buffer owned by the native
    /// library. The source buffer is borrowed — it is never freed here, since it
    /// points into libbpf's own internal structures.
    /// </summary>
    internal static string FromBorrowedPtr(IntPtr ptr)
        => ptr == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
}
