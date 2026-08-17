# Mango — Claude Code Guide

## Project overview

Mango is a personal C# wrapper around the native `libbpf` C library, exposing
eBPF object/program loading through P/Invoke. The project targets **net10.0**.

## Solution layout

```
Mango/
├── Interops/             # Raw P/Invoke bindings (NativeMethods) onto libbpf
├── Handles/               # SafeHandle wrappers for native BPF resources
│   ├── BpfObjectHandle.cs # Owns and releases a bpf_object (bpf_object__close)
│   └── BpfProgramHandle.cs
└── Mango.csproj
```

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
  (e.g. `BpfProgramHandle`, which is owned by its parent `bpf_object`)

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
