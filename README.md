# Mango

A personal C# wrapper around the native [libbpf](https://elixir.bootlin.com/linux/v6.18.6/source/tools/lib/bpf/libbpf.h) C library, exposing eBPF object/program/map/ring-buffer loading through P/Invoke. Targets **net10.0**, Linux only.

```bash
dotnet add package Mango.Libbpf
```

## What's in the box

| Project | Purpose |
|---|---|
| `Mango/` | The library: public API + P/Invoke bindings onto `libbpf` |
| `Ebpf/` | A sample BPF probe (`sys_kill` kprobe) compiled to `main.bpf.o` |
| `Mango.Poc/` | A runnable console app that loads/attaches the sample probe and prints its events |

## Public API

```csharp
using Mango;

using var obj = BpfObject.Open("probe.bpf.o").Value!;
obj.Load();

var program = obj.FindProgram("kprobe_sys_kill")!;
using var link = program.Attach().Value!;

var map = obj.FindMap("event_output")!;
using var ringBuffer = BpfRingBuffer.Create(map, data =>
{
    // data is a ReadOnlySpan<byte> valid only for this callback
    Console.WriteLine($"{data.Length} bytes received");
}).Value!;

while (true)
    ringBuffer.Poll(timeoutMs: 200);
```

- **`BpfObject`** — opens a `.bpf.o` ELF file (`Open`), performs relocation/map
  creation (`Prepare`, implicit in `Load`), loads programs into the kernel
  (`Load`), pins/unpins the object or its maps/programs, and enumerates its
  `Programs`/`Maps` (or looks one up by name via `FindProgram`/`FindMap`).
  Implements `IDisposable` — closes the underlying `bpf_object` on dispose.
- **`BpfProgram`** — a program inside a loaded object. Exposes `Name`, `Fd`,
  `Type` (`BpfProgramType`), a settable `Autoload` flag, and `Attach()` for
  libbpf's generic auto-detecting attach (kprobe, uprobe, tracepoint, raw
  tracepoint, typed tracing). Owned by its parent `BpfObject` — not
  independently disposable.
- **`BpfMap`** — a map inside a loaded object. Exposes `Name`, `Fd`, `Type`
  (`BpfMapType`), `KeySize`/`ValueSize`/`MaxEntries`, an `IEnumerable<byte[]>`
  of `Keys`, `Pin`/`Unpin`, and `TryLookup`/`TryUpdate`/`TryDelete` for raw
  byte-span element CRUD (`bool`-returning — "not found" is a normal
  outcome, not a failure). Owned by its parent `BpfObject`.
- **`BpfLink`** — the live attachment returned by `BpfProgram.Attach()`.
  `IDisposable` — detaches on dispose.
- **`BpfRingBuffer`** — polls a `BPF_MAP_TYPE_RINGBUF` map. `Create(map,
  onEvent)` wires a managed callback that receives each record's raw bytes;
  `Poll(timeoutMs)` drains pending records and returns how many were
  consumed. `IDisposable`.
- **`BpfResult<T>`** (`Mango.Models`) — the `Result<T, BpfError>` most calls
  return: check `IsSuccess`, then read `Value` or `Error`. `BpfError`
  carries the raw libbpf/kernel error code plus the message rendered by
  libbpf's own `libbpf_strerror()`.

`BpfProgramType`/`BpfMapType` mirror the kernel's `enum bpf_prog_type`/
`enum bpf_map_type` and live in `Mango.Interops` (`NativeEnums.cs`), public
since they're the type of `BpfProgram.Type`/`BpfMap.Type`.

## Running the sample (`Mango.Poc`)

`Mango.Poc` loads `Ebpf/out/main.bpf.o`, attaches its `kprobe_sys_kill`
program to `sys_kill`, and prints every `kill(pid, 64)` call it observes
(process name, pid, ppid, signal). Building the project auto-builds the
probe via `make` in `Ebpf/` and copies `main.bpf.o` next to the app's own
output.

Requirements: Linux, `clang`, kernel/libbpf headers under
`/usr/include/bpf` and `/usr/include/<arch>-linux-gnu` (see `Ebpf/Makefile`
and `Ebpf/vmlinux.sh` to regenerate `vmlinux.h`), and root — loading BPF
programs needs `CAP_BPF`/`CAP_PERFMON` or `sudo`.

```bash
dotnet build Mango.Poc
sudo dotnet Mango.Poc/bin/Debug/net10.0/Mango.Poc
```

## Building the library

```bash
dotnet build Mango/Mango.csproj
```

Packed and published to NuGet as `Mango.Libbpf` on pushing a `v*` tag (see
`.github/workflows/publish-nuget.yml`).

## License

MIT — see [LICENSE](LICENSE).
