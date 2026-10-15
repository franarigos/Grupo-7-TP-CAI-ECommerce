using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Models;

namespace Cart.API.Services
{
    public class CartService
    {
        private static readonly List<Models.Cart> _carritosDb = new List<Models.Cart>();
        private readonly HttpClient _httpClient;

        public CartService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ProductsAPI");
        }

        public async Task<Models.Cart> AgregarItemAsync(Guid userId, AddCartItemRequest request)
        {
            if (request.Cantidad <= 0)
            {
                throw new ValidationException("CRT-004", "Cantidad inválida.");
            }

            var response = await _httpClient.GetAsync($"/api/products/{request.ProductoId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new NotFoundException("CRT-002", "Producto no encontrado.");
                }
                throw new Exception("Error de conexión con Products.API.");
            }

            var producto = await response.Content.ReadFromJsonAsync<ProductResponseDto>();

            if (producto == null || producto.Stock < request.Cantidad)
            {
                throw new BusinessRuleException("CRT-003", $"Stock insuficiente. Disponible: {producto?.Stock ?? 0}, solicitado: {request.Cantidad}.");
            }

            var carrito = _carritosDb.Find(c => c.UsuarioId == userId);
            if (carrito == null)
            {
                carrito = new Models.Cart { UsuarioId = userId, FechaActualizacion = DateTime.UtcNow };
                _carritosDb.Add(carrito);
            }

            var itemExistente = carrito.Items.Find(i => i.ProductoId == request.ProductoId);
            if (itemExistente != null)
            {
                if (producto.Stock < (itemExistente.Cantidad + request.Cantidad))
                {
                    throw new BusinessRuleException("CRT-003", "Stock insuficiente para sumar a la cantidad que ya posees en el carrito.");
                }
                itemExistente.Cantidad += request.Cantidad;
            }
            else
            {
                carrito.Items.Add(new CartItem { ProductoId = request.ProductoId, Cantidad = request.Cantidad });
            }

            carrito.FechaActualizacion = DateTime.UtcNow;
            return carrito;
        }
    }
}
