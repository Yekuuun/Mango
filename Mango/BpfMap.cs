using Mango.Handles;
using Mango.Interops;
using Mango.Models;

namespace Mango;

/// <summary>
/// A BPF map contained within a loaded <see cref="BpfObject"/>. Owned
/// by its parent object — never disposed independently.
/// </summary>
public sealed class BpfMap
{
    private readonly BpfMapHandle _handle;

    internal BpfMap(BpfMapHandle handle) => _handle = handle;

    public string Name => NativeMethods.bpf_map__name(_handle);

    public int Fd => NativeMethods.bpf_map__fd(_handle);

    public BpfMapType Type => NativeMethods.bpf_map__type(_handle);

    public uint KeySize => NativeMethods.bpf_map__key_size(_handle);

    public uint ValueSize => NativeMethods.bpf_map__value_size(_handle);

    public uint MaxEntries => NativeMethods.bpf_map__max_entries(_handle);

    /// <summary>
    /// Iterates every key currently in the map, oldest libbpf iteration
    /// order. Each key is a freshly allocated <c>KeySize</c>-byte buffer.
    /// </summary>
    public IEnumerable<byte[]> Keys
    {
        get
        {
            var keySize = (int)KeySize;
            byte[]? current = null;
            while (true)
            {
                var next = new byte[keySize];
                if (NativeMethods.bpf_map__get_next_key(_handle, current, next, (nuint)keySize) != 0)
                    yield break;

                yield return next;
                current = next;
            }
        }
    }

    public BpfResult<BpfMap> Pin(string? path = null) => Execute(NativeMethods.bpf_map__pin(_handle, path));

    public BpfResult<BpfMap> Unpin(string? path = null) => Execute(NativeMethods.bpf_map__unpin(_handle, path));

    public bool TryLookup(ReadOnlySpan<byte> key, Span<byte> value, ulong flags = 0) => NativeMethods.bpf_map__lookup_elem(_handle, key, (nuint)key.Length, value, (nuint)value.Length, flags) == 0;

    public bool TryUpdate(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value, ulong flags = 0) => NativeMethods.bpf_map__update_elem(_handle, key, (nuint)key.Length, value, (nuint)value.Length, flags) == 0;

    public bool TryDelete(ReadOnlySpan<byte> key, ulong flags = 0) => NativeMethods.bpf_map__delete_elem(_handle, key, (nuint)key.Length, flags) == 0;

    private BpfResult<BpfMap> Execute(int returnCode)
    {
        return returnCode == 0 ? BpfResult<BpfMap>.Success(this) : BpfResult<BpfMap>.Failure(BpfError.FromCode(returnCode));
    }
}
