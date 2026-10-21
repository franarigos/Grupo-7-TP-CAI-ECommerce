using System;
using System.Collections.Generic;

namespace Orders.API.DTOs
{
    /// <summary>
    /// Solicitud de creacion de orden.
    /// </summary>
    public class CreateOrderRequest
    {
        /// <summary>ID del comprador.</summary>
        public Guid UsuarioId { get; set; }

        /// <summary>Productos a comprar.</summary>
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
    }

    /// <summary>
    /// Item de la orden a crear.
    /// </summary>
    public class OrderItemRequest
    {
        /// <summary>ID del producto.</summary>
        public Guid ProductoId { get; set; }

        /// <summary>Cantidad de unidades.</summary>
        public int Cantidad { get; set; }
    }
}
