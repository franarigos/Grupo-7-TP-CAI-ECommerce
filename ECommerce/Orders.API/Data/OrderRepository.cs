
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Orders.API.Models;

namespace Orders.API.Data
{
    public class OrderRepository
    {
        private readonly IConfiguration _config;

        public OrderRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqliteConnection CreateConnection()
        {
            string connectionString =
                _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=orders.db;Foreign Keys=True";

            return new SqliteConnection(connectionString);
        }

        public async Task<List<Order>> GetAllAsync(
            Guid? usuarioId)
        {
            using SqliteConnection connection = CreateConnection();

            connection.Open();

            string usuario = null;

            if (usuarioId.HasValue)
            {
                usuario = usuarioId.Value.ToString();
            }

            IEnumerable<OrderRow> filas =
                await connection.QueryAsync<OrderRow>(
                    @"
                    SELECT
                        id AS Id,
                        usuario_id AS UsuarioId,
                        total AS Total,
                        estado AS Estado,
                        fecha_creacion AS FechaCreacion,
                        fecha_actualizacion AS FechaActualizacion
                    FROM ordenes
                    WHERE @UsuarioId IS NULL
                       OR usuario_id = @UsuarioId
                    ",
                    new { UsuarioId = usuario });

            List<Order> ordenes = new List<Order>();

            foreach (OrderRow fila in filas)
            {
                Order orden = MapearOrden(fila);

                await CargarItemsAsync(connection, orden);

                ordenes.Add(orden);
            }

            return ordenes;
        }

        public async Task<Order> GetByIdAsync(Guid id)
        {
            using SqliteConnection connection = CreateConnection();

            connection.Open();

            OrderRow fila =
                await connection.QuerySingleOrDefaultAsync<OrderRow>(
                    @"
                    SELECT
                        id AS Id,
                        usuario_id AS UsuarioId,
                        total AS Total,
                        estado AS Estado,
                        fecha_creacion AS FechaCreacion,
                        fecha_actualizacion AS FechaActualizacion
                    FROM ordenes
                    WHERE id = @Id
                    ",
                    new { Id = id.ToString() });

            if (fila == null)
            {
                return null;
            }

            Order orden = MapearOrden(fila);

            await CargarItemsAsync(connection, orden);

            return orden;
        }

        public async Task CreateAsync(Order orden)
        {
            using SqliteConnection connection = CreateConnection();

            connection.Open();

            using SqliteTransaction transaction =
                connection.BeginTransaction();

            await connection.ExecuteAsync(
                @"
                INSERT INTO ordenes (
                    id,
                    usuario_id,
                    total,
                    estado,
                    fecha_creacion,
                    fecha_actualizacion
                )
                VALUES (
                    @Id,
                    @UsuarioId,
                    @Total,
                    @Estado,
                    @FechaCreacion,
                    @FechaActualizacion
                )
                ",
                new
                {
                    Id = orden.Id.ToString(),
                    UsuarioId = orden.UsuarioId.ToString(),
                    Total = orden.Total.ToString(
                        CultureInfo.InvariantCulture),
                    Estado = orden.Estado,
                    FechaCreacion = orden.FechaCreacion.ToString(
                        "O",
                        CultureInfo.InvariantCulture),
                    FechaActualizacion = (string)null
                },
                transaction);

            foreach (OrderItem item in orden.Items)
            {
                await connection.ExecuteAsync(
                    @"
                    INSERT INTO orden_items (
                        orden_id,
                        producto_id,
                        cantidad,
                        precio_unitario
                    )
                    VALUES (
                        @OrdenId,
                        @ProductoId,
                        @Cantidad,
                        @PrecioUnitario
                    )
                    ",
                    new
                    {
                        OrdenId = orden.Id.ToString(),
                        ProductoId = item.ProductoId.ToString(),
                        Cantidad = item.Cantidad,
                        PrecioUnitario =
                            item.PrecioUnitario.ToString(
                                CultureInfo.InvariantCulture)
                    },
                    transaction);
            }

            transaction.Commit();
        }

        public async Task<bool> UpdateStatusAsync(
            Guid id,
            string estadoAnterior,
            string estadoNuevo,
            DateTime fechaActualizacion)
        {
            using SqliteConnection connection = CreateConnection();

            int modificadas =
                await connection.ExecuteAsync(
                    @"
                    UPDATE ordenes
                    SET
                        estado = @EstadoNuevo,
                        fecha_actualizacion = @Fecha
                    WHERE id = @Id
                      AND estado = @EstadoAnterior
                    ",
                    new
                    {
                        Id = id.ToString(),
                        EstadoAnterior = estadoAnterior,
                        EstadoNuevo = estadoNuevo,
                        Fecha = fechaActualizacion.ToString(
                            "O",
                            CultureInfo.InvariantCulture)
                    });

            return modificadas > 0;
        }

        private async Task CargarItemsAsync(
            SqliteConnection connection,
            Order orden)
        {
            IEnumerable<OrderItemRow> filas =
                await connection.QueryAsync<OrderItemRow>(
                    @"
                    SELECT
                        producto_id AS ProductoId,
                        cantidad AS Cantidad,
                        precio_unitario AS PrecioUnitario
                    FROM orden_items
                    WHERE orden_id = @OrdenId
                    ",
                    new { OrdenId = orden.Id.ToString() });

            orden.Items = new List<OrderItem>();

            foreach (OrderItemRow fila in filas)
            {
                OrderItem item = new OrderItem();

                item.ProductoId = Guid.Parse(fila.ProductoId);
                item.Cantidad = fila.Cantidad;
                item.PrecioUnitario = decimal.Parse(
                    fila.PrecioUnitario,
                    CultureInfo.InvariantCulture);

                orden.Items.Add(item);
            }
        }

        private Order MapearOrden(OrderRow fila)
        {
            Order orden = new Order();

            orden.Id = Guid.Parse(fila.Id);
            orden.UsuarioId = Guid.Parse(fila.UsuarioId);
            orden.Total = decimal.Parse(
                fila.Total,
                CultureInfo.InvariantCulture);
            orden.Estado = fila.Estado;
            orden.FechaCreacion = DateTime.Parse(
                fila.FechaCreacion,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

            if (!string.IsNullOrWhiteSpace(
                fila.FechaActualizacion))
            {
                orden.FechaActualizacion = DateTime.Parse(
                    fila.FechaActualizacion,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);
            }

            return orden;
        }
    }

    public class OrderRow
    {
        public string Id { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string Total { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string FechaCreacion { get; set; } = string.Empty;
        public string FechaActualizacion { get; set; }
    }

    public class OrderItemRow
    {
        public string ProductoId { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string PrecioUnitario { get; set; } = string.Empty;
    }
}
