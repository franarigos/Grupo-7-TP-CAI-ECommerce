using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using global::Cart.API.Data;
using global::Cart.API.DTOs;
using global::Cart.API.Exceptions;
using CartModel = global::Cart.API.Models.Cart;
using CartItemModel = global::Cart.API.Models.CartItem;

namespace Cart.API.Services
{
    public class CartService
    {
        private readonly CartRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(
            CartRepository repository,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CartResponse> GetCartAsync(Guid userId)
        {
            CartModel? carrito =
                await _repository.GetByUserIdAsync(userId);

            if (carrito == null)
            {
                throw new NotFoundException(
                    "CRT-001",
                    "Carrito no encontrado.");
            }

            return MapToResponse(carrito);
        }

        public async Task<CartResponse> AgregarItemAsync(
            Guid userId,
            AddCartItemRequest request)
        {
            if (request == null || request.Cantidad <= 0)
            {
                throw new ValidationException(
                    "CRT-004",
                    "Cantidad inválida.");
            }

            CartModel? carrito =
                await _repository.GetByUserIdAsync(userId);

            if (carrito == null)
            {
                carrito = new CartModel
                {
                    UsuarioId = userId,
                    Items = new List<CartItemModel>(),
                    FechaActualizacion = DateTime.UtcNow
                };
            }

            CartItemModel? itemExistente = null;

            foreach (CartItemModel item in carrito.Items)
            {
                if (item.ProductoId == request.ProductoId)
                {
                    itemExistente = item;
                    break;
                }
            }

            long cantidadFinal = request.Cantidad;

            if (itemExistente != null)
            {
                cantidadFinal += itemExistente.Cantidad;
            }

            if (cantidadFinal > int.MaxValue)
            {
                throw new BusinessRuleException(
                    "CRT-003",
                    "Stock insuficiente para agregar al carrito.");
            }

            await ValidarStockAsync(
                request.ProductoId,
                (int)cantidadFinal);

            if (itemExistente != null)
            {
                itemExistente.Cantidad = (int)cantidadFinal;
            }
            else
            {
                CartItemModel nuevoItem = new CartItemModel
                {
                    ProductoId = request.ProductoId,
                    Cantidad = request.Cantidad
                };

                carrito.Items.Add(nuevoItem);
            }

            carrito.FechaActualizacion = DateTime.UtcNow;

            await _repository.SaveAsync(carrito);

            return MapToResponse(carrito);
        }

        public async Task<CartResponse> UpdateItemQuantityAsync(
            Guid userId,
            Guid productId,
            int nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
            {
                throw new ValidationException(
                    "CRT-004",
                    "Cantidad inválida.");
            }

            CartModel? carrito =
                await _repository.GetByUserIdAsync(userId);

            if (carrito == null)
            {
                throw new NotFoundException(
                    "CRT-001",
                    "Carrito no encontrado.");
            }

            CartItemModel? itemExistente = null;

            foreach (CartItemModel item in carrito.Items)
            {
                if (item.ProductoId == productId)
                {
                    itemExistente = item;
                    break;
                }
            }

            if (itemExistente == null)
            {
                throw new NotFoundException(
                    "CRT-002",
                    "Producto no encontrado.");
            }

            await ValidarStockAsync(
                productId,
                nuevaCantidad);

            itemExistente.Cantidad = nuevaCantidad;
            carrito.FechaActualizacion = DateTime.UtcNow;

            await _repository.SaveAsync(carrito);

            return MapToResponse(carrito);
        }

        public async Task RemoveItemAsync(
            Guid userId,
            Guid productId)
        {
            CartModel? carrito =
                await _repository.GetByUserIdAsync(userId);

            if (carrito == null)
            {
                throw new NotFoundException(
                    "CRT-001",
                    "Carrito no encontrado.");
            }

            CartItemModel? itemExistente = null;

            foreach (CartItemModel item in carrito.Items)
            {
                if (item.ProductoId == productId)
                {
                    itemExistente = item;
                    break;
                }
            }

            if (itemExistente != null)
            {
                carrito.Items.Remove(itemExistente);
                carrito.FechaActualizacion = DateTime.UtcNow;

                await _repository.SaveAsync(carrito);
            }
        }

        public async Task ClearCartAsync(Guid userId)
        {
            CartModel? carrito =
                await _repository.GetByUserIdAsync(userId);

            if (carrito == null)
            {
                throw new NotFoundException(
                    "CRT-001",
                    "Carrito no encontrado.");
            }

            carrito.Items.Clear();
            carrito.FechaActualizacion = DateTime.UtcNow;

            await _repository.SaveAsync(carrito);
        }

        private async Task ValidarStockAsync(
            Guid productId,
            int cantidadSolicitada)
        {
            HttpClient client =
                _httpClientFactory.CreateClient("ProductsAPI");

            using HttpRequestMessage solicitud =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/products/{productId}");

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
                await client.SendAsync(solicitud);

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException(
                    "CRT-002",
                    "Producto no encontrado.");
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    "No se pudo consultar Products.API.");
            }

            ProductResponse? producto =
                await respuesta.Content.ReadFromJsonAsync<
                    ProductResponse>();

            if (producto == null)
            {
                throw new InvalidOperationException(
                    "Products.API devolvió una respuesta inválida.");
            }

            if (producto.Stock < cantidadSolicitada)
            {
                throw new BusinessRuleException(
                    "CRT-003",
                    $"Stock insuficiente. Disponible: {producto.Stock}, solicitado: {cantidadSolicitada}.");
            }
        }

        private CartResponse MapToResponse(CartModel carrito)
        {
            CartResponse response = new CartResponse();

            response.UsuarioId = carrito.UsuarioId;
            response.FechaActualizacion =
                carrito.FechaActualizacion;

            response.Items = new List<CartItemResponse>();

            foreach (CartItemModel item in carrito.Items)
            {
                CartItemResponse itemResponse =
                    new CartItemResponse();

                itemResponse.ProductoId = item.ProductoId;
                itemResponse.Cantidad = item.Cantidad;

                response.Items.Add(itemResponse);
            }

            return response;
        }
    }
}