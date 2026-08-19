```

                     __  ___                     
                    /  |/  /___ _____  ____ _____ 
                   / /|_/ / __ `/ __ \/ __ `/ __ \
                  / /  / / /_/ / / / / /_/ / /_/ /
                 /_/  /_/\__,_/_/ /_/\__, /\____/ 
                                    /____/         

                    ----- C# bindings for libbpf -----

```

<p align="center">
  <a href="https://www.nuget.org/packages/Mango.Libbpf/"><img src="https://img.shields.io/nuget/v/Mango.Libbpf?label=NuGet" alt="NuGet Version"></a>
  <a href="https://www.nuget.org/packages/Mango.Libbpf/"><img src="https://img.shields.io/nuget/dt/Mango.Libbpf" alt="NuGet Downloads"></a>
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4" alt=".NET 10">
  <img src="https://img.shields.io/badge/platform-linux-orange" alt="Linux only">
  <img src="https://img.shields.io/badge/license-MIT-green" alt="MIT License">
</p>

**Mango** is a strongly-typed C# wrapper around the native [libbpf](https://elixir.bootlin.com/linux/v6.18.6/source/tools/lib/bpf/libbpf.h) C library. It exposes eBPF object/program/map/ring-buffer loading through P/Invoke — open a compiled `.bpf.o`, load it into the kernel, attach its programs, and read its maps/ring buffers — behind a small, `Result`-returning API, so you don't have to hand-roll the native bindings yourself.

> [!IMPORTANT]
> This is a personal project built to learn the libbpf C API and P/Invoke internals from the ground up. It's shared as-is — feel free to use it, fork it, or build on top of it.

---

## Table of contents

- [Features](#features)
- [Installation](#installation)
- [Quickstart](#quickstart)
- [Project structure](#project-structure)
- [Domain reference](#domain-reference)
- [Error handling](#error-handling)
- [Requirements](#requirements)
- [Building from source](#building-from-source)
- [Resources](#resources)
- [License](#license)

---

## Features

| Domain | Type | Covers |
|---|---|---|
| **Objects** | `BpfObject` | Open a `.bpf.o` ELF file, prepare/load it into the kernel, pin/unpin it or its maps/programs, enumerate/find programs and maps |
| **Programs** | `BpfProgram` | Inspect name/fd/type, toggle autoload, attach via libbpf's generic auto-detection (kprobe, uprobe, tracepoint, raw tracepoint, typed tracing) |
| **Maps** | `BpfMap` | Inspect name/fd/type/sizes, iterate keys, pin/unpin, lookup/update/delete elements by raw byte span |
| **Links** | `BpfLink` | The live attachment returned by `Attach()` — detaches on dispose |
| **Ring buffers** | `BpfRingBuffer` | Poll a `BPF_MAP_TYPE_RINGBUF` map with a managed callback per record |

Every public type:
- targets **.NET 10** and ships with full nullable-reference-type annotations
- returns `BpfResult<T>` instead of throwing for expected native-call failures
- wraps its native handle in a `SafeHandle` and implements `IDisposable` where the underlying libbpf object owns kernel resources
- keeps public naming aligned with the `bpf_*__*` libbpf call it wraps, so the native docs stay a direct reference

---

## Installation

```bash
dotnet add package Mango.Libbpf
```

Or via the NuGet Package Manager:

```
Install-Package Mango.Libbpf
```

---

## Quickstart

```csharp
using Mango;

using var obj = BpfObject.Open("probe.bpf.o").Value!;
obj.Load();

var program = obj.FindProgram("kprobe_sys_kill")!;
using var link = program.Attach().Value!;

var map = obj.FindMap("event_output")!;
using var ringBuffer = BpfRingBuffer.Create(map, data =>
{
    // data is a ReadOnlySpan<byte>, valid only for the duration of this call
    Console.WriteLine($"{data.Length} bytes received");
}).Value!;

while (true)
    ringBuffer.Poll(timeoutMs: 200);
```

Every domain follows the same pattern: open/find the object you need, check the returned `BpfResult<T>.IsSuccess` (or unwrap `.Value!` once you trust the call), and dispose whatever owns a native handle (`BpfObject`, `BpfLink`, `BpfRingBuffer`) when you're done.

---

## Project structure

```
Mango/
├── Interops/              # Raw P/Invoke bindings (NativeMethods) onto libbpf
│   ├── NativeBpfMethods.cs # [DllImport] extern declarations, grouped by #region
│   ├── NativeEnums.cs      # Enums mirroring kernel enums (bpf_prog_type, bpf_map_type, ...)
│   └── NativeHelpers.cs    # Delegate types for native callbacks (libbpf_set_print, ring buffer sample fn)
├── Handles/                # SafeHandle wrappers for native BPF resources
├── Models/                 # BpfError / BpfResult<T> — the Result<T, BpfError> vocabulary
├── BpfObject.cs             # Public API: Open/Prepare/Load/pin-unpin/Programs/Maps
├── BpfProgram.cs             # Public API: wraps BpfProgramHandle (Type/Autoload/Attach)
├── BpfMap.cs                  # Public API: wraps BpfMapHandle (CRUD/Keys/pin-unpin)
├── BpfLink.cs                  # Public API: wraps BpfLinkHandle (IDisposable)
└── BpfRingBuffer.cs              # Public API: wraps BpfRingBufferHandle (Create/Poll)
```

Alongside the library:

```
Ebpf/         # Sample BPF probe (sys_kill kprobe) compiled to main.bpf.o via `make`
Mango.Poc/    # Runnable console app that loads/attaches the sample probe and prints its events
```

Every native call funnels through `Interops/NativeBpfMethods.cs`'s `NativeMethods` — the only place `[DllImport]` declarations live — and errors are rendered through libbpf's own `libbpf_strerror()`.

---

## Domain reference

<details>
<summary><strong>BpfObject</strong> — open, load, pin, enumerate</summary>

```csharp
using var obj = BpfObject.Open("probe.bpf.o").Value!;
```

| Member | libbpf call |
|---|---|
| `Open(path)` | `bpf_object__open` |
| `Prepare()` | `bpf_object__prepare` |
| `Load()` | `bpf_object__load` |
| `Name` | `bpf_object__name` |
| `Programs` / `FindProgram(name)` | `bpf_object__next_program` / `bpf_object__find_program_by_name` |
| `Maps` / `FindMap(name)` | `bpf_object__next_map` / `bpf_object__find_map_by_name` |
| `Pin(path)` / `Unpin(path)` | `bpf_object__pin` / `bpf_object__unpin` |
| `PinMaps(path?)` / `UnpinMaps(path?)` | `bpf_object__pin_maps` / `bpf_object__unpin_maps` |
| `PinPrograms(path)` / `UnpinPrograms(path)` | `bpf_object__pin_programs` / `bpf_object__unpin_programs` |
| `Dispose()` | `bpf_object__close` |

`Load()` implicitly performs `Prepare()` if it wasn't called first.

</details>

<details>
<summary><strong>BpfProgram</strong> — inspect and attach</summary>

```csharp
var program = obj.FindProgram("kprobe_sys_kill")!;
using var link = program.Attach().Value!;
```

| Member | libbpf call |
|---|---|
| `Name` | `bpf_program__name` |
| `Fd` | `bpf_program__fd` |
| `Type` | `bpf_program__type` |
| `Autoload` (get/set) | `bpf_program__autoload` / `bpf_program__set_autoload` |
| `Attach()` | `bpf_program__attach` |

`Autoload` must be set before the parent object is loaded. Its setter throws `InvalidOperationException` on failure — that's a programmer error, not an expected outcome. `Attach()` uses libbpf's generic auto-detection: kprobe, uprobe, tracepoint, raw tracepoint, and typed tracing programs.

</details>

<details>
<summary><strong>BpfMap</strong> — inspect and CRUD elements</summary>

```csharp
var map = obj.FindMap("event_output")!;
map.TryLookup(key, value);
```

| Member | libbpf call |
|---|---|
| `Name` / `Fd` / `Type` | `bpf_map__name` / `bpf_map__fd` / `bpf_map__type` |
| `KeySize` / `ValueSize` / `MaxEntries` | `bpf_map__key_size` / `bpf_map__value_size` / `bpf_map__max_entries` |
| `Keys` | `bpf_map__get_next_key` |
| `Pin(path?)` / `Unpin(path?)` | `bpf_map__pin` / `bpf_map__unpin` |
| `TryLookup` / `TryUpdate` / `TryDelete` | `bpf_map__lookup_elem` / `bpf_map__update_elem` / `bpf_map__delete_elem` |

The `Try*` methods return `bool` instead of `BpfResult<T>` — "not found" is a normal outcome for map element CRUD, matching the `Dictionary.TryGetValue` idiom.

</details>

<details>
<summary><strong>BpfLink</strong> — the live attachment</summary>

```csharp
using var link = program.Attach().Value!;
```

| Member | libbpf call |
|---|---|
| `Dispose()` | `bpf_link__destroy` |

Returned by `BpfProgram.Attach()`. Disposing detaches the program from its hook.

</details>

<details>
<summary><strong>BpfRingBuffer</strong> — poll BPF_MAP_TYPE_RINGBUF</summary>

```csharp
using var rb = BpfRingBuffer.Create(map, data => Console.WriteLine(data.Length)).Value!;
while (true) rb.Poll(timeoutMs: 200);
```

| Member | libbpf call |
|---|---|
| `Create(map, onEvent)` | `ring_buffer__new` |
| `Poll(timeoutMs)` | `ring_buffer__poll` |
| `Dispose()` | `ring_buffer__free` |

`onEvent` is invoked with each record's raw bytes during `Poll`; the `ReadOnlySpan<byte>` is only valid for the duration of that call. `Poll` returns the number of records consumed.

</details>

---

## Error handling

Mango follows the shape of the native call it wraps, rather than one uniform rule:

- **Int-returning calls** (0/negative error code) — like `Load`, `Pin`, `TryUpdate`'s underlying call — build a `BpfResult<T>` via `BpfError.FromCode(rc)`.
- **Pointer/handle-returning calls** with no other numeric signal — like `Open`, `Attach` — build a `BpfResult<T>` via `BpfError.FromLastError()`, reading `Marshal.GetLastPInvokeError()`.
- **Map element CRUD** exposes `Try*(...) : bool` instead of `BpfResult<T>` — "not found" is a normal outcome, not a failure.

```csharp
public readonly record struct BpfError(int Code, string Message);

public sealed record BpfResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public BpfError? Error { get; }
}
```

`BpfError.Message` is rendered by libbpf's own `libbpf_strerror()`, so it matches what a native libbpf/bpftool user would see. Always check `IsSuccess` before reading `Value`:

```csharp
var result = BpfObject.Open(path);
if (!result.IsSuccess)
{
    Console.Error.WriteLine(result.Error);
    return;
}
```

---

## Requirements

- .NET 10.0 SDK or later
- Linux with `libbpf` installed (the native library Mango P/Invokes into)
- Root, or `CAP_BPF`/`CAP_PERFMON`, to load BPF programs into the kernel

---

## Building from source

```bash
git clone https://github.com/Yekuuun/Mango.git
cd Mango

# Build the library
dotnet build Mango/Mango.csproj

# Build + run the sample: loads Ebpf/out/main.bpf.o, attaches its sys_kill
# kprobe, and prints every kill(pid, 64) it observes. Building auto-runs
# `make` in Ebpf/ and copies main.bpf.o next to the app's own output.
dotnet build Mango.Poc
sudo dotnet Mango.Poc/bin/Debug/net10.0/Mango.Poc
```

Packed and published to NuGet as `Mango.Libbpf` on pushing a `v*` tag (see [`.github/workflows/publish-nuget.yml`](.github/workflows/publish-nuget.yml)).

---

## Resources

- [NuGet package](https://www.nuget.org/packages/Mango.Libbpf/)
- [libbpf source (`libbpf.h`)](https://elixir.bootlin.com/linux/v6.18.6/source/tools/lib/bpf/libbpf.h)
- [Issues & contributions](https://github.com/Yekuuun/Mango/issues)

---

## License

MIT — see [`LICENSE`](LICENSE).
