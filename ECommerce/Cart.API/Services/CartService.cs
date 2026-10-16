using System;
using System.Linq;
using System.Threading.Tasks;
using Cart.API.DTOs;
using Cart.API.Exceptions;
using CartModel = Cart.API.Models.Cart; 

namespace Cart.API.Services
{
    public class CartService
    {
        private readonly dynamic _repository;

        public CartService(dynamic repository)
        {
            _repository = repository;
        }

        public async Task<CartResponse> AgregarItemAsync(Guid userId, AddCartItemRequest request)
        {
            CartModel carrito = _repository.Get(userId);
            if (carrito == null)
            {
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");
            }

            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == request.ProductoId);
            if (item != null)
            {
                item.Cantidad += request.Cantidad;
            }
            else
            {
                throw new NotFoundException("CRT-002", "Producto no encontrado.");
            }

            carrito.FechaActualizacion = DateTime.UtcNow;
            _repository.Update(carrito);

            return MapToResponse(carrito);
        }

        public CartResponse GetCart(Guid userId)
        {
            CartModel carrito = _repository.Get(userId);

            if (carrito == null)
            {
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");
            }

            return MapToResponse(carrito);
        }

        public CartResponse UpdateItemQuantity(Guid userId, Guid productId, int nuevaCantidad)
        {
            CartModel carrito = _repository.Get(userId);
            if (carrito == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == productId);
            if (item == null)
                throw new NotFoundException("CRT-002", "Producto no encontrado en el carrito.");

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