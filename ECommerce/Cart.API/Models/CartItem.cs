using System;

namespace Cart.API.Models
{
    /// <summary>
    /// Representa un ítem individual dentro del carrito de compras.
    /// </summary>
    public class CartItem
    {
        /// <summary>
        /// Referencia al producto.
        /// </summary>
        public Guid ProductoId { get; set; }

        /// <summary>
        /// Cantidad solicitada del producto (requerido, mayor a 0).
        /// </summary>
        public int Cantidad { get; set; }
    }
}