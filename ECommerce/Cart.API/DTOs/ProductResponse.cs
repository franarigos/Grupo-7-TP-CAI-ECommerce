using System;

namespace Cart.API.DTOs
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}