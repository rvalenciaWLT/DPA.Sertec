using DPA.Sertec.Data.Infrastructure.Data;
using DPA.Sertec.Data.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DPA.Sertec.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TipoReparacionController : ControllerBase
{
    private readonly SertecDbContext _context;

    public TipoReparacionController(SertecDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TipoReparacion>>> GetTipoReparaciones()
    {
        return await _context.TipoReparacions
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TipoReparacion>> GetTipoReparacion(int id)
    {
        var tipoReparacion = await _context.TipoReparacions.FindAsync(id);

        if (tipoReparacion is null)
            return NotFound($"No existe el TipoReparacion con Id {id}.");

        return tipoReparacion;
    }

    [HttpPost]
    public async Task<ActionResult<TipoReparacion>> PostTipoReparacion(TipoReparacion tipoReparacion)
    {
        tipoReparacion.Id = 0;

        _context.TipoReparacions.Add(tipoReparacion);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict($"Ya existe un tipo de reparacion llamado '{tipoReparacion.Nombre}' o el precio base es invalido.");
        }

        return CreatedAtAction(nameof(GetTipoReparacion), new { id = tipoReparacion.Id }, tipoReparacion);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutTipoReparacion(int id, TipoReparacion tipoReparacion)
    {
        if (id != tipoReparacion.Id)
            return BadRequest("El Id de la ruta no coincide con el Id del cuerpo de la peticion.");

        if (!await TipoReparacionExistsAsync(id))
            return NotFound($"No existe el TipoReparacion con Id {id}.");

        _context.Entry(tipoReparacion).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict($"Ya existe un tipo de reparacion llamado '{tipoReparacion.Nombre}' o el precio base es invalido.");
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTipoReparacion(int id)
    {
        var tipoReparacion = await _context.TipoReparacions.FindAsync(id);

        if (tipoReparacion is null)
            return NotFound($"No existe el TipoReparacion con Id {id}.");

        _context.TipoReparacions.Remove(tipoReparacion);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict($"No se puede eliminar el TipoReparacion {id} porque tiene ordenes de reparacion asociadas.");
        }

        return NoContent();
    }

    private Task<bool> TipoReparacionExistsAsync(int id) =>
        _context.TipoReparacions.AnyAsync(e => e.Id == id);
}
