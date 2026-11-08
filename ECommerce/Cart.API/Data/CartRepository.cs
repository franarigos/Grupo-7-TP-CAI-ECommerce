using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using CartModel = global::Cart.API.Models.Cart;
using CartItemModel = global::Cart.API.Models.CartItem;

namespace Cart.API.Data
{
    public class CartRepository
    {
        private readonly IConfiguration _config;

        public CartRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqliteConnection CreateConnection()
        {
            string connectionString =
                _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=cart.db;Foreign Keys=True";

            return new SqliteConnection(connectionString);
        }

        public async Task<CartModel?> GetByUserIdAsync(Guid userId)
        {
            using SqliteConnection connection = CreateConnection();

            connection.Open();

            CartRow? fila =
                await connection.QuerySingleOrDefaultAsync<CartRow>(
                    @"
                    SELECT
                        usuario_id AS UsuarioId,
                        fecha_actualizacion AS FechaActualizacion
                    FROM carritos
                    WHERE usuario_id = @UsuarioId
                    ",
                    new { UsuarioId = userId.ToString() });

            if (fila == null)
            {
                return null;
            }

            CartModel carrito = new CartModel();

            carrito.UsuarioId = Guid.Parse(fila.UsuarioId);
            carrito.FechaActualizacion = DateTime.Parse(
                fila.FechaActualizacion,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

            carrito.Items = new List<CartItemModel>();

            IEnumerable<CartItemRow> filasItems =
                await connection.QueryAsync<CartItemRow>(
                    @"
                    SELECT
                        producto_id AS ProductoId,
                        cantidad AS Cantidad
                    FROM carrito_items
                    WHERE usuario_id = @UsuarioId
                    ",
                    new { UsuarioId = userId.ToString() });

            foreach (CartItemRow filaItem in filasItems)
            {
                CartItemModel item = new CartItemModel();

                item.ProductoId = Guid.Parse(filaItem.ProductoId);
                item.Cantidad = filaItem.Cantidad;

                carrito.Items.Add(item);
            }

            return carrito;
        }

        public async Task SaveAsync(CartModel carrito)
        {
            using SqliteConnection connection = CreateConnection();

            connection.Open();

            using SqliteTransaction transaction =
                connection.BeginTransaction();

            await connection.ExecuteAsync(
                @"
                INSERT INTO carritos
                (
                    usuario_id,
                    fecha_actualizacion
                )
                VALUES
                (
                    @UsuarioId,
                    @FechaActualizacion
                )
                ON CONFLICT(usuario_id)
                DO UPDATE SET
                    fecha_actualizacion = @FechaActualizacion
                ",
                new
                {
                    UsuarioId = carrito.UsuarioId.ToString(),
                    FechaActualizacion =
                        carrito.FechaActualizacion.ToString(
                            "O",
                            CultureInfo.InvariantCulture)
                },
                transaction);

            await connection.ExecuteAsync(
                @"
                DELETE FROM carrito_items
                WHERE usuario_id = @UsuarioId
                ",
                new
                {
                    UsuarioId = carrito.UsuarioId.ToString()
                },
                transaction);

            foreach (CartItemModel item in carrito.Items)
            {
                await connection.ExecuteAsync(
                    @"
                    INSERT INTO carrito_items
                    (
                        usuario_id,
                        producto_id,
                        cantidad
                    )
                    VALUES
                    (
                        @UsuarioId,
                        @ProductoId,
                        @Cantidad
                    )
                    ",
                    new
                    {
                        UsuarioId = carrito.UsuarioId.ToString(),
                        ProductoId = item.ProductoId.ToString(),
                        Cantidad = item.Cantidad
                    },
                    transaction);
            }

            transaction.Commit();
        }
    }

    public class CartRow
    {
        public string UsuarioId { get; set; } = string.Empty;

        public string FechaActualizacion { get; set; } =
            string.Empty;
    }

    public class CartItemRow
    {
        public string ProductoId { get; set; } = string.Empty;

        public int Cantidad { get; set; }
    }
}