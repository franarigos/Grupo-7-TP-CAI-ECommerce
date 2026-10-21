using System;
using System.Collections.Generic;

namespace Orders.API.DTOs
{
    /// <summary>
    /// Respuesta que devuelve los datos detallados de una orden.
    /// </summary>
    public class OrderResponse
    {
        /// <summary>
        /// Identificador único de la orden.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador del usuario dueño de la orden.
        /// </summary>
        public Guid UsuarioId { get; set; }

        /// <summary>
        /// Detalles de los productos incluidos en la orden.
        /// </summary>
        public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();

        /// <summary>
        /// Costo total de la orden.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Estado actual de la orden.
        /// </summary>
        public string Estado { get; set; }

        /// <summary>
        /// Fecha en la que se registró la orden.
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de la última actualización del estado (si aplica).
        /// </summary>
        public DateTime? FechaActualizacion { get; set; }
    }

    /// <summary>
    /// Respuesta que devuelve el detalle de un producto dentro de una orden.
    /// </summary>
    public class OrderItemResponse
    {
        /// <summary>
        /// Identificador del producto.
        /// </summary>
        public Guid ProductoId { get; set; }

        /// <summary>
        /// Cantidad comprada.
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Precio unitario al momento de la compra.
        /// </summary>
        public decimal PrecioUnitario { get; set; }
    }
}
