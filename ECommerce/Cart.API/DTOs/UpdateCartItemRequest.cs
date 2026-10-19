using System;

namespace Cart.API.DTOs
{
    /// <summary>
    /// Datos requeridos para actualizar la cantidad de un ítem en el carrito.
    /// </summary>
    public class UpdateCartItemRequest
    {
        /// <summary>
        /// Nueva cantidad del producto (debe ser mayor a 0).
        /// </summary>
        public int Cantidad { get; set; }
    }
}