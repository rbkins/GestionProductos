using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionProductos2.Models;

public partial class Producto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero")]
    public decimal PrecioBase { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El descuento debe ser mayor que cero")]
    public decimal? PrecioDescuento { get; set; }

    public string? Imagen { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }
}
