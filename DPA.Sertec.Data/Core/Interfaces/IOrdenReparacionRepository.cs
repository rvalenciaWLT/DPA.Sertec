using DPA.Sertec.Data.Infrastructure.Entities;

namespace DPA.Sertec.Data.Core.Interfaces;

/// <summary>
/// Contrato de ACCESO A DATOS para OrdenReparacion.
/// Solo persistencia: no contiene reglas de negocio ni validaciones.
/// Su implementacion vive en Infrastructure/Repositories.
/// </summary>
public interface IOrdenReparacionRepository
{
    Task<IEnumerable<OrdenReparacion>> GetAllAsync();

    Task<OrdenReparacion?> GetByIdAsync(int id);

    Task AddAsync(OrdenReparacion orden);

    Task UpdateAsync(OrdenReparacion orden);

    Task DeleteAsync(OrdenReparacion orden);

    Task<bool> ExistsAsync(int id);

    /// <summary>Valida la FK antes de insertar, para devolver 400 en vez de un 500 de SQL.</summary>
    Task<bool> EquipoExistsAsync(int equipoId);

    /// <summary>Valida la FK antes de insertar, para devolver 400 en vez de un 500 de SQL.</summary>
    Task<bool> TipoReparacionExistsAsync(int tipoReparacionId);
}
