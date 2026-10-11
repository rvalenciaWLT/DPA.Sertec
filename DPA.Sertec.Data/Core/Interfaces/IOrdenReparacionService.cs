using DPA.Sertec.Data.Core.Common;
using DPA.Sertec.Data.Core.DTOs;

namespace DPA.Sertec.Data.Core.Interfaces;

/// <summary>
/// Contrato de LOGICA DE NEGOCIO para OrdenReparacion.
/// Trabaja unicamente con DTOs: las entidades de EF nunca salen de Infrastructure.
/// Es la unica dependencia que conoce el controlador.
/// </summary>
public interface IOrdenReparacionService
{
    Task<IEnumerable<OrdenReparacionDto>> GetAllAsync();

    Task<ServiceResult<OrdenReparacionDto>> GetByIdAsync(int id);

    Task<ServiceResult<OrdenReparacionDto>> CreateAsync(OrdenReparacionCreateDto dto);

    Task<ServiceResult<OrdenReparacionDto>> UpdateAsync(int id, OrdenReparacionUpdateDto dto);

    Task<ServiceResult<bool>> DeleteAsync(int id);
}
