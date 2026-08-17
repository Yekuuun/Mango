using System.Runtime.InteropServices;

namespace Mango.Interops;

/// <summary>
/// Mirrors libbpf's <c>libbpf_print_fn_t</c>. <paramref name="fmt"/> is a
/// native printf-style format string and <paramref name="args"/> is its
/// corresponding <c>va_list</c>. Neither is marshaled to a managed type
/// here — <c>va_list</c> has no portable managed representation, so callers
/// that want the rendered message must forward <paramref name="fmt"/> and
/// <paramref name="args"/> unmodified to a native <c>vsnprintf()</c> call.
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate int LibbpfPrintFn(LibbpfPrintLevel level, IntPtr fmt, IntPtr args);
