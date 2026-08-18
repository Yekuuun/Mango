using Mango.Handles;
using Mango.Interops;
using Mango.Models;

namespace Mango;

/// <summary>
/// Polls a <c>BPF_MAP_TYPE_RINGBUF</c> map, invoking a managed callback
/// with each record's raw bytes.
/// </summary>
public sealed class BpfRingBuffer : IDisposable
{
    private readonly BpfRingBufferHandle _handle;

    // Rooted for the handle's lifetime: libbpf holds the marshaled function
    // pointer for this delegate natively, so it must not be collected while
    // the ring buffer manager can still invoke it.
    private readonly RingBufferSampleFn _nativeCallback;

    private BpfRingBuffer(BpfRingBufferHandle handle, RingBufferSampleFn nativeCallback)
    {
        _handle = handle;
        _nativeCallback = nativeCallback;
    }

    /// <summary>
    /// Creates a ring buffer manager over <paramref name="map"/>, which
    /// must be a <c>BPF_MAP_TYPE_RINGBUF</c> map. <paramref name="onEvent"/>
    /// is invoked with each record's bytes during <see cref="Poll"/>; the
    /// span is only valid for the duration of that call.
    /// </summary>
    public static BpfResult<BpfRingBuffer> Create(BpfMap map, Action<ReadOnlySpan<byte>> onEvent)
    {
        int NativeCallback(IntPtr ctx, IntPtr data, nuint size)
        {
            try
            {
                unsafe
                {
                    onEvent(new ReadOnlySpan<byte>((void*)data, (int)size));
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ring buffer callback threw: {ex}");
            }

            return 0;
        }

        var handle = NativeMethods.ring_buffer__new(map.Fd, NativeCallback, IntPtr.Zero, IntPtr.Zero);
        return handle.IsInvalid
            ? BpfResult<BpfRingBuffer>.Failure(BpfError.FromLastError())
            : BpfResult<BpfRingBuffer>.Success(new BpfRingBuffer(handle, NativeCallback));
    }

    /// <summary>
    /// Polls for new records, blocking up to <paramref name="timeoutMs"/>
    /// milliseconds. Returns the number of records consumed.
    /// </summary>
    public int Poll(int timeoutMs) => NativeMethods.ring_buffer__poll(_handle, timeoutMs);

    public void Dispose() => _handle.Dispose();
}
