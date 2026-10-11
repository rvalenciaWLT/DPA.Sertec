namespace DPA.Sertec.Data.Core.Common;

public enum ServiceStatus
{
    Ok,
    NotFound,
    Invalid,
    Conflict
}

public class ServiceResult<T>
{
    public ServiceStatus Status { get; private init; }
    public string? Error { get; private init; }
    public T? Value { get; private init; }

    public bool IsOk => Status == ServiceStatus.Ok;

    public static ServiceResult<T> Ok(T value) =>
        new() { Status = ServiceStatus.Ok, Value = value };

    public static ServiceResult<T> NotFound(string error) =>
        new() { Status = ServiceStatus.NotFound, Error = error };

    public static ServiceResult<T> Invalid(string error) =>
        new() { Status = ServiceStatus.Invalid, Error = error };

    public static ServiceResult<T> Conflict(string error) =>
        new() { Status = ServiceStatus.Conflict, Error = error };
}
