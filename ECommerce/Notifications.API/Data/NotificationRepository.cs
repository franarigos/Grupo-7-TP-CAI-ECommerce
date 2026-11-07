
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;

namespace Notifications.API.Data
{
    public class NotificationRepository
    {
        private readonly IConfiguration _config;

        public NotificationRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqliteConnection CreateConnection()
        {
            string connectionString =
                _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=notifications.db";

            return new SqliteConnection(connectionString);
        }

        public async Task CreateAsync(Notification notification)
        {
            using SqliteConnection connection =
                CreateConnection();

            await connection.ExecuteAsync(
                @"
                INSERT INTO notificaciones
                (
                    id,
                    usuario_id,
                    mensaje,
                    tipo,
                    estado,
                    fecha_envio
                )
                VALUES
                (
                    @Id,
                    @UsuarioId,
                    @Mensaje,
                    @Tipo,
                    @Estado,
                    @FechaEnvio
                )
                ",
                new
                {
                    Id = notification.Id.ToString(),
                    UsuarioId = notification.UsuarioId.ToString(),
                    Mensaje = notification.Mensaje,
                    Tipo = notification.Tipo,
                    Estado = notification.Estado,
                    FechaEnvio = notification.FechaEnvio.ToString(
                        "O",
                        CultureInfo.InvariantCulture)
                });
        }

        public async Task<List<Notification>> GetByUserAsync(
            Guid userId)
        {
            using SqliteConnection connection =
                CreateConnection();

            IEnumerable<NotificationRow> filas =
                await connection.QueryAsync<NotificationRow>(
                    @"
                    SELECT
                        id AS Id,
                        usuario_id AS UsuarioId,
                        mensaje AS Mensaje,
                        tipo AS Tipo,
                        estado AS Estado,
                        fecha_envio AS FechaEnvio
                    FROM notificaciones
                    WHERE usuario_id = @UsuarioId
                    ORDER BY fecha_envio DESC
                    ",
                    new
                    {
                        UsuarioId = userId.ToString()
                    });

            List<Notification> resultado =
                new List<Notification>();

            foreach (NotificationRow fila in filas)
            {
                Notification notification =
                    new Notification();

                notification.Id = Guid.Parse(fila.Id);
                notification.UsuarioId =
                    Guid.Parse(fila.UsuarioId);
                notification.Mensaje = fila.Mensaje;
                notification.Tipo = fila.Tipo;
                notification.Estado = fila.Estado;
                notification.FechaEnvio = DateTime.Parse(
                    fila.FechaEnvio,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);

                resultado.Add(notification);
            }

            return resultado;
        }
    }

    public class NotificationRow
    {
        public string Id { get; set; } = string.Empty;

        public string UsuarioId { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string FechaEnvio { get; set; } = string.Empty;
    }
}
