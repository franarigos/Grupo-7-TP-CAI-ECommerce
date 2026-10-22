using System;
using System.Collections.Generic;

namespace Orders.API.DTOs
{
    /// <summary>
    /// Respuesta detallada de la orden.
    /// </summary>
    public class OrderResponse
    {
        /// <summary>ID de la orden.</summary>
        public Guid Id { get; set; }

        /// <summary>ID del usuario.</summary>
        public Guid UsuarioId { get; set; }

        /// <summary>Detalles de los productos.</summary>
        public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();

        /// <summary>Monto total.</summary>
        public decimal Total { get; set; }

        /// <summary>Estado de la orden.</summary>
        public string Estado { get; set; }

        /// <summary>Fecha de creacion.</summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>Fecha de ultima modificacion.</summary>
        public DateTime? FechaActualizacion { get; set; }
    }

    /// <summary>
    /// Detalle del producto comprado.
    /// </summary>
    public class OrderItemResponse
    {
        /// <summary>ID del producto.</summary>
        public Guid ProductoId { get; set; }

        /// <summary>Cantidad comprada.</summary>
        public int Cantidad { get; set; }

        /// <summary>Precio al momento de compra.</summary>
        public decimal PrecioUnitario { get; set; }
    }
}
