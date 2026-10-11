using DPA.Sertec.Data.Core.Common;
using DPA.Sertec.Data.Core.DTOs;
using DPA.Sertec.Data.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DPA.Sertec.API.Controllers;

/// <summary>
/// Pregunta 5: controlador con patron Repository.
/// Observese que NO hay ninguna referencia al SertecDbContext ni a Entity Framework:
/// el controlador solo depende de la interfaz IOrdenReparacionService (inyeccion de
/// dependencias) y su unica responsabilidad es traducir el resultado de negocio
/// a codigos de estado HTTP.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class OrdenReparacionController : ControllerBase
{
    private readonly IOrdenReparacionService _service;

    public OrdenReparacionController(IOrdenReparacionService service)
    {
        _service = service;
    }

    // GET: api/OrdenReparacion
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrdenReparacionDto>>> GetOrdenes()
    {
        var ordenes = await _service.GetAllAsync();
        return Ok(ordenes);
    }

    // GET: api/OrdenReparacion/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdenReparacionDto>> GetOrden(int id)
    {
        var resultado = await _service.GetByIdAsync(id);
        return ToActionResult(resultado);
    }

    // POST: api/OrdenReparacion
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrdenReparacionDto>> PostOrden(OrdenReparacionCreateDto dto)
    {
        var resultado = await _service.CreateAsync(dto);

        if (!resultado.IsOk)
            return ToActionResult(resultado);

        return CreatedAtAction(nameof(GetOrden), new { id = resultado.Value!.Id }, resultado.Value);
    }

    // PUT: api/OrdenReparacion/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdenReparacionDto>> PutOrden(int id, OrdenReparacionUpdateDto dto)
    {
        var resultado = await _service.UpdateAsync(id, dto);
        return ToActionResult(resultado);
    }

    // DELETE: api/OrdenReparacion/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteOrden(int id)
    {
        var resultado = await _service.DeleteAsync(id);

        if (!resultado.IsOk)
            return ToActionResult(resultado);

        return NoContent();
    }

    /// <summary>
    /// Unico punto donde el proyecto traduce estado de negocio -> codigo HTTP.
    /// Evita repetir el mismo switch en cada accion.
    /// Devuelve ActionResult (no ActionResult&lt;T&gt;) porque asi sirve tanto en las
    /// acciones tipadas como en el DELETE, que responde IActionResult.
    /// </summary>
    private ActionResult ToActionResult<T>(ServiceResult<T> resultado) => resultado.Status switch
    {
        ServiceStatus.Ok => Ok(resultado.Value),
        ServiceStatus.NotFound => NotFound(resultado.Error),
        ServiceStatus.Invalid => BadRequest(resultado.Error),
        ServiceStatus.Conflict => Conflict(resultado.Error),
        _ => StatusCode(StatusCodes.Status500InternalServerError)
    };
}
