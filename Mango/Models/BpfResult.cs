namespace Mango.Models;

/// <summary>
/// Result of a libbpf operation that can fail with a <see cref="BpfError"/>.
/// </summary>
public sealed record BpfResult<T>
{
    public bool IsSuccess { get; private init; }
    public T? Value { get; private init; }
    public BpfError? Error { get; private init; }

    public static BpfResult<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static BpfResult<T> Failure(BpfError error) => new() { IsSuccess = false, Error = error };
}
