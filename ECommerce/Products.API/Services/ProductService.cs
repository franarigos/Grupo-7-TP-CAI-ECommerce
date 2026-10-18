using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services
{
    public class ProductService
    {
        private static readonly List<Product> _productos = new();

        public List<ProductResponse> GetProducts(string? categoria, string? nombre)
        {
            var query = _productos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(p => p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));

            return query.Select(MapToResponse).ToList();
        }

        public ProductResponse GetProductById(Guid id)
        {
            var producto = _productos.FirstOrDefault(p => p.Id == id);
            if (producto == null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");

            return MapToResponse(producto);
        }

        public ProductResponse CreateProduct(ProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) || request.Precio <= 0)
                throw new ValidationException("PRD-002", "Los datos del producto son inválidos.");

            if (_productos.Any(p => p.Nombre.Equals(request.Nombre, StringComparison.OrdinalIgnoreCase) &&
                                    p.Categoria.Equals(request.Categoria, StringComparison.OrdinalIgnoreCase)))
                throw new BusinessRuleException("PRD-003", $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");

            var nuevoProducto = new Product
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Precio = request.Precio,
                Stock = request.Stock,
                Categoria = request.Categoria,
                FechaCreacion = DateTime.UtcNow
            };

            _productos.Add(nuevoProducto);
            return MapToResponse(nuevoProducto);
        }

        public ProductResponse UpdateProduct(Guid id, ProductRequest request)
        {
            var producto = _productos.FirstOrDefault(p => p.Id == id);
            if (producto == null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");

            if (string.IsNullOrWhiteSpace(request.Nombre) || request.Precio <= 0)
                throw new ValidationException("PRD-002", "Los datos del producto son inválidos.");

            producto.Nombre = request.Nombre;
            producto.Descripcion = request.Descripcion;
            producto.Precio = request.Precio;
            producto.Stock = request.Stock;
            producto.Categoria = request.Categoria;

            return MapToResponse(producto);
        }

        public void DeleteProduct(Guid id)
        {
            var producto = _productos.FirstOrDefault(p => p.Id == id);
            if (producto == null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");

            _productos.Remove(producto);
        }

        private static ProductResponse MapToResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Nombre = product.Nombre,
                Descripcion = product.Descripcion,
                Precio = product.Precio,
                Stock = product.Stock,
                Categoria = product.Categoria,
                FechaCreacion = product.FechaCreacion
            };
        }
    }
}