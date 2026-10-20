using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Cart.API.DTOs;
using Cart.API.Exceptions;
using CartModel = Cart.API.Models.Cart;

namespace Cart.API.Services
{
    public class MockRepository
    {
        private readonly List<CartModel> _db = new List<CartModel>();
        public CartModel Get(Guid id) => _db.FirstOrDefault(c => c.UsuarioId == id);
        public void Update(CartModel cart)
        {
            if (!_db.Contains(cart)) _db.Add(cart);
        }
    }

    public class CartService
    {
        private static readonly MockRepository _repository = new MockRepository();
        private readonly IHttpClientFactory _httpClientFactory;

        public CartService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private async Task ValidarStockAsync(Guid productId, int cantidadSolicitada)
        {
            var client = _httpClientFactory.CreateClient("ProductsAPI");
            var response = await client.GetAsync($"/api/products/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new NotFoundException("CRT-002", "Producto no encontrado.");
            }

            var producto = await response.Content.ReadFromJsonAsync<ProductResponse>();
            if (producto != null && producto.Stock < cantidadSolicitada)
            {
                throw new BusinessRuleException("CRT-003", $"Stock insuficiente. Disponible: {producto.Stock}, solicitado: {cantidadSolicitada}.");
            }
        }

        public async Task<CartResponse> AgregarItemAsync(Guid userId, AddCartItemRequest request)
        {
            if (request.Cantidad <= 0)
                throw new ValidationException("CRT-004", "Cantidad inválida.");

            CartModel carrito = _repository.Get(userId);

            if (carrito == null)
            {
                carrito = new CartModel
                {
                    UsuarioId = userId,
                    Items = new List<Cart.API.Models.CartItem>(),
                    FechaActualizacion = DateTime.UtcNow
                };
            }

            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == request.ProductoId);
            int cantidadFinal = item != null ? item.Cantidad + request.Cantidad : request.Cantidad;

            await ValidarStockAsync(request.ProductoId, cantidadFinal);

            if (item != null)
            {
                item.Cantidad = cantidadFinal;
            }
            else
            {
                carrito.Items.Add(new Cart.API.Models.CartItem
                {
                    ProductoId = request.ProductoId,
                    Cantidad = request.Cantidad
                });
            }

            carrito.FechaActualizacion = DateTime.UtcNow;
            _repository.Update(carrito);

            return MapToResponse(carrito);
        }

        public CartResponse GetCart(Guid userId)
        {
            CartModel carrito = _repository.Get(userId);
            if (carrito == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            return MapToResponse(carrito);
        }

        public async Task<CartResponse> UpdateItemQuantityAsync(Guid userId, Guid productId, int nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ValidationException("CRT-004", "Cantidad inválida.");

            CartModel carrito = _repository.Get(userId);
            if (carrito == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == productId);
            if (item == null)
                throw new NotFoundException("CRT-002", "Producto no encontrado.");

            await ValidarStockAsync(productId, nuevaCantidad);

            item.Cantidad = nuevaCantidad;
            carrito.FechaActualizacion = DateTime.UtcNow;

            _repository.Update(carrito);
            return MapToResponse(carrito);
        }

        public void RemoveItem(Guid userId, Guid productId)
        {
            CartModel carrito = _repository.Get(userId);
            if (carrito == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == productId);
            if (item != null)
            {
                carrito.Items.Remove(item);
                carrito.FechaActualizacion = DateTime.UtcNow;
                _repository.Update(carrito);
            }
        }

        public void ClearCart(Guid userId)
        {
            CartModel carrito = _repository.Get(userId);
            if (carrito == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            carrito.Items.Clear();
            carrito.FechaActualizacion = DateTime.UtcNow;
            _repository.Update(carrito);
        }

        private CartResponse MapToResponse(CartModel carrito)
        {
            return new CartResponse
            {
                UsuarioId = carrito.UsuarioId,
                FechaActualizacion = carrito.FechaActualizacion,
                Items = carrito.Items.Select(i => new CartItemResponse
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad
                }).ToList()
            };
        }
    }
}