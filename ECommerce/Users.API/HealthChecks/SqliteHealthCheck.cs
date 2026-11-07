
using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Users.API.HealthChecks
{
    public class SqliteHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _config;

        public SqliteHealthCheck(IConfiguration config)
        {
            _config = config;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string connectionString =
                    _config.GetConnectionString("DefaultConnection")
                    ?? "Data Source=users.db";

                using SqliteConnection connection =
                    new SqliteConnection(connectionString);

                await connection.OpenAsync(cancellationToken);

                int resultado =
                    await connection.ExecuteScalarAsync<int>(
                        "SELECT 1");

                if (resultado == 1)
                {
                    return HealthCheckResult.Healthy(
                        "SQLite funciona correctamente.");
                }

                return HealthCheckResult.Unhealthy(
                    "SQLite no respondió correctamente.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "No se pudo conectar con SQLite.",
                    ex);
            }
        }
    }
}
