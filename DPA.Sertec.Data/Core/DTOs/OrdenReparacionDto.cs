namespace DPA.Sertec.Data.Core.DTOs;

/// <summary>
/// DTO de LECTURA de una orden de reparacion.
/// Aplana las relaciones (Equipo -> Cliente, TipoReparacion) para que el cliente
/// reciba los datos utiles en una sola llamada, sin exponer las entidades de EF
/// ni arrastrar referencias ciclicas.
/// </summary>
public class OrdenReparacionDto
{
    public int Id { get; set; }

    public DateTime FechaIngreso { get; set; }

    public string DescripcionProblema { get; set; } = null!;

    public decimal CostoEstimado { get; set; }

    public string Estado { get; set; } = null!;

    public int EquipoId { get; set; }

    /// <summary>Ej: "Lenovo ThinkPad T14 (SN-LP-00001)".</summary>
    public string EquipoDescripcion { get; set; } = null!;

    public int ClienteId { get; set; }

    /// <summary>Ej: "Valencia Rojas, Ricardo Antonio".</summary>
    public string ClienteNombreCompleto { get; set; } = null!;

    public int TipoReparacionId { get; set; }

    public string TipoReparacionNombre { get; set; } = null!;

    public decimal TipoReparacionPrecioBase { get; set; }
}
