using System;
using System.Collections.Generic;

namespace Products.API.DTOs
{
    public class OrdersApiResponse
    {
        public string Estado { get; set; } = string.Empty;

        public List<OrderItemApiResponse> Items { get; set; } =
            new List<OrderItemApiResponse>();
    }

    public class OrderItemApiResponse
    {
        public Guid ProductoId { get; set; }
    }
}
