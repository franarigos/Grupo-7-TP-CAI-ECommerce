using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Products.API.HealthChecks
{
    /// <summary>
    /// Comprueba el acceso a la base de datos de productos.
    /// </summary>
    public class SqliteHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa la comprobación de SQLite.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public SqliteHealthCheck(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Verifica que se pueda consultar la tabla de productos.
        /// </summary>
        /// <param name="context">Contexto de la comprobación.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Estado de la base de datos.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string connectionString =
                    _config.GetConnectionString("DefaultConnection")
                    ?? "Data Source=products.db";

                using SqliteConnection connection =
                    new SqliteConnection(connectionString);

                await connection.OpenAsync(cancellationToken);

                await connection.ExecuteScalarAsync<long>(
                    "SELECT COUNT(*) FROM productos;");

                return HealthCheckResult.Healthy(
                    "La base de datos de productos está disponible.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "No se pudo consultar la base de datos de productos.",
                    ex);
            }
        }
    }
}