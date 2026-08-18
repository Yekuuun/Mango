using System.Buffers.Binary;
using System.Text;

namespace Mango.Poc;

/// <summary>
/// Mirrors the packed layout of Ebpf/includes/common.h's <c>ebpf_event</c> +
/// <c>event_proc_killed</c> for this specific probe. Not a general Mango
/// type — each POC hardcodes the wire format of the one .bpf.o it targets.
/// </summary>
internal readonly record struct ProcKilledEvent(ulong TimestampNs, uint Pid, uint Ppid, uint Signal, string Comm)
{
    // ebpf_event_hdr, packed: u8 type + u16 size + u64 timestamp = 11 bytes.
    private const int HeaderSize = 11;
    private const int TimestampOffset = 3;

    // event_proc_killed, packed: u32 pid + u32 ppid + u32 signal + u32 exit_code + char comm[16].
    private const int PidOffset = HeaderSize;
    private const int PpidOffset = PidOffset + 4;
    private const int SignalOffset = PpidOffset + 4;
    private const int CommOffset = SignalOffset + 4 + 4; // + exit_code
    private const int CommLength = 16;

    public static ProcKilledEvent Parse(ReadOnlySpan<byte> data)
    {
        var timestamp = BinaryPrimitives.ReadUInt64LittleEndian(data[TimestampOffset..]);
        var pid = BinaryPrimitives.ReadUInt32LittleEndian(data[PidOffset..]);
        var ppid = BinaryPrimitives.ReadUInt32LittleEndian(data[PpidOffset..]);
        var signal = BinaryPrimitives.ReadUInt32LittleEndian(data[SignalOffset..]);
        var comm = ReadComm(data.Slice(CommOffset, CommLength));

        return new ProcKilledEvent(timestamp, pid, ppid, signal, comm);
    }

    private static string ReadComm(ReadOnlySpan<byte> raw)
    {
        var nul = raw.IndexOf((byte)0);
        return Encoding.UTF8.GetString(nul < 0 ? raw : raw[..nul]);
    }
}
