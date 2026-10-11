using System.ComponentModel.DataAnnotations;

namespace DPA.Sertec.Data.Core.DTOs;

public class OrdenReparacionUpdateDto
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }

    [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
    public DateTime FechaIngreso { get; set; }

    [Required(ErrorMessage = "La descripcion del problema es obligatoria.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "La descripcion debe tener entre 5 y 500 caracteres.")]
    public string DescripcionProblema { get; set; } = null!;

    [Range(0, 999999.99, ErrorMessage = "El costo estimado no puede ser negativo.")]
    public decimal CostoEstimado { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un EquipoId valido.")]
    public int EquipoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un TipoReparacionId valido.")]
    public int TipoReparacionId { get; set; }
}
