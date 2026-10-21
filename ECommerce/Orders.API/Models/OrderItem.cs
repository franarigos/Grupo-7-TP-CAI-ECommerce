using System;

namespace Orders.API.Models
{
    /// <summary>
    /// Representa un producto incluido dentro de una orden.
    /// </summary>
    public class OrderItem
    {
        /// <summary>Referencia al producto.</summary>
        public Guid ProductoId { get; set; }

        /// <summary>Requerido, mayor a 0.</summary>
        public int Cantidad { get; set; }

        /// <summary>Capturado del producto al momento de crear la orden.</summary>
        public decimal PrecioUnitario { get; set; }
    }
}
