namespace DPA.Sertec.Data.Core.Common;

/// <summary>
/// Resultado posible de una operacion de negocio.
/// Deliberadamente NO usa codigos HTTP: la capa de servicio no debe saber
/// que vive detras de una Web API. El controlador es quien traduce.
/// </summary>
public enum ServiceStatus
{
    Ok,
    NotFound,
    Invalid,
    Conflict
}

/// <summary>
/// Envoltorio que transporta el resultado de la capa de servicio hacia el controlador.
/// Evita usar excepciones para controlar el flujo normal de la aplicacion.
/// </summary>
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
