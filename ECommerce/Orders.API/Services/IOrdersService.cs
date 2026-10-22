using System;
using System.Collections.Generic;
using Orders.API.DTOs;

namespace Orders.API.Services
{
    public interface IOrdersService
    {
        OrderResponse CrearOrden(CreateOrderRequest request);
        OrderResponse ObtenerOrdenPorId(Guid id);
        List<OrderResponse> ObtenerOrdenes(Guid? usuarioId);
        OrderResponse ActualizarEstado(Guid id, UpdateOrderStatusRequest request);
    }
}
