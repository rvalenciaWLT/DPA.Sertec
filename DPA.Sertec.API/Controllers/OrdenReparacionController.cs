using DPA.Sertec.Data.Core.Common;
using DPA.Sertec.Data.Core.DTOs;
using DPA.Sertec.Data.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DPA.Sertec.API.Controllers;

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

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrdenReparacionDto>>> GetOrdenes()
    {
        var ordenes = await _service.GetAllAsync();
        return Ok(ordenes);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdenReparacionDto>> GetOrden(int id)
    {
        var resultado = await _service.GetByIdAsync(id);
        return ToActionResult(resultado);
    }

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

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdenReparacionDto>> PutOrden(int id, OrdenReparacionUpdateDto dto)
    {
        var resultado = await _service.UpdateAsync(id, dto);
        return ToActionResult(resultado);
    }

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

    private ActionResult ToActionResult<T>(ServiceResult<T> resultado) => resultado.Status switch
    {
        ServiceStatus.Ok => Ok(resultado.Value),
        ServiceStatus.NotFound => NotFound(resultado.Error),
        ServiceStatus.Invalid => BadRequest(resultado.Error),
        ServiceStatus.Conflict => Conflict(resultado.Error),
        _ => StatusCode(StatusCodes.Status500InternalServerError)
    };
}
