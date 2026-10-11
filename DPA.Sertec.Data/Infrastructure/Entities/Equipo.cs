using System;
using System.Collections.Generic;

namespace DPA.Sertec.Data.Infrastructure.Entities;

public partial class Equipo
{
    public int Id { get; set; }

    public string NumeroSerie { get; set; } = null!;

    public string Marca { get; set; } = null!;

    public string Modelo { get; set; } = null!;

    public int? Anio { get; set; }

    public int ClienteId { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<OrdenReparacion> OrdenReparacions { get; set; } = new List<OrdenReparacion>();
}
