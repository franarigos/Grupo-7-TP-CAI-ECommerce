using System;

namespace Cart.API.DTOs
{
    /// <summary>
    /// Datos para agregar un ítem al carrito.
    /// </summary>
    public class AddCartItemRequest
    {
        /// <summary>
        /// Identificador único del producto a agregar.
        /// </summary>
        public Guid ProductoId { get; set; }

        /// <summary>
        /// Cantidad del producto a agregar.
        /// </summary>
        public int Cantidad { get; set; }
    }
}