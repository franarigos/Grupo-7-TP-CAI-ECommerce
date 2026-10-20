using System;
using System.Collections.Generic;

namespace Cart.API.Models
{
    /// <summary>
    /// Entidad que representa el carrito de compras de un usuario.
    /// </summary>
    public class Cart
    {
        /// <summary>
        /// Identificador del usuario dueño del carrito.
        /// </summary>
        public Guid UsuarioId { get; set; }

        /// <summary>
        /// Lista de productos en el carrito.
        /// </summary>
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        /// <summary>
        /// Fecha actualizada automáticamente en cada operación.
        /// </summary>
        public DateTime FechaActualizacion { get; set; }
    }
}