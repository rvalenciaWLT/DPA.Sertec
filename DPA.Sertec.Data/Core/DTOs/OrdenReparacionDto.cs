namespace DPA.Sertec.Data.Core.DTOs;

public class OrdenReparacionDto
{
    public int Id { get; set; }

    public DateTime FechaIngreso { get; set; }

    public string DescripcionProblema { get; set; } = null!;

    public decimal CostoEstimado { get; set; }

    public string Estado { get; set; } = null!;

    public int EquipoId { get; set; }

    public string EquipoDescripcion { get; set; } = null!;

    public int ClienteId { get; set; }

    public string ClienteNombreCompleto { get; set; } = null!;

    public int TipoReparacionId { get; set; }

    public string TipoReparacionNombre { get; set; } = null!;

    public decimal TipoReparacionPrecioBase { get; set; }
}
