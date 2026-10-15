using System;
using System.Collections.Generic;

namespace Cart.API.Models
{
    public class Cart
    {
        public Guid UsuarioId { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public DateTime FechaActualizacion { get; set; }
    }
}
