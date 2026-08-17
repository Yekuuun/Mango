<!--
Sync Impact Report
==================
Version change: (unratified template) → 1.0.0
Rationale: Initial ratification. No prior constitution content existed (the file on
disk was the raw, unfilled template scaffold), so this is treated as first adoption
rather than an amendment — hence MAJOR version 1.0.0.

Modified principles: none (first draft)
Added sections:
  - Core Principles (I–V): Faithful libbpf Semantics, Safe Native Resource Ownership,
    Explicit Error Propagation, Incremental & Honest API Coverage, Minimal Public Surface
  - Technology & Platform Constraints
  - Development Workflow
  - Governance

Removed sections: none

Templates requiring alignment review (not modified by this command — consumers read
this file at runtime):
  - .specify/templates/plan-template.md — ✅ no conflicting assumptions found
  - .specify/templates/spec-template.md — ✅ no conflicting assumptions found
  - .specify/templates/tasks-template.md — ✅ no conflicting assumptions found

Deferred placeholders / TODOs: none. RATIFICATION_DATE is set to the date this
constitution was first authored, since no earlier ratified version exists.
-->

# Mango Constitution
<!-- Mango: a personal C# wrapper around the libbpf C library -->

## Core Principles

### I. Faithful libbpf Semantics
Every wrapped function MUST preserve the upstream libbpf function's naming, parameter
order, and behavioral contract as documented in the official libbpf headers/docs.
`NativeMethods` members MUST carry a doc comment summarizing the upstream `@brief`,
parameters, and return-value contract (mirroring the pattern already established in
`Interops/NativeBpfMethods.cs`), so the C# signature is traceable back to the C API it
binds. Renaming, reordering, or reshaping behavior for "nicer" C# ergonomics MUST happen
in a higher-level wrapper type, never by silently diverging from the native contract at
the P/Invoke layer.
**Rationale**: This project's entire value is being a trustworthy mirror of libbpf. If
the P/Invoke layer drifts from upstream semantics, every consumer building on it inherits
a silent correctness bug that is expensive to trace back to a native ABI mismatch.

### II. Safe Native Resource Ownership
Every native handle returned by libbpf (`bpf_object *`, `bpf_program *`, map/link/prog
fds, etc.) MUST be wrapped in a `SafeHandle` subclass before it is exposed above the
`Interops` layer. `ReleaseHandle()` MUST call the correct matching libbpf teardown
function (e.g. `bpf_object__close` for a handle obtained via `bpf_object__open*`), and
`ownsHandle` MUST correctly reflect whether this handle is responsible for releasing the
underlying resource (see `BpfObjectHandle` vs. the non-owning `BpfProgramHandle`). Raw
`IntPtr` values for native BPF objects MUST NOT cross out of the `Interops` namespace.
**Rationale**: libbpf resources are kernel-backed (fds, loaded programs, maps); leaking
them or double-freeing them has effects outside the process (leaked kernel objects,
crashes) that ordinary GC-finalizer bugs in managed code don't have.

### III. Explicit Error Propagation
libbpf reports failure via `NULL`/negative return codes plus `errno`. Wrapper code MUST
check for these failure signals at the P/Invoke boundary and translate them into a
thrown .NET exception carrying the native error code (and `errno` where applicable)
rather than returning a null/negative value to managed callers or swallowing the error.
No method above the `Interops` layer may treat a failed libbpf call as a normal, silent
return path.
**Rationale**: Silent native failures are the hardest class of bug to diagnose in an
interop library — the failure surfaces far from its native cause unless it's converted
into a catchable, informative exception at the boundary.

### IV. Incremental & Honest API Coverage
Mango does not need to wrap the full libbpf surface before it is useful. Unimplemented
libbpf functions MUST be left unwrapped and marked with a `// TODO` (as already practiced
in `NativeMethods`) rather than stubbed out with fake/no-op implementations or partial
wrappers that silently ignore parameters. A function is only added to `NativeMethods`
when a concrete caller needs it.
**Rationale**: This is a personal, incrementally-grown wrapper, not a committed
full-coverage binding. Speculative stubs create the illusion of support and are a worse
outcome than an honest compile error from a missing method.

### V. Minimal Public Surface
Types and members default to `internal` (as `BpfObjectHandle`, `BpfProgramHandle`, and
`NativeMethods` already are). A type or member becomes `public` only as a deliberate,
reviewed decision when a real external consumer/use case needs it — not by default and
not preemptively for hypothetical future use.
**Rationale**: Once a member is public it is part of the compatibility surface; keeping
the default `internal` preserves freedom to reshape the low-level bindings while the
wrapper is still being discovered/designed.

## Technology & Platform Constraints

- Target framework: `net10.0`, with `<Nullable>` and `<ImplicitUsings>` enabled
  project-wide; new code MUST NOT disable either without a documented reason.
- Mango targets Linux/eBPF via the native `libbpf` shared library only. No
  cross-platform shim, mock, or "no-op on unsupported OS" fallback path is to be added —
  if libbpf isn't present, failure is expected and acceptable.
- P/Invoke declarations live in `Mango/Interops`; native-handle wrapper types live in
  `Mango/Handles`. New interop surface follows this same split rather than inlining
  `DllImport`s into consumer code.

## Development Workflow

- This is a single-maintainer personal project: there is no mandatory PR-review gate,
  but every change MUST still be checked against the Core Principles above before being
  committed, particularly Principle II (handle ownership) and Principle III (error
  propagation), since native-resource and error-handling bugs are the costliest to find
  later.
- No test suite exists yet; when tests are introduced they MUST be able to run only on a
  Linux host with libbpf/eBPF available, and MUST be clearly separated from any future
  pure-managed unit tests that don't require kernel support.

## Governance

This constitution supersedes ad-hoc conventions for any conflict between them and the
principles above. Amendments are made by editing this file directly (via
`/speckit-constitution` or a manual edit) and MUST update the Sync Impact Report and the
version line in the same change.

**Versioning policy** (semantic versioning applied to governance, not to the library's
own release versioning):
- MAJOR: backward-incompatible principle removal or redefinition.
- MINOR: a new principle or materially expanded section is added.
- PATCH: wording clarifications, typo fixes, non-semantic edits.

Compliance review: before implementation planning (`/speckit-plan`) or task generation
(`/speckit-tasks`) for a feature, re-check the plan/tasks against these principles,
especially Principles II–IV, and record any justified deviation in that feature's plan
rather than silently ignoring it.

## Custom skills
- .claude/skills/git/SKILL.md — conventions Git du projet
- .claude/skills/csharp_coding_standards/SKILL.md — standards C# à respecter

**Version**: 1.0.0 | **Ratified**: 2026-08-17 | **Last Amended**: 2026-08-17
