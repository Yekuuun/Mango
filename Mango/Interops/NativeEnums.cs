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
/// returns this as a plain C enum (int-sized) by value. Public since it's
/// also the type of <c>BpfProgram.Type</c> in the public API.
/// </summary>
public enum BpfProgramType
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

/// <summary>
/// Mirrors the kernel's <c>enum bpf_map_type</c> (linux/bpf.h), including
/// its deprecated/aliased members, so ordinal values stay in lockstep with
/// what libbpf returns as a plain by-value C enum. Public since it's also
/// the type of <c>BpfMap.Type</c> in the public API.
/// </summary>
public enum BpfMapType
{
    Unspec,
    Hash,
    Array,
    ProgArray,
    PerfEventArray,
    PercpuHash,
    PercpuArray,
    StackTrace,
    CgroupArray,
    LruHash,
    LruPercpuHash,
    LpmTrie,
    ArrayOfMaps,
    HashOfMaps,
    Devmap,
    Sockmap,
    Cpumap,
    Xskmap,
    Sockhash,
    CgroupStorageDeprecated,
    CgroupStorage = CgroupStorageDeprecated,
    ReuseportSockarray,
    PercpuCgroupStorageDeprecated,
    PercpuCgroupStorage = PercpuCgroupStorageDeprecated,
    Queue,
    Stack,
    SkStorage,
    DevmapHash,
    StructOps,
    Ringbuf,
    InodeStorage,
    TaskStorage,
    BloomFilter,
    UserRingbuf,
    CgrpStorage,
    Arena,
}
