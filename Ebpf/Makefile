CC      := clang
ARCH    := x86

SRC_DIR := ./src
INC_DIR := ./includes
OUT_DIR := ./out

# Optional override for a vendored/custom libbpf checkout's header root
# (e.g. `make LIB_DIR=../libbpf/src/root/usr/include`). Left empty by
# default and dropped from BPF_CFLAGS below so an unset LIB_DIR can't
# swallow the next -I flag as its own argument.
LIB_DIR ?=

BPF_SRC    := $(SRC_DIR)/main.bpf.c
BPF_OBJ    := $(OUT_DIR)/main.bpf.o

BPF_CFLAGS := \
	-g \
	-O2 \
	-target bpf \
	-D__bpf__ \
	-D__TARGET_ARCH_$(ARCH) \
	$(if $(LIB_DIR),-I$(LIB_DIR)) \
	-I/usr/include/bpf \
	-I/usr/include/$(shell uname -m)-linux-gnu

DEAMON_LDFLAGS := -lbpf

.PHONY: all clean

all: $(BPF_OBJ) $(DEAMON_BIN)

$(OUT_DIR):
	mkdir -p $(OUT_DIR)

$(BPF_OBJ): $(BPF_SRC) | $(OUT_DIR)
	$(CC) $(BPF_CFLAGS) -c $< -o $@

clean:
	rm -rf $(OUT_DIR)
