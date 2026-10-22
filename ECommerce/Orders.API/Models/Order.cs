using System;
using System.Collections.Generic;

namespace Orders.API.Models
{
    /// <summary>
    /// Representa una orden de compra generada por un usuario.
    /// </summary>
    public class Order
    {
        /// <summary>Identificador único de la orden.</summary>
        public Guid Id { get; set; }

        /// <summary>Referencia al usuario que realizó la orden.</summary>
        public Guid UsuarioId { get; set; }

        /// <summary>Lista de productos incluidos en la orden.</summary>
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        /// <summary>Calculado automáticamente al crear la orden.</summary>
        public decimal Total { get; set; }

        /// <summary>Pendiente | Confirmada | Enviada | Entregada | Cancelada</summary>
        public string Estado { get; set; } = "Pendiente";

        /// <summary>Asignado automáticamente al crear.</summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>Fecha de última modificación.</summary>
        public DateTime? FechaActualizacion { get; set; }
    }
}
