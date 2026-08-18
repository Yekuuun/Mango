using Mango.Handles;
using Mango.Interops;
using Mango.Models;

namespace Mango;

/// <summary>
/// A loaded BPF ELF object — the public entry point for opening, loading,
/// pinning, and enumerating the programs and maps of a <c>.o</c> file.
/// </summary>
public sealed class BpfObject : IDisposable
{
    private readonly BpfObjectHandle _handle;

    private BpfObject(BpfObjectHandle handle) => _handle = handle;

    public string Name => NativeMethods.bpf_object__name(_handle);

    /// <summary>
    /// Iterates every program contained within the object, in libbpf's
    /// own iteration order.
    /// </summary>
    public IEnumerable<BpfProgram> Programs
    {
        get
        {
            var prev = IntPtr.Zero;
            while (true)
            {
                var handle = NativeMethods.bpf_object__next_program(_handle, prev);
                if (handle.IsInvalid)
                    yield break;

                yield return new BpfProgram(handle);
                prev = handle.DangerousGetHandle();
            }
        }
    }

    /// <summary>
    /// Iterates every map contained within the object, in libbpf's own
    /// iteration order.
    /// </summary>
    public IEnumerable<BpfMap> Maps
    {
        get
        {
            var prev = IntPtr.Zero;
            while (true)
            {
                var handle = NativeMethods.bpf_object__next_map(_handle, prev);
                if (handle.IsInvalid)
                    yield break;

                yield return new BpfMap(handle);
                prev = handle.DangerousGetHandle();
            }
        }
    }

    public BpfProgram? FindProgram(string name)
    {
        var handle = NativeMethods.bpf_object__find_program_by_name(_handle, name);
        return handle.IsInvalid ? null : new BpfProgram(handle);
    }

    public BpfMap? FindMap(string name)
    {
        var handle = NativeMethods.bpf_object__find_map_by_name(_handle, name);
        return handle.IsInvalid ? null : new BpfMap(handle);
    }

    /// <summary>
    /// Opens the BPF ELF object at <paramref name="path"/>, performing ELF
    /// parsing but not yet loading anything into the kernel — call
    /// <see cref="Load"/> next.
    /// </summary>
    public static BpfResult<BpfObject> Open(string path)
    {
        var handle = NativeMethods.bpf_object__open(path);
        return handle.IsInvalid
            ? BpfResult<BpfObject>.Failure(BpfError.FromLastError())
            : BpfResult<BpfObject>.Success(new BpfObject(handle));
    }

    /// <summary>
    /// Performs ELF processing, relocations, and map creation, leaving the
    /// object ready for <see cref="Load"/>. Implicitly performed by
    /// <see cref="Load"/> if not called first.
    /// </summary>
    public BpfResult<BpfObject> Prepare() => Execute(NativeMethods.bpf_object__prepare(_handle));

    /// <summary>Loads every autoload-enabled program into the kernel.</summary>
    public BpfResult<BpfObject> Load() => Execute(NativeMethods.bpf_object__load(_handle));

    public BpfResult<BpfObject> Pin(string path) => Execute(NativeMethods.bpf_object__pin(_handle, path));

    public BpfResult<BpfObject> Unpin(string path) => Execute(NativeMethods.bpf_object__unpin(_handle, path));

    public BpfResult<BpfObject> PinMaps(string? path = null) => Execute(NativeMethods.bpf_object__pin_maps(_handle, path));

    public BpfResult<BpfObject> UnpinMaps(string? path = null) => Execute(NativeMethods.bpf_object__unpin_maps(_handle, path));

    public BpfResult<BpfObject> PinPrograms(string path) => Execute(NativeMethods.bpf_object__pin_programs(_handle, path));

    public BpfResult<BpfObject> UnpinPrograms(string path) => Execute(NativeMethods.bpf_object__unpin_programs(_handle, path));

    public void Dispose() => _handle.Dispose();

    private BpfResult<BpfObject> Execute(int returnCode)
    {
        return returnCode == 0 ? BpfResult<BpfObject>.Success(this) : BpfResult<BpfObject>.Failure(BpfError.FromCode(returnCode));
    }
}
