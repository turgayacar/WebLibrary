using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using WebLibrary.Models;

namespace WebLibrary.Database
{
    /// <summary>
    /// Veritabanı işlemleri için yardımcı sınıf
    /// </summary>
    public static class DatabaseHelper
    {
        /// <summary>
        /// Bağlantı string'ini doğrular
        /// </summary>
        /// <param name="connectionString">Bağlantı string'i</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> ValidateConnectionString(string connectionString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                    return ServiceResult<bool>.Error("Bağlantı string'i boş olamaz");

                using var connection = new SqlConnection(connectionString);
                connection.Open();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Bağlantı doğrulama hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Veritabanı bağlantısını test eder
        /// </summary>
        /// <param name="connectionString">Bağlantı string'i</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> TestConnection(string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                
                // Basit bir sorgu çalıştır
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.CommandType = CommandType.Text;
                
                var result = command.ExecuteScalar();
                return ServiceResult<bool>.Success(result != null);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Bağlantı test hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Veritabanı bilgilerini alır
        /// </summary>
        /// <param name="connectionString">Bağlantı string'i</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<DatabaseInfo> GetDatabaseInfo(string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var info = new DatabaseInfo
                {
                    ServerName = connection.DataSource,
                    DatabaseName = connection.Database,
                    ConnectionTimeout = connection.ConnectionTimeout,
                    ServerVersion = connection.ServerVersion
                };

                // Tablo sayısını al
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                info.TableCount = Convert.ToInt32(command.ExecuteScalar());

                // Kullanıcı sayısını al
                command.CommandText = "SELECT COUNT(*) FROM sys.database_principals WHERE type = 'S'";
                info.UserCount = Convert.ToInt32(command.ExecuteScalar());

                return ServiceResult<DatabaseInfo>.Success(info);
            }
            catch (Exception ex)
            {
                return ServiceResult<DatabaseInfo>.Error($"Veritabanı bilgisi alma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Tablo listesini alır
        /// </summary>
        /// <param name="connectionString">Bağlantı string'i</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<TableInfo>> GetTables(string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var tables = new List<TableInfo>();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        t.TABLE_NAME,
                        t.TABLE_SCHEMA,
                        p.rows as RowCount,
                        CAST(ROUND((SUM(a.total_pages) * 8) / 1024.00, 2) AS NUMERIC(36, 2)) AS SizeMB
                    FROM INFORMATION_SCHEMA.TABLES t
                    LEFT JOIN sys.indexes i ON t.TABLE_NAME = OBJECT_NAME(i.object_id)
                    LEFT JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
                    LEFT JOIN sys.allocation_units a ON p.partition_id = a.container_id
                    WHERE t.TABLE_TYPE = 'BASE TABLE'
                    GROUP BY t.TABLE_NAME, t.TABLE_SCHEMA, p.rows
                    ORDER BY t.TABLE_NAME";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    tables.Add(new TableInfo
                    {
                        Name = reader["TABLE_NAME"].ToString()!,
                        Schema = reader["TABLE_SCHEMA"].ToString()!,
                        RowCount = Convert.ToInt64(reader["RowCount"]),
                        SizeMB = Convert.ToDecimal(reader["SizeMB"])
                    });
                }

                return ServiceResult<List<TableInfo>>.Success(tables);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<TableInfo>>.Error($"Tablo listesi alma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Tablo yapısını alır
        /// </summary>
        /// <param name="connectionString">Bağlantı string'i</param>
        /// <param name="tableName">Tablo adı</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<ColumnInfo>> GetTableStructure(string connectionString, string tableName)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var columns = new List<ColumnInfo>();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        c.COLUMN_NAME,
                        c.DATA_TYPE,
                        c.IS_NULLABLE,
                        c.COLUMN_DEFAULT,
                        c.CHARACTER_MAXIMUM_LENGTH,
                        c.NUMERIC_PRECISION,
                        c.NUMERIC_SCALE,
                        CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey,
                        CASE WHEN fk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IsForeignKey
                    FROM INFORMATION_SCHEMA.COLUMNS c
                    LEFT JOIN (
                        SELECT ku.COLUMN_NAME
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
                        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
                            ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY' 
                            AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                        WHERE ku.TABLE_NAME = @TableName
                    ) pk ON c.COLUMN_NAME = pk.COLUMN_NAME
                    LEFT JOIN (
                        SELECT ku.COLUMN_NAME
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
                        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
                            ON tc.CONSTRAINT_TYPE = 'FOREIGN KEY' 
                            AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                        WHERE ku.TABLE_NAME = @TableName
                    ) fk ON c.COLUMN_NAME = fk.COLUMN_NAME
                    WHERE c.TABLE_NAME = @TableName
                    ORDER BY c.ORDINAL_POSITION";

                command.Parameters.AddWithValue("@TableName", tableName);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    columns.Add(new ColumnInfo
                    {
                        Name = reader["COLUMN_NAME"].ToString()!,
                        DataType = reader["DATA_TYPE"].ToString()!,
                        IsNullable = reader["IS_NULLABLE"].ToString() == "YES",
                        DefaultValue = reader["COLUMN_DEFAULT"]?.ToString(),
                        MaxLength = reader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? Convert.ToInt32(reader["CHARACTER_MAXIMUM_LENGTH"]) : null,
                        Precision = reader["NUMERIC_PRECISION"] != DBNull.Value ? Convert.ToInt32(reader["NUMERIC_PRECISION"]) : null,
                        Scale = reader["NUMERIC_SCALE"] != DBNull.Value ? Convert.ToInt32(reader["NUMERIC_SCALE"]) : null,
                        IsPrimaryKey = Convert.ToBoolean(reader["IsPrimaryKey"]),
                        IsForeignKey = Convert.ToBoolean(reader["IsForeignKey"])
                    });
                }

                return ServiceResult<List<ColumnInfo>>.Success(columns);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ColumnInfo>>.Error($"Tablo yapısı alma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// SQL sorgusunu çalıştırır ve sonuçları döner
        /// </summary>
        /// <param name="connectionString">Bağlantı string'i</param>
        /// <param name="sql">SQL sorgusu</param>
        /// <param name="parameters">Parametreler</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<Dictionary<string, object>>> ExecuteQuery(string connectionString, string sql, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                var results = new List<Dictionary<string, object>>();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    results.Add(row);
                }

                return ServiceResult<List<Dictionary<string, object>>>.Success(results);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Dictionary<string, object>>>.Error($"Sorgu çalıştırma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// SQL komutunu çalıştırır
        /// </summary>
        /// <param name="connectionString">Bağlantı string'i</param>
        /// <param name="sql">SQL komutu</param>
        /// <param name="parameters">Parametreler</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<int> ExecuteCommand(string connectionString, string sql, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                var result = command.ExecuteNonQuery();
                return ServiceResult<int>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.Error($"Komut çalıştırma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Backup alır
        /// </summary>
        /// <param name="connectionString">Bağlantı string'i</param>
        /// <param name="backupPath">Backup dosya yolu</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> CreateBackup(string connectionString, string backupPath)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var databaseName = connection.Database;
                var sql = $"BACKUP DATABASE [{databaseName}] TO DISK = '{backupPath}' WITH FORMAT, INIT, NAME = N'{databaseName}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 3600; // 1 saat

                command.ExecuteNonQuery();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Backup alma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Backup'tan geri yükler
        /// </summary>
        /// <param name="connectionString">Bağlantı string'i</param>
        /// <param name="backupPath">Backup dosya yolu</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> RestoreFromBackup(string connectionString, string backupPath)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var databaseName = connection.Database;
                var sql = $"RESTORE DATABASE [{databaseName}] FROM DISK = '{backupPath}' WITH REPLACE";

                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 3600; // 1 saat

                command.ExecuteNonQuery();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Backup'tan geri yükleme hatası: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Veritabanı bilgisi
    /// </summary>
    public class DatabaseInfo
    {
        public string ServerName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public int ConnectionTimeout { get; set; }
        public string ServerVersion { get; set; } = string.Empty;
        public long TableCount { get; set; }
        public long UserCount { get; set; }
    }

    /// <summary>
    /// Tablo bilgisi
    /// </summary>
    public class TableInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Schema { get; set; } = string.Empty;
        public long RowCount { get; set; }
        public decimal SizeMB { get; set; }
    }

    /// <summary>
    /// Sütun bilgisi
    /// </summary>
    public class ColumnInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public string? DefaultValue { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
    }
}
