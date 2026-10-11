using DPA.Sertec.Data.Infrastructure.Entities;

namespace DPA.Sertec.Data.Core.Interfaces;

public interface IOrdenReparacionRepository
{
    Task<IEnumerable<OrdenReparacion>> GetAllAsync();

    Task<OrdenReparacion?> GetByIdAsync(int id);

    Task AddAsync(OrdenReparacion orden);

    Task UpdateAsync(OrdenReparacion orden);

    Task DeleteAsync(OrdenReparacion orden);

    Task<bool> ExistsAsync(int id);

    Task<bool> EquipoExistsAsync(int equipoId);

    Task<bool> TipoReparacionExistsAsync(int tipoReparacionId);
}
