using System;
using System.Collections.Generic;

namespace Cart.API.DTOs
{
    /// <summary>
    /// Representa la respuesta con el estado del carrito.
    /// </summary>
    public class CartResponse
    {
        /// <summary>
        /// Identificador del usuario dueño del carrito.
        /// </summary>
        public Guid UsuarioId { get; set; }

        /// <summary>
        /// Lista de productos en el carrito.
        /// </summary>
        public List<CartItemResponse> Items { get; set; } = new List<CartItemResponse>();

        /// <summary>
        /// Fecha y hora de la última modificación.
        /// </summary>
        public DateTime FechaActualizacion { get; set; }
    }

    /// <summary>
    /// Representa un producto individual dentro del carrito.
    /// </summary>
    public class CartItemResponse
    {
        /// <summary>
        /// Identificador único del producto.
        /// </summary>
        public Guid ProductoId { get; set; }

        /// <summary>
        /// Cantidad del producto en el carrito.
        /// </summary>
        public int Cantidad { get; set; }
    }
}