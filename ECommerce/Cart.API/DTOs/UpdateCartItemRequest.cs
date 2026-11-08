using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs
{
    /// <summary>
    /// Datos para actualizar la cantidad de un producto del carrito.
    /// </summary>
    public class UpdateCartItemRequest
    {
        /// <summary>
        /// Nueva cantidad de unidades, mayor a cero.
        /// </summary>
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }
    }
}