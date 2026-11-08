
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Products.API.Data;
using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services
{
    public class ProductService
    {
        private readonly ProductRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductService(
            ProductRepository repository,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<ProductResponse>> GetProductsAsync(
            string? categoria,
            string? nombre)
        {
            List<Product> productos =
                await _repository.GetAllAsync();

            List<ProductResponse> resultado =
                new List<ProductResponse>();

            foreach (Product producto in productos)
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

            return resultado;
        }

        public async Task<ProductResponse> GetProductByIdAsync(
            Guid id)
        {
            Product? producto =
                await _repository.GetByIdAsync(id);

            if (producto == null)
            {
                throw new NotFoundException(
                    "PRD-001",
                    "Producto no encontrado.");
            }

            return MapToResponse(producto);
        }

        public async Task<ProductResponse> CreateProductAsync(
            ProductRequest request)
        {
            ValidarProducto(request);

            string nombre = request.Nombre.Trim();
            string categoria = request.Categoria.Trim();

            List<Product> productos =
                await _repository.GetAllAsync();

            foreach (Product producto in productos)
            {
                if (string.Equals(
                        producto.Nombre,
                        nombre,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        producto.Categoria,
                        categoria,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new BusinessRuleException(
                        "PRD-003",
                        $"Ya existe un producto con ese nombre en la categoría '{categoria}'.");
                }
            }

            Product nuevoProducto = new Product
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = request.Descripcion ?? string.Empty,
                Precio = request.Precio,
                Stock = request.Stock,
                Categoria = categoria,
                FechaCreacion = DateTime.UtcNow
            };

            await _repository.CreateAsync(nuevoProducto);

            return MapToResponse(nuevoProducto);
        }

        public async Task<ProductResponse> UpdateProductAsync(
            Guid id,
            ProductRequest request)
        {
            Product? producto =
                await _repository.GetByIdAsync(id);

            if (producto == null)
            {
                throw new NotFoundException(
                    "PRD-001",
                    "Producto no encontrado.");
            }

            ValidarProducto(request);

            producto.Nombre = request.Nombre.Trim();
            producto.Descripcion =
                request.Descripcion ?? string.Empty;
            producto.Precio = request.Precio;
            producto.Stock = request.Stock;
            producto.Categoria = request.Categoria.Trim();

            bool actualizado =
                await _repository.UpdateAsync(producto);

            if (!actualizado)
            {
                throw new NotFoundException(
                    "PRD-001",
                    "Producto no encontrado.");
            }

            return MapToResponse(producto);
        }

        public async Task DeleteProductAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            Product? producto =
                await _repository.GetByIdAsync(id);

            if (producto == null)
            {
                throw new NotFoundException(
                    "PRD-001",
                    "Producto no encontrado.");
            }

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

            bool eliminado = await _repository.DeleteAsync(id);

            if (!eliminado)
            {
                throw new NotFoundException(
                    "PRD-001",
                    "Producto no encontrado.");
            }
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
