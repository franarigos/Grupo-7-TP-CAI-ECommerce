
using Dapper;
using Microsoft.Data.Sqlite;

namespace Orders.API.Data
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
                ?? "Data Source=orders.db;Foreign Keys=True";

            using SqliteConnection connection =
                new SqliteConnection(connectionString);

            connection.Open();

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ordenes (
                    id TEXT PRIMARY KEY,
                    usuario_id TEXT NOT NULL,
                    total TEXT NOT NULL,
                    estado TEXT NOT NULL,
                    fecha_creacion TEXT NOT NULL,
                    fecha_actualizacion TEXT
                );

                CREATE TABLE IF NOT EXISTS orden_items (
                    orden_id TEXT NOT NULL,
                    producto_id TEXT NOT NULL,
                    cantidad INTEGER NOT NULL,
                    precio_unitario TEXT NOT NULL,
                    PRIMARY KEY (orden_id, producto_id),
                    FOREIGN KEY (orden_id)
                        REFERENCES ordenes(id)
                        ON DELETE CASCADE
                );
            ");

            _logger.LogInformation(
                "SQLite inicializado: {BaseDatos}",
                connectionString);
        }
    }
}
