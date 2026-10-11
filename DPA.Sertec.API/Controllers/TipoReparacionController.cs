using DPA.Sertec.Data.Infrastructure.Data;
using DPA.Sertec.Data.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DPA.Sertec.API.Controllers;

/// <summary>
/// Pregunta 4: controlador "autogenerado".
/// Usa inyeccion directa del DbContext, sin patron Repository:
/// el acceso a datos ocurre dentro del propio controlador.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TipoReparacionController : ControllerBase
{
    private readonly SertecDbContext _context;

    public TipoReparacionController(SertecDbContext context)
    {
        _context = context;
    }

    // GET: api/TipoReparacion
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TipoReparacion>>> GetTipoReparaciones()
    {
        // AsNoTracking: es una lectura, no necesitamos el change tracker.
        return await _context.TipoReparacions
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .ToListAsync();
    }

    // GET: api/TipoReparacion/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TipoReparacion>> GetTipoReparacion(int id)
    {
        var tipoReparacion = await _context.TipoReparacions.FindAsync(id);

        if (tipoReparacion is null)
            return NotFound($"No existe el TipoReparacion con Id {id}.");

        return tipoReparacion;
    }

    // POST: api/TipoReparacion
    [HttpPost]
    public async Task<ActionResult<TipoReparacion>> PostTipoReparacion(TipoReparacion tipoReparacion)
    {
        // El Id es IDENTITY en SQL Server: lo asigna la base de datos.
        tipoReparacion.Id = 0;

        _context.TipoReparacions.Add(tipoReparacion);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Choca contra UQ_TipoReparacion_Nombre o CK_TipoReparacion_PrecioBase.
            return Conflict($"Ya existe un tipo de reparacion llamado '{tipoReparacion.Nombre}' o el precio base es invalido.");
        }

        // 201 Created + cabecera Location apuntando al GET por Id.
        return CreatedAtAction(nameof(GetTipoReparacion), new { id = tipoReparacion.Id }, tipoReparacion);
    }

    // PUT: api/TipoReparacion/5
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

    // DELETE: api/TipoReparacion/5
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
            // FK_OrdenReparacion_TipoReparacion: hay ordenes que lo referencian.
            return Conflict($"No se puede eliminar el TipoReparacion {id} porque tiene ordenes de reparacion asociadas.");
        }

        return NoContent();
    }

    private Task<bool> TipoReparacionExistsAsync(int id) =>
        _context.TipoReparacions.AnyAsync(e => e.Id == id);
}
