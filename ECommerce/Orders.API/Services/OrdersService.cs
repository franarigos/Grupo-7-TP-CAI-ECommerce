using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Orders.API.DTOs;
using Orders.API.Models;
using Orders.API.Exceptions;

namespace Orders.API.Services
{
    public class OrdersService : IOrdersService
    {
        private static readonly List<Order> _ordenes = new List<Order>();
        private static readonly object _candado = new object();

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrdersService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<OrderResponse> CrearOrdenAsync(
            CreateOrderRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null ||
                request.UsuarioId == Guid.Empty ||
                request.Items == null ||
                request.Items.Count == 0)
            {
                throw new ValidationException(
                    ErrorCodes.ORD_002,
                    "Los datos de la orden son invalidos.");
            }

            foreach (OrderItemRequest item in request.Items)
            {
                if (item == null ||
                    item.ProductoId == Guid.Empty ||
                    item.Cantidad <= 0)
                {
                    throw new ValidationException(
                        ErrorCodes.ORD_002,
                        "Los productos y las cantidades son invalidos.");
                }
            }

            await ValidarUsuarioAsync(
                request.UsuarioId,
                cancellationToken);

            Order nuevaOrden = new Order
            {
                Id = Guid.NewGuid(),
                UsuarioId = request.UsuarioId,
                FechaCreacion = DateTime.UtcNow,
                Estado = "Pendiente",
                Items = new List<OrderItem>(),
                Total = 0m
            };

            foreach (OrderItemRequest itemReq in request.Items)
            {
                ProductApiResponse producto =
                    await ObtenerProductoAsync(
                        itemReq.ProductoId,
                        cancellationToken);

                int cantidadTotal = itemReq.Cantidad;

                foreach (OrderItem anterior in nuevaOrden.Items)
                {
                    if (anterior.ProductoId == itemReq.ProductoId)
                    {
                        cantidadTotal += anterior.Cantidad;
                    }
                }

                if (cantidadTotal > producto.Stock)
                {
                    throw new BusinessRuleException(
                        ErrorCodes.ORD_005,
                        $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}, solicitado: {cantidadTotal}.");
                }

                OrderItem nuevoItem = new OrderItem
                {
                    ProductoId = itemReq.ProductoId,
                    Cantidad = itemReq.Cantidad,
                    PrecioUnitario = producto.Precio
                };

                nuevaOrden.Items.Add(nuevoItem);

                nuevaOrden.Total +=
                    nuevoItem.Cantidad * nuevoItem.PrecioUnitario;
            }

            lock (_candado)
            {
                _ordenes.Add(nuevaOrden);
                return MapearA_Response(nuevaOrden);
            }
        }

        public OrderResponse ObtenerOrdenPorId(Guid id)
        {
            lock (_candado)
            {
                foreach (Order orden in _ordenes)
                {
                    if (orden.Id == id)
                    {
                        return MapearA_Response(orden);
                    }
                }
            }

            throw new NotFoundException(
                ErrorCodes.ORD_001,
                "Orden no encontrada.");
        }

        public List<OrderResponse> ObtenerOrdenes(Guid? usuarioId)
        {
            List<OrderResponse> resultado =
                new List<OrderResponse>();

            lock (_candado)
            {
                foreach (Order orden in _ordenes)
                {
                    if (usuarioId == null ||
                        orden.UsuarioId == usuarioId)
                    {
                        resultado.Add(MapearA_Response(orden));
                    }
                }
            }

            return resultado;
        }

        public OrderResponse ActualizarEstado(
            Guid id,
            UpdateOrderStatusRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Estado) ||
                !EsEstadoValido(request.Estado))
            {
                throw new ValidationException(
                    ErrorCodes.ORD_002,
                    "El estado ingresado es invalido.");
            }

            lock (_candado)
            {
                foreach (Order orden in _ordenes)
                {
                    if (orden.Id == id)
                    {
                        if (orden.Estado == request.Estado)
                        {
                            return MapearA_Response(orden);
                        }

                        if (!PuedeCambiarEstado(
                            orden.Estado, request.Estado))
                        {
                            throw new BusinessRuleException(
                                ErrorCodes.ORD_006,
                                $"Una orden en estado '{orden.Estado}' no puede pasar a '{request.Estado}'.");
                        }

                        orden.Estado = request.Estado;
                        orden.FechaActualizacion = DateTime.UtcNow;

                        return MapearA_Response(orden);
                    }
                }
            }

            throw new NotFoundException(
                ErrorCodes.ORD_001,
                "Orden no encontrada.");
        }

        private static bool EsEstadoValido(string estado)
        {
            return estado == "Pendiente" ||
                   estado == "Confirmada" ||
                   estado == "Enviada" ||
                   estado == "Entregada" ||
                   estado == "Cancelada";
        }

        private static bool PuedeCambiarEstado(
            string actual,
            string nuevo)
        {
            if (actual == "Pendiente")
            {
                return nuevo == "Confirmada" ||
                       nuevo == "Cancelada";
            }

            if (actual == "Confirmada")
            {
                return nuevo == "Enviada" ||
                       nuevo == "Cancelada";
            }

            if (actual == "Enviada")
            {
                return nuevo == "Entregada";
            }

            return false;
        }

        private async Task ValidarUsuarioAsync(
            Guid usuarioId,
            CancellationToken cancellationToken)
        {
            HttpClient cliente =
                _httpClientFactory.CreateClient("UsersApi");

            using HttpRequestMessage solicitud =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/users/{usuarioId}");

            AgregarCorrelationId(solicitud);

            using HttpResponseMessage respuesta =
                await cliente.SendAsync(
                    solicitud, cancellationToken);

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException(
                    ErrorCodes.ORD_003,
                    "Usuario no encontrado al crear la orden.");
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    "Error al consultar Users.API.");
            }
        }

        private async Task<ProductApiResponse> ObtenerProductoAsync(
            Guid productoId,
            CancellationToken cancellationToken)
        {
            HttpClient cliente =
                _httpClientFactory.CreateClient("ProductsApi");

            using HttpRequestMessage solicitud =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/products/{productoId}");

            AgregarCorrelationId(solicitud);

            using HttpResponseMessage respuesta =
                await cliente.SendAsync(
                    solicitud, cancellationToken);

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException(
                    ErrorCodes.ORD_004,
                    "Producto no encontrado al crear la orden.");
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    "Error al consultar Products.API.");
            }

            ProductApiResponse producto =
                await respuesta.Content
                    .ReadFromJsonAsync<ProductApiResponse>(
                        cancellationToken: cancellationToken);

            if (producto == null)
            {
                throw new InvalidOperationException(
                    "Products.API devolvio una respuesta vacia.");
            }

            return producto;
        }

        private void AgregarCorrelationId(
            HttpRequestMessage solicitud)
        {
            HttpContext contexto =
                _httpContextAccessor.HttpContext;

            if (contexto != null &&
                contexto.Items.TryGetValue(
                    "X-Correlation-Id", out object valor) &&
                valor != null)
            {
                solicitud.Headers.TryAddWithoutValidation(
                    "X-Correlation-Id",
                    valor.ToString());
            }
        }

        private static OrderResponse MapearA_Response(Order orden)
        {
            OrderResponse response = new OrderResponse
            {
                Id = orden.Id,
                UsuarioId = orden.UsuarioId,
                Total = orden.Total,
                Estado = orden.Estado,
                FechaCreacion = orden.FechaCreacion,
                FechaActualizacion = orden.FechaActualizacion,
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