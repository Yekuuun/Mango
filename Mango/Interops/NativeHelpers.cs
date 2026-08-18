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

/// <summary>
/// Mirrors libbpf's <c>ring_buffer_sample_fn</c>. Invoked once per record
/// consumed from a ring buffer; <paramref name="data"/>/<paramref name="size"/>
/// point at kernel-owned memory that is only valid for the duration of the
/// call. Return 0 to keep polling, or a negative value to stop
/// <c>ring_buffer__poll()</c>/<c>ring_buffer__consume()</c> early.
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate int RingBufferSampleFn(IntPtr ctx, IntPtr data, nuint size);
