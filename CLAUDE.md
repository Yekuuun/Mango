# Mango — Claude Code Guide

## Project overview

Mango is a personal C# wrapper around the native `libbpf` C library, exposing
eBPF object/program loading through P/Invoke. The project targets **net10.0**.

## Solution layout

```
Mango/
├── Interops/                # Raw P/Invoke bindings (NativeMethods) onto libbpf
│   ├── NativeBpfMethods.cs  # [DllImport] extern declarations, grouped by #region
│   ├── NativeEnums.cs       # Enums mirroring kernel enums (bpf_prog_type, bpf_map_type, ...)
│   └── NativeHelpers.cs     # Delegate types for native callbacks (libbpf_set_print)
├── Handles/                 # SafeHandle wrappers for native BPF resources
│   ├── BpfObjectHandle.cs   # Owns and releases a bpf_object (bpf_object__close)
│   ├── BpfProgramHandle.cs  # Non-owning — lifetime belongs to its parent bpf_object
│   ├── BpfMapHandle.cs      # Non-owning — lifetime belongs to its parent bpf_object
│   └── BpfLinkHandle.cs     # Owns and releases a bpf_link (bpf_link__destroy)
├── Models/                  # Result/error value types for the public API
│   ├── BpfError.cs          # errno/return-code + libbpf_strerror() rendered as a value
│   └── BpfResult.cs         # BpfResult<T> — Result<T, BpfError> for the public API
├── BpfObject.cs              # Public API: Open/Prepare/Load/pin-unpin/Programs/Maps
├── BpfProgram.cs              # Public API: wraps BpfProgramHandle (Type/Autoload/Attach)
├── BpfMap.cs                   # Public API: wraps BpfMapHandle (CRUD/Keys/pin-unpin)
├── BpfLink.cs                   # Public API: wraps BpfLinkHandle (IDisposable)
└── Mango.csproj
```

### Public API layer (M4)

- The public surface (`BpfObject`, `BpfProgram`, `BpfMap`, `BpfLink`) lives at
  the project root, in the `Mango` namespace — not under `Handles/` or
  `Interops/`, which stay internal plumbing.
- `BpfError`/`BpfResult<T>` live under `Models/`, in the `Mango.Models`
  namespace — the shared result/error vocabulary the public API returns.
- `BpfProgramType`/`BpfMapType` (mirroring the kernel's `bpf_prog_type`/
  `bpf_map_type`) are **public enums declared in `Interops/NativeEnums.cs`**,
  colocated with the native signatures they mirror, rather than duplicated
  as separate public-layer types — one definition avoids ordinal drift
  between an internal and a public copy.
- Error handling follows the native call's own shape rather than one
  uniform rule:
  - Int-returning calls (0/negative-error-code) → `BpfError.FromCode(rc)`
    used to build a `BpfResult<T>` — the negative code doesn't depend on
    `errno`/`SetLastError` timing.
  - Pointer/handle-returning calls with `SetLastError = true` and no
    other numeric signal (`bpf_object__open`, `bpf_program__attach`) →
    `BpfError.FromLastError()` (reads `Marshal.GetLastPInvokeError()`).
  - Map element CRUD (`bpf_map__lookup/update/delete_elem`) exposes
    `Try*(...) : bool` instead of `BpfResult<T>` — "not found" is a normal
    outcome there, matching the `Dictionary.TryGetValue` idiom.
  - `Autoload`'s setter throws `InvalidOperationException` on failure
    instead of returning a result — failing to set autoload on a valid
    handle is a programmer error, not an expected outcome.

## Architecture rules

### Adding a new libbpf binding

1. **P/Invoke declaration** — add the `[DllImport("libbpf", SetLastError = true)]`
   extern method to `Interops/NativeBpfMethods.cs`, grouped under its
   `#region` (e.g. `OBJECT`, `PROGRAM`)
2. **String marshaling** — libbpf is a native C library expecting narrow
   strings; always marshal `string` parameters/returns as
   `[MarshalAs(UnmanagedType.LPUTF8Str)]`, never `LPWStr`
3. **Error reporting** — set `SetLastError = true` on any binding whose
   native doc comment states the error code is stored in `errno`
4. **Handles** — native resources returned as opaque pointers get a
   `SafeHandle` under `Handles/` that releases them via the matching
   libbpf `*__close`/`*__destroy` call in `ReleaseHandle()`
5. Keep the native function's doc comment (`/** @brief ... */`) above the
   binding, mirroring libbpf's own header documentation

### Key conventions

- `internal static class NativeMethods` in `Interops/` is the only place
  `[DllImport]` declarations live — no scattered P/Invoke elsewhere
- `SafeHandle` subclasses are `internal sealed` and own their handle
  (`ownsHandle: true`) unless the native object is non-owning by design
  (e.g. `BpfProgramHandle`/`BpfMapHandle`, both owned by their parent
  `bpf_object`)

## Active skills

Always apply these skills without being asked:

| Skill | When |
|---|---|
| `csharp-coding-standards` | Any new or refactored C# code |
| `git` | Every commit message or branch name |

## Commits & branches

Follow `.claude/skills/git/SKILL.md` for all commit messages and branch
naming. Always invoke `/commit` to generate commits.

## Safety

A pre-tool-use Bash hook (`validate-bash-dotnet.sh`) blocks destructive
commands (`rm`, SQL deletes, `sudo`) and warns on `git push`/`reset`/`rebase`.
Do not bypass it.

## Official LIBBPF documentation link 
https://elixir.bootlin.com/linux/v6.18.6/source/tools/lib/bpf/libbpf.h
