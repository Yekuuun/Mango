using System.Runtime.InteropServices;
using Mango.Handles;

namespace Mango.Interops;

internal static class NativeMethods
{
    #region OBJECT

    /**
    * @brief **bpf_object__open()** creates a bpf_object by opening
    * the BPF ELF object file pointed to by the passed path and loading it
    * into memory.
    * @param path BPF object file path.
    * @return pointer to the new bpf_object; or NULL is returned on error,
    * error code is stored in errno
    */
    [DllImport("libbpf", SetLastError = true)]
    public static extern BpfObjectHandle bpf_object__open([MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    /**
    * @brief **bpf_object__open_file()** creates a bpf_object by opening
    * the BPF ELF object file pointed to by the passed path and loading it
    * into memory.
    * @param path BPF object file path
    * @param opts options for how to load the bpf object, this parameter is
    * optional and can be set to NULL
    * @return pointer to the new bpf_object; or NULL is returned on error,
    * error code is stored in errno
    */
    [DllImport("libbpf", SetLastError = true)]
    public static extern BpfObjectHandle bpf_object__open_file([MarshalAs(UnmanagedType.LPUTF8Str)] string path, IntPtr opts);

    /**
    * @brief **bpf_object__open_mem()** creates a bpf_object by reading
    * the BPF objects raw bytes from a memory buffer containing a valid
    * BPF ELF object file.
    * @param obj_buf pointer to the buffer containing ELF file bytes
    * @param obj_buf_sz number of bytes in the buffer
    * @param opts options for how to load the bpf object
    * @return pointer to the new bpf_object; or NULL is returned on error,
    * error code is stored in errno
    */
    [DllImport("libbpf", SetLastError = true)]
    public static extern BpfObjectHandle bpf_object__open_mem(IntPtr objBuf, [MarshalAs(UnmanagedType.SysInt)] int objBufSz, IntPtr opts);

    /**
    * @brief **bpf_object__prepare()** prepares BPF object for loading:
    * performs ELF processing, relocations, prepares final state of BPF program
    * instructions (accessible with bpf_program__insns()), creates and
    * (potentially) pins maps. Leaves BPF object in the state ready for program
    * loading.
    * @param obj Pointer to a valid BPF object instance returned by
    * **bpf_object__open*()** API
    * @return 0, on success; negative error code, otherwise, error code is
    * stored in errno
    */
    [DllImport("libbpf", SetLastError = true)]
    public static extern int bpf_object__prepare(BpfObjectHandle obj);

    /**
    * @brief **bpf_object__load()** loads BPF object into kernel.
    * @param obj Pointer to a valid BPF object instance returned by
    * **bpf_object__open*()** APIs
    * @return 0, on success; negative error code, otherwise, error code is
    * stored in errno
    */
    [DllImport("libbpf", SetLastError = true)]
    public static extern int bpf_object__load(BpfObjectHandle obj);

    /**
    * @brief **bpf_object__name()** retrieves the name of the BPF object.
    * @param obj Pointer to a valid BPF object
    * @return the object's name
    */
    [DllImport("libbpf")]
    [return: MarshalAs(UnmanagedType.LPUTF8Str)]
    public static extern string bpf_object__name(BpfObjectHandle obj);

    /**
    * @brief **bpf_object__pin()** pins all programs and maps contained
    * within the BPF object at the provided path.
    * @param obj Pointer to a valid BPF object
    * @param path path to pin BPF object programs and maps under, will
    * attempt to create the path if it does not already exist
    * @return 0, on success; negative error code, otherwise, error code is
    * stored in errno
    */
    [DllImport("libbpf", SetLastError = true)]
    public static extern int bpf_object__pin(BpfObjectHandle obj, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    /**
    * @brief **bpf_object__unpin()** unpins all programs and maps contained
    * within the BPF object that were pinned at the provided path.
    * @param obj Pointer to a valid BPF object
    * @param path path programs and maps were pinned under
    * @return 0, on success; negative error code, otherwise, error code is
    * stored in errno
    */
    [DllImport("libbpf", SetLastError = true)]
    public static extern int bpf_object__unpin(BpfObjectHandle obj, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    /**
    * @brief **bpf_object__pin_maps()** pins each map contained within the
    * BPF object at the provided path.
    * @param obj Pointer to a valid BPF object
    * @param path path under which maps are to be pinned; if NULL, each
    * map's own pin_path is used instead
    * @return 0, on success; negative error code, otherwise, error code is
    * stored in errno
    */
    [DllImport("libbpf", SetLastError = true)]
    public static extern int bpf_object__pin_maps(BpfObjectHandle obj, [MarshalAs(UnmanagedType.LPUTF8Str)] string? path);

    /**
    * @brief **bpf_object__unpin_maps()** unpins each map contained within
    * the BPF object that was pinned at the provided path.
    * @param obj Pointer to a valid BPF object
    * @param path path maps were pinned under; if NULL, each map's own
    * pin_path is used instead
    * @return 0, on success; negative error code, otherwise, error code is
    * stored in errno
    */
    [DllImport("libbpf", SetLastError = true)]
    public static extern int bpf_object__unpin_maps(BpfObjectHandle obj, [MarshalAs(UnmanagedType.LPUTF8Str)] string? path);

    /**
    * @brief **bpf_object__pin_programs()** pins each program contained
    * within the BPF object at the provided path.
    * @param obj Pointer to a valid BPF object
    * @param path path under which programs are to be pinned
    * @return 0, on success; negative error code, otherwise, error code is
    * stored in errno
    */
    [DllImport("libbpf", SetLastError = true)]
    public static extern int bpf_object__pin_programs(BpfObjectHandle obj, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    /**
    * @brief **bpf_object__unpin_programs()** unpins each program contained
    * within the BPF object that was pinned at the provided path.
    * @param obj Pointer to a valid BPF object
    * @param path path programs were pinned under
    * @return 0, on success; negative error code, otherwise, error code is
    * stored in errno
    */
    [DllImport("libbpf", SetLastError = true)]
    public static extern int bpf_object__unpin_programs(BpfObjectHandle obj, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    /**
    * @brief **bpf_object__close()** closes a BPF object and releases all
    * resources.
    * @param obj Pointer to a valid BPF object
    */
    [DllImport("libbpf")]
    public static extern void bpf_object__close(IntPtr handle);

    //TO DO => declare other functions.

    #endregion

    #region PROGRAM

    /**
    * @brief **bpf_object__find_program_by_name()** locates a BPF program by
    * name within the object.
    * @param obj Pointer to a valid BPF object
    * @param name name of the BPF program
    * @return BPF program instance, if such program exists within the BPF
    * object; or NULL otherwise
    */
    [DllImport("libbpf", SetLastError = true)]
    public static extern BpfProgramHandle bpf_object__find_program_by_name(BpfObjectHandle obj, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    /**
    * @brief **bpf_object__next_program()** iterates over the programs
    * contained within the BPF object.
    * @param obj Pointer to a valid BPF object
    * @param prev the previous program returned by this function, or
    * `IntPtr.Zero` to fetch the first program
    * @return the next BPF program contained within the BPF object; or
    * an invalid handle if `prev` was the last program
    */
    [DllImport("libbpf")]
    public static extern BpfProgramHandle bpf_object__next_program(BpfObjectHandle obj, IntPtr prev);

    /**
    * @brief **bpf_program__fd()** retrieves the file descriptor for the
    * loaded BPF program.
    * @param prog BPF program to get the file descriptor for
    * @return file descriptor of the program, or negative error code
    */
    [DllImport("libbpf")]
    public static extern int bpf_program__fd(BpfProgramHandle prog);

    /**
    * @brief **bpf_program__name()** retrieves the name of the BPF program.
    * @param prog BPF program
    * @return the program's name
    */
    [DllImport("libbpf")]
    [return: MarshalAs(UnmanagedType.LPUTF8Str)]
    public static extern string bpf_program__name(BpfProgramHandle prog);

    /**
    * @brief **bpf_program__type()** retrieves the BPF program type.
    * @param prog BPF program
    * @return the program's `bpf_prog_type`
    */
    [DllImport("libbpf")]
    public static extern BpfProgType bpf_program__type(BpfProgramHandle prog);

    /**
    * @brief **bpf_program__autoload()** reports whether the BPF program
    * will be loaded by default during **bpf_object__load()**.
    * @param prog BPF program
    * @return true if the program is set to be auto-loaded
    */
    [DllImport("libbpf")]
    public static extern bool bpf_program__autoload(BpfProgramHandle prog);

    /**
    * @brief **bpf_program__set_autoload()** sets whether the BPF program
    * should be loaded by default during **bpf_object__load()**. Has to be
    * called before the object is loaded.
    * @param prog BPF program
    * @param autoload whether the program should be auto-loaded
    * @return 0, on success; negative error code, otherwise
    */
    [DllImport("libbpf")]
    public static extern int bpf_program__set_autoload(BpfProgramHandle prog, [MarshalAs(UnmanagedType.I1)] bool autoload);

    /**
    * @brief **bpf_program__attach()** is a generic function for attaching
    * a BPF program based on auto-detection of program type, attach type,
    * and extra parameters, where applicable. Supported for kprobe/kretprobe,
    * uprobe/uretprobe, tracepoint, raw tracepoint, and typed raw
    * TP/fentry/fexit/fmod_ret tracing programs.
    * @param prog BPF program to attach
    * @return reference to the newly created BPF link; or an invalid handle
    * is returned on error, error code is stored in errno
    */
    [DllImport("libbpf", SetLastError = true)]
    public static extern BpfLinkHandle bpf_program__attach(BpfProgramHandle prog);

    #endregion

    #region LINK

    /**
    * @brief **bpf_link__destroy()** detaches and destroys a BPF link,
    * releasing all associated resources.
    * @param link Pointer to a valid BPF link
    * @return 0, on success; negative error code, otherwise
    */
    [DllImport("libbpf")]
    public static extern int bpf_link__destroy(IntPtr link);

    #endregion

    #region MAP

    /**
    * @brief **bpf_object__next_map()** iterates over the maps
    * contained within the BPF object.
    * @param obj Pointer to a valid BPF object
    * @param prev the previous map returned by this function, or
    * `IntPtr.Zero` to fetch the first map
    * @return the next BPF map contained within the BPF object; or an
    * invalid handle if `prev` was the last map
    */
    [DllImport("libbpf")]
    public static extern BpfMapHandle bpf_object__next_map(BpfObjectHandle obj, IntPtr prev);

    //TO DO => declare other functions (lookup/update/delete elem, pin, fd).

    #endregion

    #region LOG

    /**
    * @brief **libbpf_set_print()** sets user-provided log callback function
    * to be used for libbpf warnings and informational messages. If the user
    * callback is not set, messages are logged to stderr by default. The
    * verbosity of these messages can be controlled by setting the
    * environment variable LIBBPF_LOG_LEVEL to either warn, info, or debug.
    * @param fn the log print function; pass null to silence libbpf output
    * @return the previously registered print function, or null if none was
    * set
    *
    * This function is thread-safe.
    */
    [DllImport("libbpf")]
    public static extern LibbpfPrintFn? libbpf_set_print(LibbpfPrintFn? fn);

    #endregion

    #region ERROR

    /**
    * @brief **libbpf_strerror()** writes the human-readable string
    * corresponding to the given error code into the provided buffer.
    * @param err the error code to describe (typically a negative errno
    * value or the value read from `Marshal.GetLastPInvokeError()`)
    * @param buf buffer to receive the NUL-terminated UTF-8 message
    * @param bufSz size, in bytes, of buf
    * @return 0, on success; negative error code, otherwise
    */
    [DllImport("libbpf")]
    public static extern int libbpf_strerror(int err, byte[] buf, nuint bufSz);

    #endregion
}