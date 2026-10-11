using DPA.Sertec.Data.Core.Common;
using DPA.Sertec.Data.Core.DTOs;
using DPA.Sertec.Data.Core.Interfaces;
using DPA.Sertec.Data.Infrastructure.Entities;

namespace DPA.Sertec.Data.Infrastructure.Services;

/// <summary>
/// Capa intermedia entre el controlador y el repositorio.
/// Concentra las reglas de negocio y el mapeo entidad -> DTO, de modo que:
///   - el controlador queda delgado (solo traduce resultados a HTTP),
///   - el repositorio queda limpio (solo persistencia),
///   - las reglas se pueden probar sin levantar la Web API.
/// Depende de IOrdenReparacionRepository (abstraccion), no de la clase concreta.
/// </summary>
public class OrdenReparacionService : IOrdenReparacionService
{
    /// <summary>Debe mantenerse alineado con CK_OrdenReparacion_Estado en la base de datos.</summary>
    private static readonly string[] EstadosValidos =
        ["Pendiente", "EnProceso", "Reparado", "Entregado", "Cancelado"];

    private readonly IOrdenReparacionRepository _repository;

    public OrdenReparacionService(IOrdenReparacionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<OrdenReparacionDto>> GetAllAsync()
    {
        var ordenes = await _repository.GetAllAsync();
        return ordenes.Select(MapToDto).ToList();
    }

    public async Task<ServiceResult<OrdenReparacionDto>> GetByIdAsync(int id)
    {
        var orden = await _repository.GetByIdAsync(id);

        return orden is null
            ? ServiceResult<OrdenReparacionDto>.NotFound($"No existe la orden de reparacion con Id {id}.")
            : ServiceResult<OrdenReparacionDto>.Ok(MapToDto(orden));
    }

    public async Task<ServiceResult<OrdenReparacionDto>> CreateAsync(OrdenReparacionCreateDto dto)
    {
        var validacion = await ValidarReferenciasAsync(dto.Estado, dto.EquipoId, dto.TipoReparacionId);
        if (validacion is not null)
            return ServiceResult<OrdenReparacionDto>.Invalid(validacion);

        var orden = new OrdenReparacion
        {
            // Si el cliente no manda fecha, se registra el momento del ingreso.
            FechaIngreso = TruncarASegundos(dto.FechaIngreso ?? DateTime.Now),
            DescripcionProblema = dto.DescripcionProblema.Trim(),
            CostoEstimado = dto.CostoEstimado,
            Estado = dto.Estado.Trim(),
            EquipoId = dto.EquipoId,
            TipoReparacionId = dto.TipoReparacionId
        };

        await _repository.AddAsync(orden);

        // Se relee para traer Equipo, Cliente y TipoReparacion ya materializados.
        var creada = await _repository.GetByIdAsync(orden.Id);

        return ServiceResult<OrdenReparacionDto>.Ok(MapToDto(creada!));
    }

    public async Task<ServiceResult<OrdenReparacionDto>> UpdateAsync(int id, OrdenReparacionUpdateDto dto)
    {
        if (id != dto.Id)
            return ServiceResult<OrdenReparacionDto>.Invalid("El Id de la ruta no coincide con el Id del cuerpo de la peticion.");

        var orden = await _repository.GetByIdAsync(id);
        if (orden is null)
            return ServiceResult<OrdenReparacionDto>.NotFound($"No existe la orden de reparacion con Id {id}.");

        var validacion = await ValidarReferenciasAsync(dto.Estado, dto.EquipoId, dto.TipoReparacionId);
        if (validacion is not null)
            return ServiceResult<OrdenReparacionDto>.Invalid(validacion);

        orden.FechaIngreso = TruncarASegundos(dto.FechaIngreso);
        orden.DescripcionProblema = dto.DescripcionProblema.Trim();
        orden.CostoEstimado = dto.CostoEstimado;
        orden.Estado = dto.Estado.Trim();
        orden.EquipoId = dto.EquipoId;
        orden.TipoReparacionId = dto.TipoReparacionId;

        await _repository.UpdateAsync(orden);

        var actualizada = await _repository.GetByIdAsync(id);

        return ServiceResult<OrdenReparacionDto>.Ok(MapToDto(actualizada!));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var orden = await _repository.GetByIdAsync(id);
        if (orden is null)
            return ServiceResult<bool>.NotFound($"No existe la orden de reparacion con Id {id}.");

        // REGLA DE NEGOCIO: una orden ya entregada al cliente es un registro historico
        // y no puede borrarse. Para anularla se debe pasar su estado a 'Cancelado'.
        if (string.Equals(orden.Estado, "Entregado", StringComparison.OrdinalIgnoreCase))
            return ServiceResult<bool>.Conflict($"La orden {id} ya fue entregada y no puede eliminarse. Cambie su estado a 'Cancelado' si desea anularla.");

        await _repository.DeleteAsync(orden);

        return ServiceResult<bool>.Ok(true);
    }

    /// <summary>
    /// Valida el estado contra el catalogo y comprueba que las claves foraneas existan,
    /// para responder 400 con un mensaje claro en vez de dejar que SQL Server lance un 500.
    /// Devuelve null si todo es valido.
    /// </summary>
    private async Task<string?> ValidarReferenciasAsync(string estado, int equipoId, int tipoReparacionId)
    {
        if (!EstadosValidos.Contains(estado?.Trim(), StringComparer.OrdinalIgnoreCase))
            return $"El estado '{estado}' no es valido. Valores permitidos: {string.Join(", ", EstadosValidos)}.";

        if (!await _repository.EquipoExistsAsync(equipoId))
            return $"No existe el equipo con Id {equipoId}.";

        if (!await _repository.TipoReparacionExistsAsync(tipoReparacionId))
            return $"No existe el tipo de reparacion con Id {tipoReparacionId}.";

        return null;
    }

    /// <summary>
    /// La columna FechaIngreso es DATETIME2(0): SQL Server redondea a segundos.
    /// Si no truncamos aqui, la respuesta del POST traeria milisegundos que el
    /// GET posterior ya no devolveria, y los datos se verian inconsistentes.
    /// </summary>
    private static DateTime TruncarASegundos(DateTime fecha) =>
        new(fecha.Year, fecha.Month, fecha.Day, fecha.Hour, fecha.Minute, fecha.Second, fecha.Kind);

    private static OrdenReparacionDto MapToDto(OrdenReparacion o) => new()
    {
        Id = o.Id,
        FechaIngreso = o.FechaIngreso,
        DescripcionProblema = o.DescripcionProblema,
        CostoEstimado = o.CostoEstimado,
        Estado = o.Estado,
        EquipoId = o.EquipoId,
        EquipoDescripcion = $"{o.Equipo.Marca} {o.Equipo.Modelo} ({o.Equipo.NumeroSerie})",
        ClienteId = o.Equipo.ClienteId,
        ClienteNombreCompleto = $"{o.Equipo.Cliente.Paterno} {o.Equipo.Cliente.Materno} {o.Equipo.Cliente.Nombres}".Replace("  ", " ").Trim(),
        TipoReparacionId = o.TipoReparacionId,
        TipoReparacionNombre = o.TipoReparacion.Nombre,
        TipoReparacionPrecioBase = o.TipoReparacion.PrecioBase
    };
}
