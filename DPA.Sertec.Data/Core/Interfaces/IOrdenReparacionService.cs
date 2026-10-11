using DPA.Sertec.Data.Core.Common;
using DPA.Sertec.Data.Core.DTOs;

namespace DPA.Sertec.Data.Core.Interfaces;

public interface IOrdenReparacionService
{
    Task<IEnumerable<OrdenReparacionDto>> GetAllAsync();

    Task<ServiceResult<OrdenReparacionDto>> GetByIdAsync(int id);

    Task<ServiceResult<OrdenReparacionDto>> CreateAsync(OrdenReparacionCreateDto dto);

    Task<ServiceResult<OrdenReparacionDto>> UpdateAsync(int id, OrdenReparacionUpdateDto dto);

    Task<ServiceResult<bool>> DeleteAsync(int id);
}
