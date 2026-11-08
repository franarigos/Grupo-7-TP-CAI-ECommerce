using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Orders.API.HealthChecks
{
    /// <summary>
    /// Comprueba el acceso a la base de órdenes.
    /// </summary>
    public class SqliteHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa la comprobación.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public SqliteHealthCheck(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Verifica que se puedan consultar las tablas de órdenes.
        /// </summary>
        /// <param name="context">Contexto de la comprobación.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Estado de disponibilidad de SQLite.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string connectionString =
                    _config.GetConnectionString("DefaultConnection")
                    ?? "Data Source=orders.db";

                using SqliteConnection connection =
                    new SqliteConnection(connectionString);

                await connection.OpenAsync(cancellationToken);

                await connection.ExecuteScalarAsync<long>(
                    "SELECT COUNT(*) FROM ordenes;");

                await connection.ExecuteScalarAsync<long>(
                    "SELECT COUNT(*) FROM orden_items;");

                return HealthCheckResult.Healthy(
                    "SQLite está disponible.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "No se pudieron consultar las tablas del servicio.",
                    ex);
            }
        }
    }
}