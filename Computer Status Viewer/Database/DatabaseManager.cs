using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Computer_Status_Viewer.Models;

namespace Computer_Status_Viewer.Database
{
    /// <summary>
    /// Менеджер для работы с базой данных отчётов
    /// </summary>
    public class DatabaseManager
    {
        private readonly string _connectionString;
        private readonly string _databasePath;

        /// <summary>
        /// Вспомогательный метод для получения int значения по имени столбца
        /// </summary>
        private int GetInt32(SQLiteDataReader reader, string columnName)
        {
            return reader.GetInt32(reader.GetOrdinal(columnName));
        }

        /// <summary>
        /// Вспомогательный метод для получения string значения по имени столбца
        /// </summary>
        private string GetString(SQLiteDataReader reader, string columnName)
        {
            return reader.GetString(reader.GetOrdinal(columnName));
        }

        /// <summary>
        /// Вспомогательный метод для получения DateTime значения по имени столбца
        /// </summary>
        private DateTime GetDateTime(SQLiteDataReader reader, string columnName)
        {
            return reader.GetDateTime(reader.GetOrdinal(columnName));
        }

        /// <summary>
        /// Вспомогательный метод для проверки NULL значения по имени столбца
        /// </summary>
        private bool IsDBNull(SQLiteDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName));
        }

        public DatabaseManager()
        {
            // Путь к базе данных в папке приложения
            _databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports.db");
            _connectionString = $"Data Source={_databasePath};Version=3;";
            
            // Создаём базу данных и таблицы при первом запуске
            InitializeDatabase();
        }

        /// <summary>
        /// Инициализация базы данных и создание таблиц
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    
                    // Создание таблицы типов отчётов
                    string createReportTypesTable = @"
                        CREATE TABLE IF NOT EXISTS ReportTypes (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Description TEXT,
                            Category TEXT NOT NULL,
                            IsAutomatic INTEGER NOT NULL DEFAULT 0,
                            Template TEXT
                        )";

                    // Создание таблицы отчётов
                    string createReportsTable = @"
                        CREATE TABLE IF NOT EXISTS Reports (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Title TEXT NOT NULL,
                            Description TEXT,
                            CreatedDate DATETIME NOT NULL,
                            ModifiedDate DATETIME,
                            ReportTypeId INTEGER NOT NULL,
                            IsAutomatic INTEGER NOT NULL DEFAULT 0,
                            FilePath TEXT,
                            Status TEXT NOT NULL DEFAULT 'Создан',
                            FOREIGN KEY (ReportTypeId) REFERENCES ReportTypes(Id)
                        )";

                    // Создание таблицы данных отчётов
                    string createReportDataTable = @"
                        CREATE TABLE IF NOT EXISTS ReportData (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ReportId INTEGER NOT NULL,
                            Key TEXT NOT NULL,
                            Value TEXT,
                            DataType TEXT NOT NULL DEFAULT 'string',
                            Category TEXT,
                            Timestamp DATETIME NOT NULL,
                            FOREIGN KEY (ReportId) REFERENCES Reports(Id)
                        )";

                    using (var command = new SQLiteCommand(createReportTypesTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SQLiteCommand(createReportsTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SQLiteCommand(createReportDataTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Вставка базовых типов отчётов
                    InsertDefaultReportTypes(connection);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка инициализации базы данных: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Вставка базовых типов отчётов
        /// </summary>
        private void InsertDefaultReportTypes(SQLiteConnection connection)
        {
            string checkTypesQuery = "SELECT COUNT(*) FROM ReportTypes";
            using (var command = new SQLiteCommand(checkTypesQuery, connection))
            {
                var count = Convert.ToInt32(command.ExecuteScalar());
                if (count > 0) return; // Типы уже существуют
            }

            var defaultTypes = new[]
            {
                new { Name = "Системная информация", Description = "Полная информация о системе", Category = "Система", IsAutomatic = true, Template = "system_info" },
                new { Name = "Производительность", Description = "Отчёт о производительности системы", Category = "Производительность", IsAutomatic = true, Template = "performance" },
                new { Name = "Безопасность", Description = "Проверка безопасности системы", Category = "Безопасность", IsAutomatic = true, Template = "security" },
                new { Name = "Пользовательский отчёт", Description = "Пользовательский отчёт", Category = "Пользовательский", IsAutomatic = false, Template = "custom" }
            };

            string insertQuery = @"
                INSERT INTO ReportTypes (Name, Description, Category, IsAutomatic, Template)
                VALUES (@Name, @Description, @Category, @IsAutomatic, @Template)";

            foreach (var type in defaultTypes)
            {
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", type.Name);
                    command.Parameters.AddWithValue("@Description", type.Description);
                    command.Parameters.AddWithValue("@Category", type.Category);
                    command.Parameters.AddWithValue("@IsAutomatic", type.IsAutomatic ? 1 : 0);
                    command.Parameters.AddWithValue("@Template", type.Template);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Создание нового отчёта
        /// </summary>
        public int CreateReport(Report report)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    INSERT INTO Reports (Title, Description, CreatedDate, ModifiedDate, ReportTypeId, IsAutomatic, FilePath, Status)
                    VALUES (@Title, @Description, @CreatedDate, @ModifiedDate, @ReportTypeId, @IsAutomatic, @FilePath, @Status);
                    SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", report.Title);
                    command.Parameters.AddWithValue("@Description", report.Description);
                    command.Parameters.AddWithValue("@CreatedDate", report.CreatedDate);
                    command.Parameters.AddWithValue("@ModifiedDate", report.ModifiedDate);
                    command.Parameters.AddWithValue("@ReportTypeId", report.ReportTypeId);
                    command.Parameters.AddWithValue("@IsAutomatic", report.IsAutomatic ? 1 : 0);
                    command.Parameters.AddWithValue("@FilePath", report.FilePath);
                    command.Parameters.AddWithValue("@Status", report.Status);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Получение всех отчётов
        /// </summary>
        public List<Report> GetAllReports()
        {
            var reports = new List<Report>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT r.Id, r.Title, r.Description, r.CreatedDate, r.ModifiedDate, 
                           r.ReportTypeId, r.IsAutomatic, r.FilePath, r.Status,
                           rt.Name as ReportTypeName, rt.Category
                    FROM Reports r
                    JOIN ReportTypes rt ON r.ReportTypeId = rt.Id
                    ORDER BY r.CreatedDate DESC";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reports.Add(new Report
                        {
                            Id = GetInt32(reader, "Id"),
                            Title = GetString(reader, "Title"),
                            Description = IsDBNull(reader, "Description") ? null : GetString(reader, "Description"),
                            CreatedDate = GetDateTime(reader, "CreatedDate"),
                            ModifiedDate = IsDBNull(reader, "ModifiedDate") ? null : (DateTime?)GetDateTime(reader, "ModifiedDate"),
                            ReportTypeId = GetInt32(reader, "ReportTypeId"),
                            IsAutomatic = GetInt32(reader, "IsAutomatic") == 1,
                            FilePath = IsDBNull(reader, "FilePath") ? null : GetString(reader, "FilePath"),
                            Status = GetString(reader, "Status"),
                            ReportType = new ReportType
                            {
                                Id = GetInt32(reader, "ReportTypeId"),
                                Name = GetString(reader, "ReportTypeName"),
                                Category = GetString(reader, "Category")
                            }
                        });
                    }
                }
            }
            return reports;
        }

        /// <summary>
        /// Получение отчёта по ID
        /// </summary>
        public Report GetReportById(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT r.Id, r.Title, r.Description, r.CreatedDate, r.ModifiedDate, 
                           r.ReportTypeId, r.IsAutomatic, r.FilePath, r.Status,
                           rt.Name as ReportTypeName, rt.Category
                    FROM Reports r
                    JOIN ReportTypes rt ON r.ReportTypeId = rt.Id
                    WHERE r.Id = @Id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Report
                            {
                                Id = GetInt32(reader, "Id"),
                                Title = GetString(reader, "Title"),
                                Description = IsDBNull(reader, "Description") ? null : GetString(reader, "Description"),
                                CreatedDate = GetDateTime(reader, "CreatedDate"),
                                ModifiedDate = IsDBNull(reader, "ModifiedDate") ? null : (DateTime?)GetDateTime(reader, "ModifiedDate"),
                                ReportTypeId = GetInt32(reader, "ReportTypeId"),
                                IsAutomatic = GetInt32(reader, "IsAutomatic") == 1,
                                FilePath = IsDBNull(reader, "FilePath") ? null : GetString(reader, "FilePath"),
                                Status = GetString(reader, "Status"),
                                ReportType = new ReportType
                                {
                                    Id = GetInt32(reader, "ReportTypeId"),
                                    Name = GetString(reader, "ReportTypeName"),
                                    Category = GetString(reader, "Category")
                                }
                            };
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Добавление данных к отчёту
        /// </summary>
        public void AddReportData(int reportId, string key, string value, string dataType = "string", string category = null)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    INSERT INTO ReportData (ReportId, Key, Value, DataType, Category, Timestamp)
                    VALUES (@ReportId, @Key, @Value, @DataType, @Category, @Timestamp)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ReportId", reportId);
                    command.Parameters.AddWithValue("@Key", key);
                    command.Parameters.AddWithValue("@Value", value);
                    command.Parameters.AddWithValue("@DataType", dataType);
                    command.Parameters.AddWithValue("@Category", category);
                    command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Получение данных отчёта
        /// </summary>
        public List<ReportData> GetReportData(int reportId)
        {
            var data = new List<ReportData>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT Id, ReportId, Key, Value, DataType, Category, Timestamp
                    FROM ReportData
                    WHERE ReportId = @ReportId
                    ORDER BY Timestamp DESC";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ReportId", reportId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new ReportData
                            {
                                Id = GetInt32(reader, "Id"),
                                ReportId = GetInt32(reader, "ReportId"),
                                Key = GetString(reader, "Key"),
                                Value = IsDBNull(reader, "Value") ? null : GetString(reader, "Value"),
                                DataType = GetString(reader, "DataType"),
                                Category = IsDBNull(reader, "Category") ? null : GetString(reader, "Category"),
                                Timestamp = GetDateTime(reader, "Timestamp")
                            });
                        }
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Обновление статуса отчёта
        /// </summary>
        public void UpdateReportStatus(int reportId, string status, string filePath = null)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    UPDATE Reports 
                    SET Status = @Status, ModifiedDate = @ModifiedDate" + 
                    (filePath != null ? ", FilePath = @FilePath" : "") + 
                    " WHERE Id = @Id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Status", status);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.Parameters.AddWithValue("@Id", reportId);
                    if (filePath != null)
                    {
                        command.Parameters.AddWithValue("@FilePath", filePath);
                    }
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Удаление отчёта
        /// </summary>
        public void DeleteReport(int reportId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                // Сначала удаляем данные отчёта
                string deleteDataQuery = "DELETE FROM ReportData WHERE ReportId = @ReportId";
                using (var command = new SQLiteCommand(deleteDataQuery, connection))
                {
                    command.Parameters.AddWithValue("@ReportId", reportId);
                    command.ExecuteNonQuery();
                }

                // Затем удаляем сам отчёт
                string deleteReportQuery = "DELETE FROM Reports WHERE Id = @Id";
                using (var command = new SQLiteCommand(deleteReportQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", reportId);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Получение всех типов отчётов
        /// </summary>
        public List<ReportType> GetAllReportTypes()
        {
            var types = new List<ReportType>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Name, Description, Category, IsAutomatic, Template FROM ReportTypes ORDER BY Category, Name";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        types.Add(new ReportType
                        {
                            Id = GetInt32(reader, "Id"),
                            Name = GetString(reader, "Name"),
                            Description = IsDBNull(reader, "Description") ? null : GetString(reader, "Description"),
                            Category = GetString(reader, "Category"),
                            IsAutomatic = GetInt32(reader, "IsAutomatic") == 1,
                            Template = IsDBNull(reader, "Template") ? null : GetString(reader, "Template")
                        });
                    }
                }
            }
            return types;
        }
    }
}
