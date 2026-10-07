using Products.API.DTOs;
using Products.API.Exceptions;

namespace Products.API.Services
{
    public class ProductService
    {
        public List<ProductResponse> GetProducts(string? categoria, string? nombre)
        {
            return new List<ProductResponse>();
        }

        public ProductResponse GetProductById(Guid id)
        {
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
        }

        public ProductResponse CreateProduct(ProductRequest request)
        {
            return new ProductResponse();
        }

        public ProductResponse UpdateProduct(Guid id, ProductRequest request)
        {
            return new ProductResponse();
        }

        public void DeleteProduct(Guid id)
        {
        }
    }
}