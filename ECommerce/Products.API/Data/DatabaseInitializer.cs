
using Dapper;
using Microsoft.Data.Sqlite;

namespace Products.API.Data
{
    public class DatabaseInitializer
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(
            IConfiguration config,
            ILogger<DatabaseInitializer> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void Initialize()
        {
            string connectionString =
                _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=products.db";

            using SqliteConnection connection =
                new SqliteConnection(connectionString);

            connection.Open();

            connection.Execute("""
                CREATE TABLE IF NOT EXISTS productos (
                    id TEXT PRIMARY KEY,
                    nombre TEXT NOT NULL,
                    descripcion TEXT,
                    precio TEXT NOT NULL,
                    stock INTEGER NOT NULL,
                    categoria TEXT NOT NULL,
                    fecha_creacion TEXT NOT NULL
                );
                """);

            _logger.LogInformation(
                "Base de datos SQLite inicializada: {BaseDatos}",
                connectionString);
        }
    }
}
