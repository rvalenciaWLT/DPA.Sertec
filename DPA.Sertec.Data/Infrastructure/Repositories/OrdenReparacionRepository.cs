using DPA.Sertec.Data.Core.Interfaces;
using DPA.Sertec.Data.Infrastructure.Data;
using DPA.Sertec.Data.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace DPA.Sertec.Data.Infrastructure.Repositories;

public class OrdenReparacionRepository : IOrdenReparacionRepository
{
    private readonly SertecDbContext _context;

    public OrdenReparacionRepository(SertecDbContext context)
    {
        _context = context;
    }

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
