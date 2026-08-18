using System.Runtime.InteropServices;
using System.Text;
using Mango.Interops;

namespace Mango.Models;

/// <summary>
/// A libbpf/kernel error, carrying the raw error code and the message
/// rendered by libbpf's own <c>libbpf_strerror()</c>.
/// </summary>
public readonly record struct BpfError(int Code, string Message)
{
    /// <summary>
    /// Builds a <see cref="BpfError"/> from a negative error code returned
    /// directly by an int-returning libbpf call.
    /// </summary>
    internal static BpfError FromCode(int code) => new(code, Describe(code));

    /// <summary>
    /// Builds a <see cref="BpfError"/> from <c>errno</c>, for libbpf calls
    /// whose only failure signal is a NULL/invalid pointer return.
    /// </summary>
    internal static BpfError FromLastError() => FromCode(Marshal.GetLastPInvokeError());

    private static string Describe(int code)
    {
        var buf = new byte[256];
        if (NativeMethods.libbpf_strerror(code, buf, (nuint)buf.Length) != 0)
            return $"errno {code}";

        var length = Array.IndexOf(buf, (byte)0);
        return Encoding.UTF8.GetString(buf, 0, length < 0 ? buf.Length : length);
    }

    public override string ToString() => $"({Code}) {Message}";
}
