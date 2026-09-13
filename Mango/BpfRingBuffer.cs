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

        // Materialized once, on purpose: every method group conversion of
        // NativeCallback allocates a *separate* delegate object. Handing one
        // to ring_buffer__new and storing another in the field would leave
        // the marshaled function pointer rooted by nothing, and the first
        // record consumed would then abort the process with "A callback was
        // made on a garbage collected delegate".
        var nativeCallback = new RingBufferSampleFn(NativeCallback);

        var handle = NativeMethods.ring_buffer__new(map.Fd, nativeCallback, IntPtr.Zero, IntPtr.Zero);
        return handle.IsInvalid
            ? BpfResult<BpfRingBuffer>.Failure(BpfError.FromLastError())
            : BpfResult<BpfRingBuffer>.Success(new BpfRingBuffer(handle, nativeCallback));
    }

    /// <summary>
    /// Polls for new records, blocking up to <paramref name="timeoutMs"/>
    /// milliseconds. Returns the number of records consumed.
    /// </summary>
    public int Poll(int timeoutMs)
    {
        int consumed = NativeMethods.ring_buffer__poll(_handle, timeoutMs);

        // The JIT drops `this` as soon as _handle has been read, so without
        // this the delegate is collectable for the whole blocking call — the
        // exact window in which libbpf invokes it. The SafeHandle survives on
        // its own (the marshaler add-refs it), the delegate does not.
        GC.KeepAlive(_nativeCallback);

        return consumed;
    }

    public void Dispose() => _handle.Dispose();
}
