using Mango;

namespace Mango.Poc;

/// <summary>
/// Static POC: loads Ebpf/out/main.bpf.o, attaches its sys_kill kprobe, and
/// prints every kill(pid, 64) event it reports. Hardcoded to this one
/// probe's object path/program/map names on purpose — a different .bpf.o
/// means different operations, so this isn't meant to be reusable.
/// </summary>
internal static class Program
{
    // The BuildAndCopyEbpfProbe MSBuild target drops main.bpf.o next to this
    // assembly's own output, so this resolves the same way no matter where
    // `dotnet run`/`dotnet build`/the IDE was launched from.
    private static readonly string ObjectPath = Path.Combine(AppContext.BaseDirectory, "main.bpf.o");
    private const string ProgramName = "kprobe_sys_kill";
    private const string MapName = "event_output";

    internal static int Main()
    {
        if (!File.Exists(ObjectPath))
        {
            Console.Error.WriteLine($"'{ObjectPath}' not found — build via `dotnet build`/`dotnet run` so the BuildAndCopyEbpfProbe target can run.");
            return 1;
        }

        var openResult = BpfObject.Open(ObjectPath);
        if (!openResult.IsSuccess)
        {
            Console.Error.WriteLine($"failed to open {ObjectPath}: {openResult.Error}");
            return 1;
        }

        using var obj = openResult.Value!;

        var loadResult = obj.Load();
        if (!loadResult.IsSuccess)
        {
            Console.Error.WriteLine($"failed to load object (are you root?): {loadResult.Error}");
            return 1;
        }

        var program = obj.FindProgram(ProgramName);
        if (program is null)
        {
            Console.Error.WriteLine($"program '{ProgramName}' not found in {ObjectPath}");
            return 1;
        }

        var attachResult = program.Attach();
        if (!attachResult.IsSuccess)
        {
            Console.Error.WriteLine($"failed to attach '{ProgramName}': {attachResult.Error}");
            return 1;
        }

        using var link = attachResult.Value!;

        var map = obj.FindMap(MapName);
        if (map is null)
        {
            Console.Error.WriteLine($"map '{MapName}' not found in {ObjectPath}");
            return 1;
        }

        var ringBufferResult = BpfRingBuffer.Create(map, OnEvent);
        if (!ringBufferResult.IsSuccess)
        {
            Console.Error.WriteLine($"failed to create ring buffer over '{MapName}': {ringBufferResult.Error}");
            return 1;
        }

        using var ringBuffer = ringBufferResult.Value!;

        var running = true;
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            running = false;
        };

        Console.WriteLine($"hooked {ProgramName} — waiting for kill(pid, 64). Ctrl+C to stop.");

        while (running)
            ringBuffer.Poll(timeoutMs: 200);

        return 0;
    }

    private static void OnEvent(ReadOnlySpan<byte> data)
    {
        var evt = ProcKilledEvent.Parse(data);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {evt.Comm} (pid={evt.Pid}, ppid={evt.Ppid}) sent signal {evt.Signal}");
    }
}
