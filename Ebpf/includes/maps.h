#ifndef MAPS_H
#define MAPS_H

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  MAPS
// └────────────────────────────────────┘
//----------------------------------------------------

#include "vmlinux.h"
#include <bpf/bpf_tracing.h>
#include <bpf/bpf_helpers.h>
#include <bpf/bpf_core_read.h>

#define MAX_ENTRY_SIZE      4096

/**
 * Main struct for handling output events using ring buffer.
 */
struct {
    __uint(type, BPF_MAP_TYPE_RINGBUF);
    __uint(max_entries, 4 * MAX_ENTRY_SIZE);
} event_output SEC(".maps");

#endif