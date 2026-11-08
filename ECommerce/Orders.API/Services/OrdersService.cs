
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Orders.API.Data;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;

namespace Orders.API.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly OrderRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrdersService(
            OrderRepository repository,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
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
                    "Los datos de la orden son inválidos.");
            }

            foreach (OrderItemRequest item in request.Items)
            {
                if (item == null ||
                    item.ProductoId == Guid.Empty ||
                    item.Cantidad <= 0)
                {
                    throw new ValidationException(
                        ErrorCodes.ORD_002,
                        "Los productos y las cantidades son inválidos.");
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

            foreach (OrderItemRequest itemRequest in request.Items)
            {
                OrderItem existente = null;

                foreach (OrderItem item in nuevaOrden.Items)
                {
                    if (item.ProductoId == itemRequest.ProductoId)
                    {
                        existente = item;
                        break;
                    }
                }

                long cantidadTotal = itemRequest.Cantidad;

                if (existente != null)
                {
                    cantidadTotal += existente.Cantidad;
                }

                if (cantidadTotal > int.MaxValue)
                {
                    throw new ValidationException(
                        ErrorCodes.ORD_002,
                        "La cantidad solicitada es inválida.");
                }

                ProductApiResponse producto =
                    await ObtenerProductoAsync(
                        itemRequest.ProductoId,
                        cancellationToken);

                if (cantidadTotal > producto.Stock)
                {
                    throw new BusinessRuleException(
                        ErrorCodes.ORD_005,
                        $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}, solicitado: {cantidadTotal}.");
                }

                if (existente != null)
                {
                    existente.Cantidad = (int)cantidadTotal;
                }
                else
                {
                    OrderItem nuevoItem = new OrderItem
                    {
                        ProductoId = itemRequest.ProductoId,
                        Cantidad = (int)cantidadTotal,
                        PrecioUnitario = producto.Precio
                    };

                    nuevaOrden.Items.Add(nuevoItem);
                }
            }

            foreach (OrderItem item in nuevaOrden.Items)
            {
                nuevaOrden.Total +=
                    item.Cantidad * item.PrecioUnitario;
            }

            await _repository.CreateAsync(nuevaOrden);

            return MapearAResponse(nuevaOrden);
        }

        public async Task<OrderResponse> ObtenerOrdenPorIdAsync(
            Guid id)
        {
            Order orden = await _repository.GetByIdAsync(id);

            if (orden == null)
            {
                throw new NotFoundException(
                    ErrorCodes.ORD_001,
                    "Orden no encontrada.");
            }

            return MapearAResponse(orden);
        }

        public async Task<List<OrderResponse>> ObtenerOrdenesAsync(
            Guid? usuarioId)
        {
            List<Order> ordenes =
                await _repository.GetAllAsync(usuarioId);

            List<OrderResponse> resultado =
                new List<OrderResponse>();

            foreach (Order orden in ordenes)
            {
                resultado.Add(MapearAResponse(orden));
            }

            return resultado;
        }

        public async Task<OrderResponse> ActualizarEstadoAsync(
            Guid id,
            UpdateOrderStatusRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Estado) ||
                !EsEstadoValido(request.Estado))
            {
                throw new ValidationException(
                    ErrorCodes.ORD_002,
                    "El estado ingresado es inválido.");
            }

            Order orden = await _repository.GetByIdAsync(id);

            if (orden == null)
            {
                throw new NotFoundException(
                    ErrorCodes.ORD_001,
                    "Orden no encontrada.");
            }

            if (orden.Estado == request.Estado)
            {
                return MapearAResponse(orden);
            }

            if (!PuedeCambiarEstado(
                orden.Estado,
                request.Estado))
            {
                throw new BusinessRuleException(
                    ErrorCodes.ORD_006,
                    $"Una orden en estado '{orden.Estado}' no puede pasar a '{request.Estado}'.");
            }

            DateTime fecha = DateTime.UtcNow;

            bool actualizado =
                await _repository.UpdateStatusAsync(
                    id,
                    orden.Estado,
                    request.Estado,
                    fecha);

            if (!actualizado)
            {
                throw new BusinessRuleException(
                    ErrorCodes.ORD_006,
                    "El estado de la orden fue modificado por otra solicitud.");
            }

            orden.Estado = request.Estado;
            orden.FechaActualizacion = fecha;

            return MapearAResponse(orden);
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
                    solicitud,
                    cancellationToken);

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
                    solicitud,
                    cancellationToken);

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
                await respuesta.Content.ReadFromJsonAsync<
                    ProductApiResponse>(
                    cancellationToken: cancellationToken);

            if (producto == null)
            {
                throw new InvalidOperationException(
                    "Products.API devolvió una respuesta inválida.");
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
                    "X-Correlation-Id",
                    out object valor) &&
                valor != null)
            {
                solicitud.Headers.TryAddWithoutValidation(
                    "X-Correlation-Id",
                    valor.ToString());
            }
        }

        private static OrderResponse MapearAResponse(
            Order orden)
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
