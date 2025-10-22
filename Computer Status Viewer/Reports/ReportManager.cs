using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Computer_Status_Viewer.Database;
using Computer_Status_Viewer.Models;

namespace Computer_Status_Viewer.Reports
{
    /// <summary>
    /// Менеджер для создания и управления отчётами
    /// </summary>
    public class ReportManager
    {
        private readonly DatabaseManager _databaseManager;
        private readonly string _reportsDirectory;

        public ReportManager()
        {
            _databaseManager = new DatabaseManager();
            _reportsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
            
            // Создаём папку для отчётов, если её нет
            if (!Directory.Exists(_reportsDirectory))
            {
                Directory.CreateDirectory(_reportsDirectory);
            }
        }

        /// <summary>
        /// Создание автоматического отчёта о системе
        /// </summary>
        public int CreateSystemReport()
        {
            try
            {
                var report = new Report
                {
                    Title = $"Отчёт о системе - {DateTime.Now:dd.MM.yyyy HH:mm}",
                    Description = "Автоматический отчёт о состоянии системы",
                    CreatedDate = DateTime.Now,
                    ReportTypeId = 1, // Системная информация
                    IsAutomatic = true,
                    Status = "В процессе"
                };

                int reportId = _databaseManager.CreateReport(report);

                // Собираем данные о системе
                CollectSystemData(reportId);

                // Генерируем файл отчёта
                string filePath = GenerateReportFile(reportId, "system");
                
                // Обновляем статус отчёта
                _databaseManager.UpdateReportStatus(reportId, "Завершён", filePath);

                return reportId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания системного отчёта: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Создание отчёта о производительности
        /// </summary>
        public int CreatePerformanceReport()
        {
            try
            {
                var report = new Report
                {
                    Title = $"Отчёт о производительности - {DateTime.Now:dd.MM.yyyy HH:mm}",
                    Description = "Автоматический отчёт о производительности системы",
                    CreatedDate = DateTime.Now,
                    ReportTypeId = 2, // Производительность
                    IsAutomatic = true,
                    Status = "В процессе"
                };

                int reportId = _databaseManager.CreateReport(report);

                // Собираем данные о производительности
                CollectPerformanceData(reportId);

                // Генерируем файл отчёта
                string filePath = GenerateReportFile(reportId, "performance");
                
                // Обновляем статус отчёта
                _databaseManager.UpdateReportStatus(reportId, "Завершён", filePath);

                return reportId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания отчёта о производительности: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Создание быстрого отчёта
        /// </summary>
        public int CreateQuickReport()
        {
            try
            {
                var report = new Report
                {
                    Title = $"Быстрый отчёт - {DateTime.Now:dd.MM.yyyy HH:mm}",
                    Description = "Краткая сводка состояния системы",
                    CreatedDate = DateTime.Now,
                    ReportTypeId = 1, // Системная информация
                    IsAutomatic = false,
                    Status = "В процессе"
                };

                int reportId = _databaseManager.CreateReport(report);

                // Собираем только основную информацию
                CollectQuickData(reportId);

                // Генерируем файл отчёта
                string filePath = GenerateReportFile(reportId, "quick");
                
                // Обновляем статус отчёта
                _databaseManager.UpdateReportStatus(reportId, "Завершён", filePath);

                return reportId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания быстрого отчёта: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Создание подробного отчёта
        /// </summary>
        public int CreateDetailedReport()
        {
            try
            {
                var report = new Report
                {
                    Title = $"Подробный отчёт - {DateTime.Now:dd.MM.yyyy HH:mm}",
                    Description = "Полная диагностика системы с детальной информацией",
                    CreatedDate = DateTime.Now,
                    ReportTypeId = 2, // Производительность
                    IsAutomatic = false,
                    Status = "В процессе"
                };

                int reportId = _databaseManager.CreateReport(report);

                // Собираем полную информацию о системе
                CollectSystemData(reportId);
                CollectPerformanceData(reportId);
                CollectDetailedHardwareData(reportId);

                // Генерируем файл отчёта
                string filePath = GenerateReportFile(reportId, "detailed");
                
                // Обновляем статус отчёта
                _databaseManager.UpdateReportStatus(reportId, "Завершён", filePath);

                return reportId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания подробного отчёта: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Создание пользовательского отчёта с критериями
        /// </summary>
        public int CreateCustomReportWithCriteria(string title, string description, List<string> criteria)
        {
            try
            {
                var report = new Report
                {
                    Title = title,
                    Description = description,
                    CreatedDate = DateTime.Now,
                    ReportTypeId = 4, // Пользовательский отчёт
                    IsAutomatic = false,
                    Status = "В процессе"
                };

                int reportId = _databaseManager.CreateReport(report);

                // Собираем данные согласно выбранным критериям
                CollectCustomData(reportId, criteria);

                // Генерируем файл отчёта
                string filePath = GenerateReportFile(reportId, "custom");
                
                // Обновляем статус отчёта
                _databaseManager.UpdateReportStatus(reportId, "Завершён", filePath);

                return reportId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания пользовательского отчёта: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Создание пользовательского отчёта (старый метод для совместимости)
        /// </summary>
        public int CreateCustomReport(string title, string description, Dictionary<string, string> customData)
        {
            try
            {
                var report = new Report
                {
                    Title = title,
                    Description = description,
                    CreatedDate = DateTime.Now,
                    ReportTypeId = 4, // Пользовательский отчёт
                    IsAutomatic = false,
                    Status = "В процессе"
                };

                int reportId = _databaseManager.CreateReport(report);

                // Добавляем пользовательские данные
                foreach (var data in customData)
                {
                    _databaseManager.AddReportData(reportId, data.Key, data.Value, "string", "Пользовательские данные");
                }

                // Генерируем файл отчёта
                string filePath = GenerateReportFile(reportId, "custom");
                
                // Обновляем статус отчёта
                _databaseManager.UpdateReportStatus(reportId, "Завершён", filePath);

                return reportId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания пользовательского отчёта: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Сбор данных о системе
        /// </summary>
        private void CollectSystemData(int reportId)
        {
            try
            {
                // Информация о процессоре
                var cpuInfo = System.Environment.ProcessorCount;
                _databaseManager.AddReportData(reportId, "Количество процессоров", cpuInfo.ToString(), "int", "Процессор");
                _databaseManager.AddReportData(reportId, "Архитектура процессора", System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"), "string", "Процессор");

                // Информация об операционной системе
                _databaseManager.AddReportData(reportId, "Операционная система", System.Environment.OSVersion.ToString(), "string", "ОС");
                _databaseManager.AddReportData(reportId, "Версия .NET", System.Environment.Version.ToString(), "string", "ОС");
                _databaseManager.AddReportData(reportId, "Платформа", System.Environment.OSVersion.Platform.ToString(), "string", "ОС");

                // Информация о памяти
                var workingSet = GC.GetTotalMemory(false);
                _databaseManager.AddReportData(reportId, "Используемая память (байт)", workingSet.ToString(), "long", "Память");
                _databaseManager.AddReportData(reportId, "Используемая память (МБ)", (workingSet / 1024 / 1024).ToString(), "double", "Память");

                // Информация о пользователе
                _databaseManager.AddReportData(reportId, "Имя пользователя", System.Environment.UserName, "string", "Пользователь");
                _databaseManager.AddReportData(reportId, "Имя компьютера", System.Environment.MachineName, "string", "Пользователь");
                _databaseManager.AddReportData(reportId, "Домен", System.Environment.UserDomainName, "string", "Пользователь");

                // Информация о дисках
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    if (drive.IsReady)
                    {
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name}", 
                            $"Свободно: {drive.AvailableFreeSpace / 1024 / 1024 / 1024} ГБ, Всего: {drive.TotalSize / 1024 / 1024 / 1024} ГБ", 
                            "string", "Диски");
                    }
                }

                // Время работы системы
                var uptime = DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime;
                _databaseManager.AddReportData(reportId, "Время работы приложения", uptime.ToString(@"dd\.hh\:mm\:ss"), "string", "Время");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора данных", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор данных о производительности
        /// </summary>
        private void CollectPerformanceData(int reportId)
        {
            try
            {
                // Загрузка процессора (приблизительная)
                var cpuUsage = GetCpuUsage();
                _databaseManager.AddReportData(reportId, "Загрузка процессора (%)", cpuUsage.ToString("F2"), "double", "Производительность");

                // Использование памяти
                var memoryUsage = GetMemoryUsage();
                _databaseManager.AddReportData(reportId, "Использование памяти (%)", memoryUsage.ToString("F2"), "double", "Производительность");

                // Количество процессов
                var processCount = System.Diagnostics.Process.GetProcesses().Length;
                _databaseManager.AddReportData(reportId, "Количество процессов", processCount.ToString(), "int", "Производительность");

                // Время отклика системы
                var responseTime = DateTime.Now;
                _databaseManager.AddReportData(reportId, "Время отклика", responseTime.ToString("HH:mm:ss.fff"), "datetime", "Производительность");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора данных производительности", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Получение загрузки процессора
        /// </summary>
        private double GetCpuUsage()
        {
            try
            {
                using (var process = System.Diagnostics.Process.GetCurrentProcess())
                {
                    var startTime = DateTime.UtcNow;
                    var startCpuUsage = process.TotalProcessorTime;
                    
                    System.Threading.Thread.Sleep(100);
                    
                    var endTime = DateTime.UtcNow;
                    var endCpuUsage = process.TotalProcessorTime;
                    
                    var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                    var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                    var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
                    
                    return cpuUsageTotal * 100;
                }
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Получение использования памяти
        /// </summary>
        private double GetMemoryUsage()
        {
            try
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var workingSet = process.WorkingSet64;
                var totalMemory = GC.GetTotalMemory(false);
                
                return (double)workingSet / totalMemory * 100;
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Генерация файла отчёта
        /// </summary>
        private string GenerateReportFile(int reportId, string reportType)
        {
            try
            {
                var report = _databaseManager.GetReportById(reportId);
                var reportData = _databaseManager.GetReportData(reportId);
                
                string fileName = $"{reportType}_report_{reportId}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string filePath = Path.Combine(_reportsDirectory, fileName);
                
                var reportContent = new StringBuilder();
                reportContent.AppendLine($"=== {report.Title} ===");
                reportContent.AppendLine($"Описание: {report.Description}");
                reportContent.AppendLine($"Дата создания: {report.CreatedDate:dd.MM.yyyy HH:mm:ss}");
                reportContent.AppendLine($"Тип отчёта: {report.ReportType?.Name}");
                reportContent.AppendLine($"Статус: {report.Status}");
                reportContent.AppendLine();
                
                // Группируем данные по категориям
                var groupedData = new Dictionary<string, List<ReportData>>();
                foreach (var data in reportData)
                {
                    if (!groupedData.ContainsKey(data.Category ?? "Без категории"))
                    {
                        groupedData[data.Category ?? "Без категории"] = new List<ReportData>();
                    }
                    groupedData[data.Category ?? "Без категории"].Add(data);
                }
                
                foreach (var category in groupedData)
                {
                    reportContent.AppendLine($"--- {category.Key} ---");
                    foreach (var data in category.Value)
                    {
                        reportContent.AppendLine($"{data.Key}: {data.Value}");
                    }
                    reportContent.AppendLine();
                }
                
                File.WriteAllText(filePath, reportContent.ToString(), Encoding.UTF8);
                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка генерации файла отчёта: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Получение всех отчётов
        /// </summary>
        public List<Report> GetAllReports()
        {
            return _databaseManager.GetAllReports();
        }

        /// <summary>
        /// Получение отчёта по ID
        /// </summary>
        public Report GetReportById(int id)
        {
            return _databaseManager.GetReportById(id);
        }

        /// <summary>
        /// Удаление отчёта
        /// </summary>
        public void DeleteReport(int reportId)
        {
            var report = _databaseManager.GetReportById(reportId);
            if (report != null && !string.IsNullOrEmpty(report.FilePath) && File.Exists(report.FilePath))
            {
                File.Delete(report.FilePath);
            }
            _databaseManager.DeleteReport(reportId);
        }

        /// <summary>
        /// Получение всех типов отчётов
        /// </summary>
        public List<ReportType> GetAllReportTypes()
        {
            return _databaseManager.GetAllReportTypes();
        }

        /// <summary>
        /// Получение данных отчёта
        /// </summary>
        public List<ReportData> GetReportData(int reportId)
        {
            return _databaseManager.GetReportData(reportId);
        }

        /// <summary>
        /// Сбор быстрых данных (основная информация)
        /// </summary>
        private void CollectQuickData(int reportId)
        {
            try
            {
                // Основная информация о системе
                _databaseManager.AddReportData(reportId, "Операционная система", System.Environment.OSVersion.ToString(), "string", "ОС");
                _databaseManager.AddReportData(reportId, "Имя компьютера", System.Environment.MachineName, "string", "Система");
                _databaseManager.AddReportData(reportId, "Имя пользователя", System.Environment.UserName, "string", "Система");
                _databaseManager.AddReportData(reportId, "Количество процессоров", System.Environment.ProcessorCount.ToString(), "int", "Процессор");
                
                // Память
                var workingSet = GC.GetTotalMemory(false);
                _databaseManager.AddReportData(reportId, "Используемая память (МБ)", (workingSet / 1024 / 1024).ToString(), "double", "Память");
                
                // Диски (только основные)
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives.Take(3)) // Только первые 3 диска
                {
                    if (drive.IsReady)
                    {
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name}", 
                            $"Свободно: {drive.AvailableFreeSpace / 1024 / 1024 / 1024} ГБ", 
                            "string", "Диски");
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора быстрых данных", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор детальной аппаратной информации
        /// </summary>
        private void CollectDetailedHardwareData(int reportId)
        {
            try
            {
                // Детальная информация о процессоре
                _databaseManager.AddReportData(reportId, "Архитектура процессора", System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"), "string", "Процессор");
                _databaseManager.AddReportData(reportId, "Идентификатор процессора", System.Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"), "string", "Процессор");
                
                // Детальная информация о памяти
                var process = System.Diagnostics.Process.GetCurrentProcess();
                _databaseManager.AddReportData(reportId, "Рабочий набор процесса (МБ)", (process.WorkingSet64 / 1024 / 1024).ToString(), "double", "Память");
                _databaseManager.AddReportData(reportId, "Пиковый рабочий набор (МБ)", (process.PeakWorkingSet64 / 1024 / 1024).ToString(), "double", "Память");
                
                // Информация о .NET
                _databaseManager.AddReportData(reportId, "Версия .NET", System.Environment.Version.ToString(), "string", "ОС");
                _databaseManager.AddReportData(reportId, "Платформа .NET", System.Environment.OSVersion.Platform.ToString(), "string", "ОС");
                
                // Системные переменные
                _databaseManager.AddReportData(reportId, "Системная папка", System.Environment.SystemDirectory, "string", "Система");
                _databaseManager.AddReportData(reportId, "Папка пользователя", System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "string", "Система");
                
                // Время работы
                var uptime = DateTime.Now - process.StartTime;
                _databaseManager.AddReportData(reportId, "Время работы приложения", uptime.ToString(@"dd\.hh\:mm\:ss"), "string", "Время");
                
                // Детальная информация о дисках
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    if (drive.IsReady)
                    {
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Тип", drive.DriveType.ToString(), "string", "Диски");
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Формат", drive.DriveFormat, "string", "Диски");
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Общий размер", (drive.TotalSize / 1024 / 1024 / 1024).ToString() + " ГБ", "string", "Диски");
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Свободно", (drive.AvailableFreeSpace / 1024 / 1024 / 1024).ToString() + " ГБ", "string", "Диски");
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора детальной аппаратной информации", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор пользовательских данных согласно критериям
        /// </summary>
        private void CollectCustomData(int reportId, List<string> criteria)
        {
            try
            {
                foreach (var criterion in criteria)
                {
                    switch (criterion)
                    {
                        case "OS_INFO":
                            _databaseManager.AddReportData(reportId, "Операционная система", System.Environment.OSVersion.ToString(), "string", "ОС");
                            _databaseManager.AddReportData(reportId, "Версия .NET", System.Environment.Version.ToString(), "string", "ОС");
                            _databaseManager.AddReportData(reportId, "Платформа", System.Environment.OSVersion.Platform.ToString(), "string", "ОС");
                            break;
                            
                        case "COMPUTER_INFO":
                            _databaseManager.AddReportData(reportId, "Имя компьютера", System.Environment.MachineName, "string", "Система");
                            _databaseManager.AddReportData(reportId, "Домен", System.Environment.UserDomainName, "string", "Система");
                            _databaseManager.AddReportData(reportId, "Системная папка", System.Environment.SystemDirectory, "string", "Система");
                            break;
                            
                        case "USER_INFO":
                            _databaseManager.AddReportData(reportId, "Имя пользователя", System.Environment.UserName, "string", "Пользователь");
                            _databaseManager.AddReportData(reportId, "Папка пользователя", System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "string", "Пользователь");
                            break;
                            
                        case "DATETIME_INFO":
                            _databaseManager.AddReportData(reportId, "Дата создания", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), "datetime", "Время");
                            _databaseManager.AddReportData(reportId, "Время UTC", DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm:ss"), "datetime", "Время");
                            break;
                            
                        case "CPU_INFO":
                            _databaseManager.AddReportData(reportId, "Количество процессоров", System.Environment.ProcessorCount.ToString(), "int", "Процессор");
                            _databaseManager.AddReportData(reportId, "Архитектура процессора", System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"), "string", "Процессор");
                            _databaseManager.AddReportData(reportId, "Идентификатор процессора", System.Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"), "string", "Процессор");
                            break;
                            
                        case "MEMORY_INFO":
                            var workingSet = GC.GetTotalMemory(false);
                            _databaseManager.AddReportData(reportId, "Используемая память (МБ)", (workingSet / 1024 / 1024).ToString(), "double", "Память");
                            var process = System.Diagnostics.Process.GetCurrentProcess();
                            _databaseManager.AddReportData(reportId, "Рабочий набор процесса (МБ)", (process.WorkingSet64 / 1024 / 1024).ToString(), "double", "Память");
                            break;
                            
                        case "DISK_INFO":
                            var drives = DriveInfo.GetDrives();
                            foreach (var drive in drives)
                            {
                                if (drive.IsReady)
                                {
                                    _databaseManager.AddReportData(reportId, $"Диск {drive.Name}", 
                                        $"Свободно: {drive.AvailableFreeSpace / 1024 / 1024 / 1024} ГБ, Всего: {drive.TotalSize / 1024 / 1024 / 1024} ГБ", 
                                        "string", "Диски");
                                }
                            }
                            break;
                            
                        case "GPU_INFO":
                            // Заглушка для информации о видеокарте
                            _databaseManager.AddReportData(reportId, "Видеокарта", "Информация о видеокарте недоступна", "string", "Видеокарта");
                            break;
                            
                        case "MOTHERBOARD_INFO":
                            // Заглушка для информации о материнской плате
                            _databaseManager.AddReportData(reportId, "Материнская плата", "Информация о материнской плате недоступна", "string", "Материнская плата");
                            break;
                            
                        case "PERFORMANCE_INFO":
                            var cpuUsage = GetCpuUsage();
                            var memoryUsage = GetMemoryUsage();
                            _databaseManager.AddReportData(reportId, "Загрузка процессора (%)", cpuUsage.ToString("F2"), "double", "Производительность");
                            _databaseManager.AddReportData(reportId, "Использование памяти (%)", memoryUsage.ToString("F2"), "double", "Производительность");
                            break;
                            
                        case "NETWORK_INFO":
                            _databaseManager.AddReportData(reportId, "Сетевая информация", "Информация о сети недоступна", "string", "Сеть");
                            break;
                            
                        case "PROCESS_INFO":
                            var processCount = System.Diagnostics.Process.GetProcesses().Length;
                            _databaseManager.AddReportData(reportId, "Количество процессов", processCount.ToString(), "int", "Процессы");
                            break;
                            
                        case "SCREENSHOTS":
                            _databaseManager.AddReportData(reportId, "Скриншоты", "Скриншоты недоступны в данной версии", "string", "Дополнительно");
                            break;
                            
                        case "LOGS":
                            _databaseManager.AddReportData(reportId, "Системные логи", "Логи недоступны в данной версии", "string", "Дополнительно");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора пользовательских данных", ex.Message, "string", "Ошибки");
            }
        }
    }
}
