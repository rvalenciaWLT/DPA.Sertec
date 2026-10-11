using DPA.Sertec.Data.Core.Interfaces;
using DPA.Sertec.Data.Infrastructure.Data;
using DPA.Sertec.Data.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace DPA.Sertec.Data.Infrastructure.Repositories;

/// <summary>
/// Implementacion del repositorio sobre Entity Framework Core.
/// Es el UNICO lugar del proyecto que conoce el SertecDbContext para esta entidad:
/// si manana se cambia de EF a Dapper, solo se reescribe esta clase.
/// </summary>
public class OrdenReparacionRepository : IOrdenReparacionRepository
{
    private readonly SertecDbContext _context;

    public OrdenReparacionRepository(SertecDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Consulta base con los Include necesarios para armar el DTO en una sola ida a la BD
    /// (evita el problema N+1 de consultar el equipo y el cliente orden por orden).
    /// </summary>
    private IQueryable<OrdenReparacion> BaseQuery() =>
        _context.OrdenReparacions
            .Include(o => o.Equipo)
                .ThenInclude(e => e.Cliente)
            .Include(o => o.TipoReparacion);

    public async Task<IEnumerable<OrdenReparacion>> GetAllAsync() =>
        await BaseQuery()
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .ToListAsync();

    public async Task<OrdenReparacion?> GetByIdAsync(int id) =>
        await BaseQuery().FirstOrDefaultAsync(o => o.Id == id);

    public async Task AddAsync(OrdenReparacion orden)
    {
        await _context.OrdenReparacions.AddAsync(orden);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(OrdenReparacion orden)
    {
        // La entidad viene del change tracker (GetByIdAsync la devuelve rastreada),
        // por eso basta con guardar los cambios ya aplicados sobre ella.
        _context.OrdenReparacions.Update(orden);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(OrdenReparacion orden)
    {
        _context.OrdenReparacions.Remove(orden);
        await _context.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(int id) =>
        _context.OrdenReparacions.AnyAsync(o => o.Id == id);

    public Task<bool> EquipoExistsAsync(int equipoId) =>
        _context.Equipos.AnyAsync(e => e.Id == equipoId);

    public Task<bool> TipoReparacionExistsAsync(int tipoReparacionId) =>
        _context.TipoReparacions.AnyAsync(t => t.Id == tipoReparacionId);
}
