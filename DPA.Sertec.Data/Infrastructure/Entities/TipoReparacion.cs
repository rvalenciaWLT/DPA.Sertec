using System;
using System.Collections.Generic;

namespace DPA.Sertec.Data.Infrastructure.Entities;

public partial class TipoReparacion
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal PrecioBase { get; set; }

    public virtual ICollection<OrdenReparacion> OrdenReparacions { get; set; } = new List<OrdenReparacion>();
}
