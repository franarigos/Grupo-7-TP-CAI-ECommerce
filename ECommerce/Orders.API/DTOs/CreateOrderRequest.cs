using System;
using System.Collections.Generic;

namespace Orders.API.DTOs
{
    /// <summary>
    /// Solicitud para crear una nueva orden de compra.
    /// </summary>
    public class CreateOrderRequest
    {
        /// <summary>Identificador del usuario que realiza la compra.</summary>
        public Guid UsuarioId { get; set; }

        /// <summary>Lista de productos solicitados en la orden.</summary>
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
    }

    /// <summary>
    /// Representa un producto dentro de la solicitud de creación de orden.
    /// </summary>
    public class OrderItemRequest
    {
        /// <summary>Identificador del producto a comprar.</summary>
        public Guid ProductoId { get; set; }

        /// <summary>Cantidad deseada del producto.</summary>
        public int Cantidad { get; set; }
    }
}
