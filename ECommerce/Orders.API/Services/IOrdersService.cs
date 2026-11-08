
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

        Task<OrderResponse> ObtenerOrdenPorIdAsync(Guid id);

        Task<List<OrderResponse>> ObtenerOrdenesAsync(
            Guid? usuarioId);

        Task<OrderResponse> ActualizarEstadoAsync(
            Guid id,
            UpdateOrderStatusRequest request);
    }
}
