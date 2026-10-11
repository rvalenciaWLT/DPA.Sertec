using System;
using System.Collections.Generic;

namespace DPA.Sertec.Data.Infrastructure.Entities;

public partial class OrdenReparacion
{
    public int Id { get; set; }

    public DateTime FechaIngreso { get; set; }

    public string DescripcionProblema { get; set; } = null!;

    public decimal CostoEstimado { get; set; }

    public string Estado { get; set; } = null!;

    public int EquipoId { get; set; }

    public int TipoReparacionId { get; set; }

    public virtual Equipo Equipo { get; set; } = null!;

    public virtual TipoReparacion TipoReparacion { get; set; } = null!;
}
