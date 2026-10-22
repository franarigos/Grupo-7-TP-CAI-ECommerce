using System;
using System.Collections.Generic;
using Orders.API.DTOs;
using Orders.API.Models;
using Orders.API.Exceptions;

namespace Orders.API.Services
{
    public class OrdersService : IOrdersService
    {
        public OrderResponse CrearOrden(CreateOrderRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                throw new ValidationException(ErrorCodes.ORD_002, "La orden debe tener al menos un item.");
            }

            Order nuevaOrden = new Order
            {
                Id = Guid.NewGuid(),
                UsuarioId = request.UsuarioId,
                FechaCreacion = DateTime.UtcNow,
                Estado = "Pendiente",
                Items = new List<OrderItem>(),
                Total = 0
            };

            foreach (OrderItemRequest itemReq in request.Items)
            {
                if (itemReq.Cantidad <= 0)
                {
                    throw new ValidationException(ErrorCodes.ORD_002, "La cantidad debe ser mayor a cero.");
                }

                decimal precioFicticio = 1000m;

                OrderItem nuevoItem = new OrderItem
                {
                    ProductoId = itemReq.ProductoId,
                    Cantidad = itemReq.Cantidad,
                    PrecioUnitario = precioFicticio
                };

                nuevaOrden.Items.Add(nuevoItem);

                nuevaOrden.Total += (nuevoItem.Cantidad * nuevoItem.PrecioUnitario);
            }

            return MapearA_Response(nuevaOrden);
        }

        public OrderResponse ObtenerOrdenPorId(Guid id)
        {
            Order orden = null;

            if (orden == null)
            {
                throw new NotFoundException(ErrorCodes.ORD_001, "Orden no encontrada.");
            }

            return MapearA_Response(orden);
        }

        public List<OrderResponse> ObtenerOrdenes(Guid? usuarioId)
        {
            List<Order> todasLasOrdenes = new List<Order>();
            List<OrderResponse> resultado = new List<OrderResponse>();

            foreach (Order orden in todasLasOrdenes)
            {
                if (usuarioId == null || orden.UsuarioId == usuarioId)
                {
                    resultado.Add(MapearA_Response(orden));
                }
            }

            return resultado;
        }

        public OrderResponse ActualizarEstado(Guid id, UpdateOrderStatusRequest request)
        {
            Order orden = null;

            if (orden == null)
            {
                throw new NotFoundException(ErrorCodes.ORD_001, "Orden no encontrada.");
            }

            if (orden.Estado == "Cancelada" || orden.Estado == "Entregada")
            {
                throw new BusinessRuleException(ErrorCodes.ORD_006, $"Una orden en estado '{orden.Estado}' no puede ser modificada.");
            }

            orden.Estado = request.Estado;

            return MapearA_Response(orden);
        }

        private OrderResponse MapearA_Response(Order orden)
        {
            OrderResponse response = new OrderResponse
            {
                Id = orden.Id,
                UsuarioId = orden.UsuarioId,
                Total = orden.Total,
                Estado = orden.Estado,
                FechaCreacion = orden.FechaCreacion,
                Items = new List<OrderItemResponse>()
            };

            foreach (OrderItem item in orden.Items)
            {
                response.Items.Add(new OrderItemResponse
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario
                });
            }

            return response;
        }
    }
}
