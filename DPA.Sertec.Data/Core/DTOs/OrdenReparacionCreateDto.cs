using System.ComponentModel.DataAnnotations;

namespace DPA.Sertec.Data.Core.DTOs;

/// <summary>
/// DTO de ESCRITURA para crear una orden.
/// No expone Id (lo genera SQL Server con IDENTITY) ni propiedades de navegacion,
/// asi el cliente no puede inyectar datos que no le corresponden (over-posting).
/// </summary>
public class OrdenReparacionCreateDto
{
    /// <summary>Opcional: si no se envia, se usa la fecha y hora actual.</summary>
    public DateTime? FechaIngreso { get; set; }

    [Required(ErrorMessage = "La descripcion del problema es obligatoria.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "La descripcion debe tener entre 5 y 500 caracteres.")]
    public string DescripcionProblema { get; set; } = null!;

    [Range(0, 999999.99, ErrorMessage = "El costo estimado no puede ser negativo.")]
    public decimal CostoEstimado { get; set; }

    /// <summary>Pendiente | EnProceso | Reparado | Entregado | Cancelado.</summary>
    [Required(ErrorMessage = "El estado es obligatorio.")]
    [StringLength(20)]
    public string Estado { get; set; } = "Pendiente";

    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un EquipoId valido.")]
    public int EquipoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un TipoReparacionId valido.")]
    public int TipoReparacionId { get; set; }
}
