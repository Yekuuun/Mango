#ifndef COMMON_H
#define COMMON_H

#if defined(__bpf__)
#include "vmlinux.h"
#include <bpf/bpf_tracing.h>
#include <bpf/bpf_helpers.h>
#include <bpf/bpf_core_read.h>
#else
#include <linux/types.h>
#endif

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  GLOBAL
// └────────────────────────────────────┘ 
//----------------------------------------------------
#define MAX_PATH            256
#define COMM_LEN            16  /*TASK_COMM_LEN*/

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  UTILS
// └────────────────────────────────────┘
//----------------------------------------------------

/**
 * Zeroes out size bytes at ptr. libc's memset isn't available in BPF
 * context, so this leans on the compiler builtin instead.
 */
static __always_inline void rtl_secure_zero_memory(void *ptr, __u32 size)
{
    __builtin_memset(ptr, 0, size);
}

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  EVENT TYPES
// └────────────────────────────────────┘
//----------------------------------------------------
#define EVENT_PROC_KILLED    1

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  PAYLOAD STRUCTS
// └────────────────────────────────────┘ 
//----------------------------------------------------

typedef struct event_proc_killed {
    __u32 pid;
    __u32 ppid;
    __u32 signal;
    __u32 exit_code;
    char  comm[COMM_LEN];
} __attribute__((packed)) event_proc_killed;

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  PAYLOAD ENTITIES
// └────────────────────────────────────┘ 
//----------------------------------------------------
typedef struct ebpf_event_hdr {
    __u8  type; //enough for event types.
    __u16 size;
    __u64 timestamp;
} __attribute__((packed)) ebpf_event_hdr;

/**
 * Utility function for setting hdr attributes.
 */
#define SET_EVENT_HDR(_type, _size, _timestamp) \
((ebpf_event_hdr){                           \
    .type      = (_type),                       \
    .size      = (_size),                       \
    .timestamp = (_timestamp)                   \
})                                              \

typedef struct ebpf_event {
    struct ebpf_event_hdr hdr;

    /*UNIONS*/
    union common {
        struct event_proc_killed    proc_killed;

        /*OTHER EVENTS*/
    } payload;
    
} __attribute__((packed)) ebpf_event;

#endif