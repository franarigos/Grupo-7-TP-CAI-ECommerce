using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orders.API.DTOs;

namespace Orders.API.Services
{
    public interface IOrdersService
    {
        Task<OrderResponse> CrearOrdenAsync(
            CreateOrderRequest request,
            CancellationToken cancellationToken);

        OrderResponse ObtenerOrdenPorId(Guid id);

        List<OrderResponse> ObtenerOrdenes(Guid? usuarioId);

        OrderResponse ActualizarEstado(
            Guid id,
            UpdateOrderStatusRequest request);
    }
}
