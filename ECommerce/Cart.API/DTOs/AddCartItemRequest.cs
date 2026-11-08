using System;
using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs
{
    /// <summary>
    /// Datos para agregar un producto al carrito.
    /// </summary>
    public class AddCartItemRequest
    {
        /// <summary>
        /// Identificador del producto.
        /// </summary>
        public Guid ProductoId { get; set; }

        /// <summary>
        /// Cantidad de unidades que se desea agregar.
        /// </summary>
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }
    }
}