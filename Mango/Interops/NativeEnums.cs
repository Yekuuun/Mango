namespace Mango.Interops;

/// <summary>
/// Mirrors libbpf's <c>enum libbpf_print_level</c>.
/// </summary>
internal enum LibbpfPrintLevel
{
    Warn,
    Info,
    Debug,
};

/// <summary>
/// Mirrors the kernel's <c>enum bpf_prog_type</c> (linux/bpf.h). Ordinal
/// values must stay in lockstep with the kernel header since libbpf
/// returns this as a plain C enum (int-sized) by value.
/// </summary>
internal enum BpfProgType
{
    Unspec,
    SocketFilter,
    Kprobe,
    SchedCls,
    SchedAct,
    Tracepoint,
    Xdp,
    PerfEvent,
    CgroupSkb,
    CgroupSock,
    LwtIn,
    LwtOut,
    LwtXmit,
    SockOps,
    SkSkb,
    CgroupDevice,
    SkMsg,
    RawTracepoint,
    CgroupSockAddr,
    LwtSeg6Local,
    LircMode2,
    SkReuseport,
    FlowDissector,
    CgroupSysctl,
    RawTracepointWritable,
    CgroupSockopt,
    Tracing,
    StructOps,
    Ext,
    Lsm,
    SkLookup,
    Syscall,
    Netfilter,
}
