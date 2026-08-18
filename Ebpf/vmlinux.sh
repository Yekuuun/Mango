#!/bin/bash

# Generates kernel/includes/vmlinux.h for CO-RE eBPF compilation.
# Requires bpftool:
# Ubuntu/Debian : sudo apt install -y linux-tools-$(uname -r)
# Fedora/RHEL   : sudo dnf install -y bpftool

set -euo pipefail

# bpftool ships in /usr/sbin on Debian/Ubuntu, which isn't always on PATH.
export PATH="$PATH:/usr/sbin:/sbin"

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT="$ROOT/includes/vmlinux.h"

mkdir -p "$(dirname "$OUTPUT")"
rm -f "$OUTPUT"
bpftool btf dump file /sys/kernel/btf/vmlinux format c > "$OUTPUT"
echo "vmlinux.h generated → $OUTPUT"