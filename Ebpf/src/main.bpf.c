/**
 * Simple hooking using ebpf base techniques.
 * 
 * Note : i'm using kprobes for hooking but consider reading more about ebpf especially LSM since here i'm just hooking for telemetry. NO BLOCKING ACTIONS TAKEN.
 * 
 * i've personnaly read the following book : https://www.amazon.com.be/Learning-eBPF-Programming-Observability-Networking/dp/1098135121/ref=sr_1_1?sr=8-1
 * by O'Reilly
 * 
 * @ressources : https://syscalls64.paolostivanin.com/
 * 
 * @author Yekuuun
 */

#include "../includes/common.h"
#include "../includes/maps.h"

char LICENSE[] SEC("license") = "GPL";

#define SIGNAL_TO_TRACK 64 //signal to track our events.

/**
 * Hook for the __x64_sys_kill syscall.
 * @param regs => current state of registers when in the current context of execution.
 * @link => https://man7.org/linux/man-pages/man2/delete_module.2.html
*/
static int hook_sys_kill(struct pt_regs *regs){
    /**
     * 2 interesting infos to get : 
     * 
     * dx => pid_t pid
     * si => signal sendt
     */

    __u32 pid  = (__u32)PT_REGS_PARM1_CORE(regs);
    int signal = (int)PT_REGS_PARM2_CORE(regs);

    if(signal != SIGNAL_TO_TRACK)
        return 0;

    char comm[COMM_LEN] = {0};
    if(bpf_get_current_comm(comm, sizeof(comm)) < 0)
        return 0;

    struct task_struct *tsk = bpf_get_current_task_btf();
    if(!tsk)
        return 0;

    /**
     * Allocating space in the output ring buffer for sending event.
     */
    ebpf_event *evt = bpf_ringbuf_reserve(&event_output, sizeof(ebpf_event), 0);
    if(!evt)
        return 0;

    //cleaning allocated memory.
    rtl_secure_zero_memory(evt, sizeof(ebpf_event));

    evt->hdr = SET_EVENT_HDR(EVENT_PROC_KILLED, (__u16)sizeof(ebpf_event), bpf_ktime_get_ns());
    evt->payload.proc_killed.pid    = (__u32)pid;
    evt->payload.proc_killed.ppid   = (__u32)tsk->pid;
    evt->payload.proc_killed.signal = (__u32)signal;

    __builtin_memcpy(evt->payload.proc_killed.comm, comm, COMM_LEN);

    bpf_ringbuf_submit(evt, 0);
    return 0;
}

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  CORE HOOKS.
// └────────────────────────────────────┘
//----------------------------------------------------

SEC("kprobe/__x64_sys_kill")
int BPF_KPROBE(kprobe_sys_kill, struct pt_regs *regs)
{
    return hook_sys_kill(regs);
}

//----