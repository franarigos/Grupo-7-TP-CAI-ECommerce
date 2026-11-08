
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Products.API.Models;

namespace Products.API.Data
{
    public class ProductRepository
    {
        private readonly IConfiguration _config;

        public ProductRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqliteConnection CreateConnection()
        {
            string connectionString =
                _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=products.db";

            return new SqliteConnection(connectionString);
        }

        public async Task<List<Product>> GetAllAsync()
        {
            using SqliteConnection connection = CreateConnection();

            IEnumerable<ProductRow> filas =
                await connection.QueryAsync<ProductRow>("""
                    SELECT
                        id AS Id,
                        nombre AS Nombre,
                        descripcion AS Descripcion,
                        precio AS Precio,
                        stock AS Stock,
                        categoria AS Categoria,
                        fecha_creacion AS FechaCreacion
                    FROM productos
                    """);

            List<Product> productos = new List<Product>();

            foreach (ProductRow fila in filas)
            {
                productos.Add(MapearProducto(fila));
            }

            return productos;
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            using SqliteConnection connection = CreateConnection();

            ProductRow? fila =
                await connection.QuerySingleOrDefaultAsync<ProductRow>(
                    """
                    SELECT
                        id AS Id,
                        nombre AS Nombre,
                        descripcion AS Descripcion,
                        precio AS Precio,
                        stock AS Stock,
                        categoria AS Categoria,
                        fecha_creacion AS FechaCreacion
                    FROM productos
                    WHERE id = @Id
                    """,
                    new { Id = id.ToString() });

            if (fila == null)
            {
                return null;
            }

            return MapearProducto(fila);
        }

        public async Task CreateAsync(Product producto)
        {
            using SqliteConnection connection = CreateConnection();

            await connection.ExecuteAsync(
                """
                INSERT INTO productos
                (
                    id,
                    nombre,
                    descripcion,
                    precio,
                    stock,
                    categoria,
                    fecha_creacion
                )
                VALUES
                (
                    @Id,
                    @Nombre,
                    @Descripcion,
                    @Precio,
                    @Stock,
                    @Categoria,
                    @FechaCreacion
                )
                """,
                new
                {
                    Id = producto.Id.ToString(),
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio.ToString(
                        CultureInfo.InvariantCulture),
                    Stock = producto.Stock,
                    Categoria = producto.Categoria,
                    FechaCreacion = producto.FechaCreacion.ToString(
                        "O",
                        CultureInfo.InvariantCulture)
                });
        }

        public async Task<bool> UpdateAsync(Product producto)
        {
            using SqliteConnection connection = CreateConnection();

            int filasModificadas =
                await connection.ExecuteAsync(
                    """
                    UPDATE productos
                    SET
                        nombre = @Nombre,
                        descripcion = @Descripcion,
                        precio = @Precio,
                        stock = @Stock,
                        categoria = @Categoria
                    WHERE id = @Id
                    """,
                    new
                    {
                        Id = producto.Id.ToString(),
                        Nombre = producto.Nombre,
                        Descripcion = producto.Descripcion,
                        Precio = producto.Precio.ToString(
                            CultureInfo.InvariantCulture),
                        Stock = producto.Stock,
                        Categoria = producto.Categoria
                    });

            return filasModificadas > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            using SqliteConnection connection = CreateConnection();

            int filasEliminadas =
                await connection.ExecuteAsync(
                    """
                    DELETE FROM productos
                    WHERE id = @Id
                    """,
                    new { Id = id.ToString() });

            return filasEliminadas > 0;
        }

        private Product MapearProducto(ProductRow fila)
        {
            Product producto = new Product();

            producto.Id = Guid.Parse(fila.Id);
            producto.Nombre = fila.Nombre;
            producto.Descripcion = fila.Descripcion;
            producto.Precio = decimal.Parse(
                fila.Precio,
                CultureInfo.InvariantCulture);
            producto.Stock = fila.Stock;
            producto.Categoria = fila.Categoria;
            producto.FechaCreacion = DateTime.Parse(
                fila.FechaCreacion,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

            return producto;
        }
    }

    public class ProductRow
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Precio { get; set; } = string.Empty;
        public int Stock { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string FechaCreacion { get; set; } = string.Empty;
    }
}
