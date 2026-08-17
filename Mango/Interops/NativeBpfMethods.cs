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
    public static extern int bpf_object__prepare(IntPtr obj);

    /**
    * @brief **bpf_object__load()** loads BPF object into kernel.
    * @param obj Pointer to a valid BPF object instance returned by
    * **bpf_object__open*()** APIs
    * @return 0, on success; negative error code, otherwise, error code is
    * stored in errno
    */
    [DllImport("libbpf", SetLastError = true)]
    public static extern int bpf_object__load(IntPtr obj);

    /**
    * @brief **bpf_object__name()** retrieves the name of the BPF object.
    * @param obj Pointer to a valid BPF object
    * @return the object's name
    */
    [DllImport("libbpf")]
    [return: MarshalAs(UnmanagedType.LPUTF8Str)]
    public static extern string bpf_object__name(IntPtr obj);

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
    public static extern BpfProgramHandle bpf_object__find_program_by_name(IntPtr obj, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    #endregion
}