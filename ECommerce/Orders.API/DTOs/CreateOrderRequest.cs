using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs
{
    /// <summary>
    /// Datos para crear una orden de compra.
    /// </summary>
    public class CreateOrderRequest
    {
        /// <summary>
        /// Identificador del usuario comprador.
        /// </summary>
        public Guid UsuarioId { get; set; }

        /// <summary>
        /// Lista de productos que se desea comprar.
        /// </summary>
        [Required(
            ErrorMessage = "Los productos son obligatorios.")]
        [MinLength(
            1,
            ErrorMessage = "La orden debe contener al menos un producto.")]
        public List<OrderItemRequest> Items { get; set; } =
            new List<OrderItemRequest>();
    }

    /// <summary>
    /// Datos de un producto incluido en la orden.
    /// </summary>
    public class OrderItemRequest
    {
        /// <summary>
        /// Identificador del producto.
        /// </summary>
        public Guid ProductoId { get; set; }

        /// <summary>
        /// Cantidad de unidades que se desea comprar.
        /// </summary>
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }
    }
}