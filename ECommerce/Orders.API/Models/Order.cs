using System;
using System.Collections.Generic;

namespace Orders.API.Models
{
    /// <summary>
    /// Representa una orden de compra generada por un usuario.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Identificador único de la orden.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador del usuario que realizó la orden.
        /// </summary>
        public Guid UsuarioId { get; set; }

        /// <summary>
        /// Lista de productos que componen la orden de compra.
        /// </summary>
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Monto total calculado para la orden.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Estado actual de la orden: Pendiente, Confirmada, Enviada, Entregada o Cancelada.
        /// </summary>
        public string Estado { get; set; } = "Pendiente";

        /// <summary>
        /// Fecha y hora en la que se generó la orden.
        /// </summary>
        public DateTime FechaCreacion { get; set; }
    }
}
