using System;

namespace Cart.API.DTOs
{
    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public int Stock { get; set; }
    }
}
