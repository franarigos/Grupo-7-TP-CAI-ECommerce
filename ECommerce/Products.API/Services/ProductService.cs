using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services
{
    public class ProductService
    {
        private static readonly List<Product> _productos =
            new List<Product>();

        private static readonly object _candado = new object();

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public List<ProductResponse> GetProducts(
            string? categoria,
            string? nombre)
        {
            List<ProductResponse> resultado =
                new List<ProductResponse>();

            lock (_candado)
            {
                foreach (Product producto in _productos)
                {
                    bool coincideCategoria =
                        string.IsNullOrWhiteSpace(categoria) ||
                        string.Equals(
                            producto.Categoria,
                            categoria,
                            StringComparison.OrdinalIgnoreCase);

                    bool coincideNombre =
                        string.IsNullOrWhiteSpace(nombre) ||
                        producto.Nombre.Contains(
                            nombre,
                            StringComparison.OrdinalIgnoreCase);

                    if (coincideCategoria && coincideNombre)
                    {
                        resultado.Add(MapToResponse(producto));
                    }
                }
            }

            return resultado;
        }

        public ProductResponse GetProductById(Guid id)
        {
            lock (_candado)
            {
                foreach (Product producto in _productos)
                {
                    if (producto.Id == id)
                    {
                        return MapToResponse(producto);
                    }
                }
            }

            throw new NotFoundException(
                "PRD-001",
                "Producto no encontrado.");
        }

        public ProductResponse CreateProduct(ProductRequest request)
        {
            ValidarProducto(request);

            lock (_candado)
            {
                foreach (Product producto in _productos)
                {
                    if (string.Equals(
                            producto.Nombre,
                            request.Nombre,
                            StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(
                            producto.Categoria,
                            request.Categoria,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        throw new BusinessRuleException(
                            "PRD-003",
                            $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");
                    }
                }

                Product nuevoProducto = new Product
                {
                    Id = Guid.NewGuid(),
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion ?? string.Empty,
                    Precio = request.Precio,
                    Stock = request.Stock,
                    Categoria = request.Categoria,
                    FechaCreacion = DateTime.UtcNow
                };

                _productos.Add(nuevoProducto);

                return MapToResponse(nuevoProducto);
            }
        }

        public ProductResponse UpdateProduct(
            Guid id,
            ProductRequest request)
        {
            lock (_candado)
            {
                foreach (Product producto in _productos)
                {
                    if (producto.Id == id)
                    {
                        ValidarProducto(request);

                        producto.Nombre = request.Nombre;
                        producto.Descripcion =
                            request.Descripcion ?? string.Empty;
                        producto.Precio = request.Precio;
                        producto.Stock = request.Stock;
                        producto.Categoria = request.Categoria;

                        return MapToResponse(producto);
                    }
                }
            }

            throw new NotFoundException(
                "PRD-001",
                "Producto no encontrado.");
        }

        public async Task DeleteProductAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            GetProductById(id);

            bool tieneOrdenesActivas =
                await TieneOrdenesActivasAsync(
                    id,
                    cancellationToken);

            if (tieneOrdenesActivas)
            {
                throw new BusinessRuleException(
                    "PRD-004",
                    "El producto tiene órdenes activas y no puede eliminarse.");
            }

            lock (_candado)
            {
                foreach (Product producto in _productos)
                {
                    if (producto.Id == id)
                    {
                        _productos.Remove(producto);
                        return;
                    }
                }
            }

            throw new NotFoundException(
                "PRD-001",
                "Producto no encontrado.");
        }

        private async Task<bool> TieneOrdenesActivasAsync(
            Guid productoId,
            CancellationToken cancellationToken)
        {
            HttpClient cliente =
                _httpClientFactory.CreateClient("OrdersApi");

            using HttpRequestMessage solicitud =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/orders");

            HttpContext? contexto =
                _httpContextAccessor.HttpContext;

            if (contexto != null)
            {
                string correlationId =
                    contexto.Items["CorrelationId"]?.ToString()
                    ?? contexto.TraceIdentifier;

                solicitud.Headers.TryAddWithoutValidation(
                    "X-Correlation-Id",
                    correlationId);
            }

            using HttpResponseMessage respuesta =
                await cliente.SendAsync(
                    solicitud,
                    cancellationToken);

            if (!respuesta.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    "Error al consultar Orders.API.");
            }

            List<OrdersApiResponse>? ordenes =
                await respuesta.Content.ReadFromJsonAsync<
                    List<OrdersApiResponse>>(
                    cancellationToken: cancellationToken);

            if (ordenes == null)
            {
                throw new InvalidOperationException(
                    "Orders.API devolvió una respuesta inválida.");
            }

            foreach (OrdersApiResponse orden in ordenes)
            {
                if (orden.Estado == "Pendiente" ||
                    orden.Estado == "Confirmada")
                {
                    foreach (OrderItemApiResponse item in orden.Items)
                    {
                        if (item.ProductoId == productoId)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void ValidarProducto(ProductRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Nombre) ||
                request.Nombre.Length > 100 ||
                string.IsNullOrWhiteSpace(request.Categoria) ||
                (request.Descripcion != null &&
                 request.Descripcion.Length > 500) ||
                request.Precio <= 0 ||
                request.Stock < 0)
            {
                throw new ValidationException(
                    "PRD-002",
                    "Los datos del producto son inválidos.");
            }
        }

        private ProductResponse MapToResponse(Product producto)
        {
            return new ProductResponse
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                Categoria = producto.Categoria,
                FechaCreacion = producto.FechaCreacion
            };
        }
    }
}