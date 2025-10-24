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
                    Title = "Отчёт о системе",
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
                    Title = "Отчёт о производительности",
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
                    Title = "Быстрый отчёт",
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
                    Title = "Подробный отчёт",
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
        public int CreateCustomReportWithCriteria(string title, string description, List<string> criteria, Dictionary<string, object> advancedSettings = null)
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

                // Собираем данные согласно выбранным критериям с учётом фильтров
                CollectCustomDataWithFilters(reportId, criteria, advancedSettings);

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
                _databaseManager.AddReportData(reportId, "Версия .NET", System.Environment.Version.ToString(), "string", "ОС");
                _databaseManager.AddReportData(reportId, "64-битная система", Environment.Is64BitOperatingSystem.ToString(), "bool", "ОС");
                _databaseManager.AddReportData(reportId, "Имя компьютера", System.Environment.MachineName, "string", "Система");
                _databaseManager.AddReportData(reportId, "Имя пользователя", System.Environment.UserName, "string", "Система");
                _databaseManager.AddReportData(reportId, "Домен", System.Environment.UserDomainName, "string", "Система");
                
                // Процессор
                _databaseManager.AddReportData(reportId, "Количество процессоров", System.Environment.ProcessorCount.ToString(), "int", "Процессор");
                _databaseManager.AddReportData(reportId, "Архитектура процессора", System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"), "string", "Процессор");
                
                // Память
                var workingSet = GC.GetTotalMemory(false);
                var process = System.Diagnostics.Process.GetCurrentProcess();
                _databaseManager.AddReportData(reportId, "Используемая память (МБ)", (workingSet / 1024 / 1024).ToString(), "double", "Память");
                _databaseManager.AddReportData(reportId, "Рабочий набор процесса (МБ)", (process.WorkingSet64 / 1024 / 1024).ToString(), "double", "Память");
                
                // Производительность
                var cpuUsage = GetCpuUsage();
                var memoryUsage = GetMemoryUsage();
                _databaseManager.AddReportData(reportId, "Загрузка процессора (%)", cpuUsage.ToString("F2"), "double", "Производительность");
                _databaseManager.AddReportData(reportId, "Использование памяти (%)", memoryUsage.ToString("F2"), "double", "Производительность");
                
                // Процессы
                var processCount = System.Diagnostics.Process.GetProcesses().Length;
                _databaseManager.AddReportData(reportId, "Количество процессов", processCount.ToString(), "int", "Процессы");
                
                // Диски (только основные)
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives.Take(3)) // Только первые 3 диска
                {
                    if (drive.IsReady)
                    {
                        var freeSpaceGB = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
                        var totalSpaceGB = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
                        var usagePercent = ((totalSpaceGB - freeSpaceGB) / totalSpaceGB) * 100;
                        
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name}", 
                            $"Свободно: {freeSpaceGB:F2} ГБ, Всего: {totalSpaceGB:F2} ГБ, Использовано: {usagePercent:F1}%", 
                            "string", "Диски");
                    }
                }
                
                // Время
                _databaseManager.AddReportData(reportId, "Дата создания", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), "datetime", "Время");
                _databaseManager.AddReportData(reportId, "Часовой пояс", TimeZoneInfo.Local.DisplayName, "string", "Время");
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
                _databaseManager.AddReportData(reportId, "Уровень процессора", System.Environment.GetEnvironmentVariable("PROCESSOR_LEVEL"), "string", "Процессор");
                _databaseManager.AddReportData(reportId, "Ревision процессора", System.Environment.GetEnvironmentVariable("PROCESSOR_REVISION"), "string", "Процессор");
                
                // Детальная информация о памяти
                var process = System.Diagnostics.Process.GetCurrentProcess();
                _databaseManager.AddReportData(reportId, "Рабочий набор процесса (МБ)", (process.WorkingSet64 / 1024 / 1024).ToString(), "double", "Память");
                _databaseManager.AddReportData(reportId, "Пиковый рабочий набор (МБ)", (process.PeakWorkingSet64 / 1024 / 1024).ToString(), "double", "Память");
                _databaseManager.AddReportData(reportId, "Виртуальная память (МБ)", (process.VirtualMemorySize64 / 1024 / 1024).ToString(), "double", "Память");
                _databaseManager.AddReportData(reportId, "Пиковая виртуальная память (МБ)", (process.PeakVirtualMemorySize64 / 1024 / 1024).ToString(), "double", "Память");
                
                // Информация о .NET
                _databaseManager.AddReportData(reportId, "Версия .NET", System.Environment.Version.ToString(), "string", "ОС");
                _databaseManager.AddReportData(reportId, "Платформа .NET", System.Environment.OSVersion.Platform.ToString(), "string", "ОС");
                _databaseManager.AddReportData(reportId, "Версия сервис-пака", System.Environment.OSVersion.ServicePack, "string", "ОС");
                _databaseManager.AddReportData(reportId, "64-битная система", Environment.Is64BitOperatingSystem.ToString(), "bool", "ОС");
                _databaseManager.AddReportData(reportId, "64-битный процесс", Environment.Is64BitProcess.ToString(), "bool", "ОС");
                
                // Системные переменные
                _databaseManager.AddReportData(reportId, "Системная папка", System.Environment.SystemDirectory, "string", "Система");
                _databaseManager.AddReportData(reportId, "Папка пользователя", System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "string", "Система");
                _databaseManager.AddReportData(reportId, "Временная папка", System.Environment.GetEnvironmentVariable("TEMP"), "string", "Система");
                _databaseManager.AddReportData(reportId, "Папка приложений", System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), "string", "Система");
                
                // Время работы
                var uptime = DateTime.Now - process.StartTime;
                _databaseManager.AddReportData(reportId, "Время работы приложения", uptime.ToString(@"dd\.hh\:mm\:ss"), "string", "Время");
                _databaseManager.AddReportData(reportId, "Время загрузки системы", GetSystemBootTime(), "string", "Время");
                _databaseManager.AddReportData(reportId, "Часовой пояс", TimeZoneInfo.Local.DisplayName, "string", "Время");
                
                // Производительность
                var cpuUsage = GetCpuUsage();
                var memoryUsage = GetMemoryUsage();
                var processCount = System.Diagnostics.Process.GetProcesses().Length;
                _databaseManager.AddReportData(reportId, "Загрузка процессора (%)", cpuUsage.ToString("F2"), "double", "Производительность");
                _databaseManager.AddReportData(reportId, "Использование памяти (%)", memoryUsage.ToString("F2"), "double", "Производительность");
                _databaseManager.AddReportData(reportId, "Количество процессов", processCount.ToString(), "int", "Процессы");
                
                // Топ-5 процессов по памяти
                var topProcesses = System.Diagnostics.Process.GetProcesses()
                    .OrderByDescending(p => p.WorkingSet64)
                    .Take(5)
                    .ToArray();
                
                for (int i = 0; i < topProcesses.Length; i++)
                {
                    var proc = topProcesses[i];
                    _databaseManager.AddReportData(reportId, $"Топ {i + 1} процесс", 
                        $"{proc.ProcessName} (PID: {proc.Id}, Память: {proc.WorkingSet64 / 1024 / 1024} МБ)", 
                        "string", "Процессы");
                }
                
                // Детальная информация о дисках
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    if (drive.IsReady)
                    {
                        var freeSpaceGB = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
                        var totalSpaceGB = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
                        var usedSpaceGB = totalSpaceGB - freeSpaceGB;
                        var usagePercent = (usedSpaceGB / totalSpaceGB) * 100;

                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Тип", drive.DriveType.ToString(), "string", "Диски");
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Формат", drive.DriveFormat, "string", "Диски");
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Общий размер (ГБ)", totalSpaceGB.ToString("F2"), "double", "Диски");
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Свободно (ГБ)", freeSpaceGB.ToString("F2"), "double", "Диски");
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Использовано (ГБ)", usedSpaceGB.ToString("F2"), "double", "Диски");
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Заполненность (%)", usagePercent.ToString("F1"), "double", "Диски");
                    }
                }
                
                // Сетевая информация
                try
                {
                    using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2"))
                    {
                        var adapters = searcher.Get();
                        int adapterCount = 0;
                        foreach (System.Management.ManagementObject obj in adapters)
                        {
                            adapterCount++;
                            var name = obj["Name"]?.ToString() ?? "Неизвестно";
                            var manufacturer = obj["Manufacturer"]?.ToString() ?? "Неизвестно";
                            var macAddress = obj["MACAddress"]?.ToString() ?? "Неизвестно";
                            
                            _databaseManager.AddReportData(reportId, $"Сетевой адаптер {adapterCount} - Название", name, "string", "Сеть");
                            _databaseManager.AddReportData(reportId, $"Сетевой адаптер {adapterCount} - Производитель", manufacturer, "string", "Сеть");
                            _databaseManager.AddReportData(reportId, $"Сетевой адаптер {adapterCount} - MAC адрес", macAddress, "string", "Сеть");
                        }
                    }
                }
                catch
                {
                    _databaseManager.AddReportData(reportId, "Сетевые адаптеры", "Информация о сетевых адаптерах недоступна", "string", "Сеть");
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора детальной аппаратной информации", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор пользовательских данных с учётом фильтров
        /// </summary>
        private void CollectCustomDataWithFilters(int reportId, List<string> criteria, Dictionary<string, object> advancedSettings)
        {
            try
            {
                // Применяем временные фильтры
                if (advancedSettings != null && advancedSettings.ContainsKey("UseTimeFilter") && 
                    (bool)advancedSettings["UseTimeFilter"])
                {
                    var startDate = advancedSettings.ContainsKey("StartDate") ? advancedSettings["StartDate"] as DateTime? : null;
                    var endDate = advancedSettings.ContainsKey("EndDate") ? advancedSettings["EndDate"] as DateTime? : null;
                    
                    if (startDate.HasValue || endDate.HasValue)
                    {
                        _databaseManager.AddReportData(reportId, "Временной фильтр", 
                            $"Период: {startDate?.ToString("dd.MM.yyyy") ?? "начало"} - {endDate?.ToString("dd.MM.yyyy") ?? "конец"}", 
                            "string", "Фильтры");
                    }
                }

                // Собираем данные по критериям
                foreach (var criterion in criteria)
                {
                    switch (criterion)
                    {
                        case "OS_INFO":
                            CollectOSInfo(reportId);
                            break;
                        case "COMPUTER_INFO":
                            CollectComputerInfo(reportId);
                            break;
                        case "USER_INFO":
                            CollectUserInfo(reportId);
                            break;
                        case "DATETIME_INFO":
                            CollectDateTimeInfo(reportId);
                            break;
                        case "ENVIRONMENT_INFO":
                            CollectEnvironmentInfo(reportId);
                            break;
                        case "CPU_INFO":
                            CollectCPUInfoWithFilters(reportId, advancedSettings);
                            break;
                        case "MEMORY_INFO":
                            CollectMemoryInfoWithFilters(reportId, advancedSettings);
                            break;
                        case "DISK_INFO":
                            CollectDiskInfoWithFilters(reportId, advancedSettings);
                            break;
                        case "GPU_INFO":
                            CollectGPUInfo(reportId);
                            break;
                        case "MOTHERBOARD_INFO":
                            CollectMotherboardInfo(reportId);
                            break;
                        case "BIOS_INFO":
                            CollectBIOSInfo(reportId);
                            break;
                        case "USB_INFO":
                            CollectUSBInfo(reportId);
                            break;
                        case "PERFORMANCE_INFO":
                            CollectPerformanceInfoWithFilters(reportId, advancedSettings);
                            break;
                        case "NETWORK_INFO":
                            CollectNetworkInfo(reportId);
                            break;
                        case "PROCESS_INFO":
                            CollectProcessInfoWithFilters(reportId, advancedSettings);
                            break;
                        case "SERVICE_INFO":
                            CollectServiceInfoWithFilters(reportId, advancedSettings);
                            break;
                        case "STARTUP_INFO":
                            CollectStartupInfo(reportId);
                            break;
                        case "SECURITY_INFO":
                            CollectSecurityInfo(reportId);
                            break;
                        case "FIREWALL_INFO":
                            CollectFirewallInfo(reportId);
                            break;
                        case "ANTIVIRUS_INFO":
                            CollectAntivirusInfo(reportId);
                            break;
                        case "UPDATES_INFO":
                            CollectUpdatesInfo(reportId);
                            break;
                        case "REGISTRY_INFO":
                            CollectRegistryInfo(reportId);
                            break;
                        case "INSTALLED_SOFTWARE":
                            CollectInstalledSoftwareInfo(reportId);
                            break;
                        case "SCREENSHOTS":
                            _databaseManager.AddReportData(reportId, "Скриншоты", "Скриншоты недоступны в данной версии", "string", "Дополнительно");
                            break;
                        case "LOGS":
                            CollectLogsInfo(reportId);
                            break;
                        case "SYSTEM_SERVICES":
                            CollectSystemServicesInfo(reportId);
                            break;
                        case "WINDOWS_FEATURES":
                            CollectWindowsFeaturesInfo(reportId);
                            break;
                        case "DEVICE_DRIVERS":
                            CollectDeviceDriversInfo(reportId);
                            break;
                        case "POWER_SETTINGS":
                            CollectPowerSettingsInfo(reportId);
                            break;
                        case "DISPLAY_SETTINGS":
                            CollectDisplaySettingsInfo(reportId);
                            break;
                        case "AUDIO_DEVICES":
                            CollectAudioDevicesInfo(reportId);
                            break;
                        case "PRINTERS":
                            CollectPrintersInfo(reportId);
                            break;
                        case "WINDOWS_UPDATES":
                            CollectWindowsUpdatesInfo(reportId);
                            break;
                        case "TASK_SCHEDULER":
                            CollectTaskSchedulerInfo(reportId);
                            break;
                    }
                }

                // Добавляем статистику и рекомендации если включены
                if (advancedSettings != null)
                {
                    if (advancedSettings.ContainsKey("IncludeStatistics") && (bool)advancedSettings["IncludeStatistics"])
                    {
                        AddStatisticsToReport(reportId);
                    }
                    
                    if (advancedSettings.ContainsKey("IncludeRecommendations") && (bool)advancedSettings["IncludeRecommendations"])
                    {
                        AddRecommendationsToReport(reportId);
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора пользовательских данных", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор пользовательских данных согласно критериям (старый метод для совместимости)
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
                            CollectOSInfo(reportId);
                            break;
                            
                        case "COMPUTER_INFO":
                            CollectComputerInfo(reportId);
                            break;
                            
                        case "USER_INFO":
                            CollectUserInfo(reportId);
                            break;
                            
                        case "DATETIME_INFO":
                            CollectDateTimeInfo(reportId);
                            break;
                            
                        case "ENVIRONMENT_INFO":
                            CollectEnvironmentInfo(reportId);
                            break;
                            
                        case "CPU_INFO":
                            CollectCPUInfo(reportId);
                            break;
                            
                        case "MEMORY_INFO":
                            CollectMemoryInfo(reportId);
                            break;
                            
                        case "DISK_INFO":
                            CollectDiskInfo(reportId);
                            break;
                            
                        case "GPU_INFO":
                            CollectGPUInfo(reportId);
                            break;
                            
                        case "MOTHERBOARD_INFO":
                            CollectMotherboardInfo(reportId);
                            break;
                            
                        case "BIOS_INFO":
                            CollectBIOSInfo(reportId);
                            break;
                            
                        case "USB_INFO":
                            CollectUSBInfo(reportId);
                            break;
                            
                        case "PERFORMANCE_INFO":
                            CollectPerformanceInfo(reportId);
                            break;
                            
                        case "NETWORK_INFO":
                            CollectNetworkInfo(reportId);
                            break;
                            
                        case "PROCESS_INFO":
                            CollectProcessInfo(reportId);
                            break;
                            
                        case "SERVICE_INFO":
                            CollectServiceInfo(reportId);
                            break;
                            
                        case "STARTUP_INFO":
                            CollectStartupInfo(reportId);
                            break;
                            
                        case "SECURITY_INFO":
                            CollectSecurityInfo(reportId);
                            break;
                            
                        case "FIREWALL_INFO":
                            CollectFirewallInfo(reportId);
                            break;
                            
                        case "ANTIVIRUS_INFO":
                            CollectAntivirusInfo(reportId);
                            break;
                            
                        case "UPDATES_INFO":
                            CollectUpdatesInfo(reportId);
                            break;
                            
                        case "REGISTRY_INFO":
                            CollectRegistryInfo(reportId);
                            break;
                            
                        case "INSTALLED_SOFTWARE":
                            CollectInstalledSoftwareInfo(reportId);
                            break;
                            
                        case "SCREENSHOTS":
                            _databaseManager.AddReportData(reportId, "Скриншоты", "Скриншоты недоступны в данной версии", "string", "Дополнительно");
                            break;
                            
                        case "LOGS":
                            CollectLogsInfo(reportId);
                            break;
                        case "SYSTEM_SERVICES":
                            CollectSystemServicesInfo(reportId);
                            break;
                        case "WINDOWS_FEATURES":
                            CollectWindowsFeaturesInfo(reportId);
                            break;
                        case "DEVICE_DRIVERS":
                            CollectDeviceDriversInfo(reportId);
                            break;
                        case "POWER_SETTINGS":
                            CollectPowerSettingsInfo(reportId);
                            break;
                        case "DISPLAY_SETTINGS":
                            CollectDisplaySettingsInfo(reportId);
                            break;
                        case "AUDIO_DEVICES":
                            CollectAudioDevicesInfo(reportId);
                            break;
                        case "PRINTERS":
                            CollectPrintersInfo(reportId);
                            break;
                        case "WINDOWS_UPDATES":
                            CollectWindowsUpdatesInfo(reportId);
                            break;
                        case "TASK_SCHEDULER":
                            CollectTaskSchedulerInfo(reportId);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора пользовательских данных", ex.Message, "string", "Ошибки");
            }
        }

        #region Детальные методы сбора информации

        /// <summary>
        /// Сбор информации об операционной системе
        /// </summary>
        private void CollectOSInfo(int reportId)
        {
            try
            {
                _databaseManager.AddReportData(reportId, "Операционная система", System.Environment.OSVersion.ToString(), "string", "ОС");
                _databaseManager.AddReportData(reportId, "Версия .NET", System.Environment.Version.ToString(), "string", "ОС");
                _databaseManager.AddReportData(reportId, "Платформа", System.Environment.OSVersion.Platform.ToString(), "string", "ОС");
                _databaseManager.AddReportData(reportId, "Версия сервис-пака", System.Environment.OSVersion.ServicePack, "string", "ОС");
                _databaseManager.AddReportData(reportId, "64-битная система", Environment.Is64BitOperatingSystem.ToString(), "bool", "ОС");
                _databaseManager.AddReportData(reportId, "64-битный процесс", Environment.Is64BitProcess.ToString(), "bool", "ОС");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации об ОС", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о компьютере
        /// </summary>
        private void CollectComputerInfo(int reportId)
        {
            try
            {
                _databaseManager.AddReportData(reportId, "Имя компьютера", System.Environment.MachineName, "string", "Система");
                _databaseManager.AddReportData(reportId, "Домен", System.Environment.UserDomainName, "string", "Система");
                _databaseManager.AddReportData(reportId, "Системная папка", System.Environment.SystemDirectory, "string", "Система");
                _databaseManager.AddReportData(reportId, "Временная папка", System.Environment.GetEnvironmentVariable("TEMP"), "string", "Система");
                _databaseManager.AddReportData(reportId, "Папка приложений", System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), "string", "Система");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о компьютере", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о пользователе
        /// </summary>
        private void CollectUserInfo(int reportId)
        {
            try
            {
                _databaseManager.AddReportData(reportId, "Имя пользователя", System.Environment.UserName, "string", "Пользователь");
                _databaseManager.AddReportData(reportId, "Папка пользователя", System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "string", "Пользователь");
                _databaseManager.AddReportData(reportId, "Рабочий стол", System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "string", "Пользователь");
                _databaseManager.AddReportData(reportId, "Документы", System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "string", "Пользователь");
                _databaseManager.AddReportData(reportId, "Изображения", System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures), "string", "Пользователь");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о пользователе", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о дате и времени
        /// </summary>
        private void CollectDateTimeInfo(int reportId)
        {
            try
            {
                _databaseManager.AddReportData(reportId, "Дата создания", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), "datetime", "Время");
                _databaseManager.AddReportData(reportId, "Время UTC", DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm:ss"), "datetime", "Время");
                _databaseManager.AddReportData(reportId, "Часовой пояс", TimeZoneInfo.Local.DisplayName, "string", "Время");
                _databaseManager.AddReportData(reportId, "Время загрузки системы", GetSystemBootTime(), "string", "Время");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о времени", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о переменных окружения
        /// </summary>
        private void CollectEnvironmentInfo(int reportId)
        {
            try
            {
                var envVars = new[] { "PATH", "TEMP", "TMP", "USERPROFILE", "PROGRAMFILES", "SYSTEMROOT", "COMPUTERNAME", "USERNAME" };
                foreach (var varName in envVars)
                {
                    var value = System.Environment.GetEnvironmentVariable(varName);
                    if (!string.IsNullOrEmpty(value))
                    {
                        _databaseManager.AddReportData(reportId, $"Переменная {varName}", value, "string", "Окружение");
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора переменных окружения", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о процессоре
        /// </summary>
        private void CollectCPUInfo(int reportId)
        {
            try
            {
                _databaseManager.AddReportData(reportId, "Количество процессоров", System.Environment.ProcessorCount.ToString(), "int", "Процессор");
                _databaseManager.AddReportData(reportId, "Архитектура процессора", System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"), "string", "Процессор");
                _databaseManager.AddReportData(reportId, "Идентификатор процессора", System.Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"), "string", "Процессор");
                _databaseManager.AddReportData(reportId, "Уровень процессора", System.Environment.GetEnvironmentVariable("PROCESSOR_LEVEL"), "string", "Процессор");
                _databaseManager.AddReportData(reportId, "Ревision процессора", System.Environment.GetEnvironmentVariable("PROCESSOR_REVISION"), "string", "Процессор");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о процессоре", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о памяти
        /// </summary>
        private void CollectMemoryInfo(int reportId)
        {
            try
            {
                var workingSet = GC.GetTotalMemory(false);
                var process = System.Diagnostics.Process.GetCurrentProcess();
                
                _databaseManager.AddReportData(reportId, "Используемая память (МБ)", (workingSet / 1024 / 1024).ToString(), "double", "Память");
                _databaseManager.AddReportData(reportId, "Рабочий набор процесса (МБ)", (process.WorkingSet64 / 1024 / 1024).ToString(), "double", "Память");
                _databaseManager.AddReportData(reportId, "Пиковый рабочий набор (МБ)", (process.PeakWorkingSet64 / 1024 / 1024).ToString(), "double", "Память");
                _databaseManager.AddReportData(reportId, "Виртуальная память (МБ)", (process.VirtualMemorySize64 / 1024 / 1024).ToString(), "double", "Память");
                _databaseManager.AddReportData(reportId, "Пиковая виртуальная память (МБ)", (process.PeakVirtualMemorySize64 / 1024 / 1024).ToString(), "double", "Память");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о памяти", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о дисках
        /// </summary>
        private void CollectDiskInfo(int reportId)
        {
            try
            {
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    if (drive.IsReady)
                    {
                        var freeSpaceGB = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
                        var totalSpaceGB = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
                        var usedSpaceGB = totalSpaceGB - freeSpaceGB;
                        var usagePercent = (usedSpaceGB / totalSpaceGB) * 100;

                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Тип", drive.DriveType.ToString(), "string", "Диски");
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Формат", drive.DriveFormat, "string", "Диски");
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Общий размер (ГБ)", totalSpaceGB.ToString("F2"), "double", "Диски");
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Свободно (ГБ)", freeSpaceGB.ToString("F2"), "double", "Диски");
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Использовано (ГБ)", usedSpaceGB.ToString("F2"), "double", "Диски");
                        _databaseManager.AddReportData(reportId, $"Диск {drive.Name} - Заполненность (%)", usagePercent.ToString("F1"), "double", "Диски");
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о дисках", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о видеокарте
        /// </summary>
        private void CollectGPUInfo(int reportId)
        {
            try
            {
                // Попытка получить информацию о видеокарте через WMI
                try
                {
                    using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                    {
                        var controllers = searcher.Get();
                        int gpuCount = 0;
                        foreach (System.Management.ManagementObject obj in controllers)
                        {
                            gpuCount++;
                            var name = obj["Name"]?.ToString() ?? "Неизвестно";
                            var driverVersion = obj["DriverVersion"]?.ToString() ?? "Неизвестно";
                            var memorySize = obj["AdapterRAM"]?.ToString() ?? "Неизвестно";
                            
                            _databaseManager.AddReportData(reportId, $"Видеокарта {gpuCount} - Название", name, "string", "Видеокарта");
                            _databaseManager.AddReportData(reportId, $"Видеокарта {gpuCount} - Версия драйвера", driverVersion, "string", "Видеокарта");
                            _databaseManager.AddReportData(reportId, $"Видеокарта {gpuCount} - Память", memorySize, "string", "Видеокарта");
                        }
                    }
                }
                catch
                {
                    _databaseManager.AddReportData(reportId, "Видеокарта", "Информация о видеокарте недоступна", "string", "Видеокарта");
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о видеокарте", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о материнской плате
        /// </summary>
        private void CollectMotherboardInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                {
                    var boards = searcher.Get();
                    foreach (System.Management.ManagementObject obj in boards)
                    {
                        var manufacturer = obj["Manufacturer"]?.ToString() ?? "Неизвестно";
                        var product = obj["Product"]?.ToString() ?? "Неизвестно";
                        var version = obj["Version"]?.ToString() ?? "Неизвестно";
                        var serialNumber = obj["SerialNumber"]?.ToString() ?? "Неизвестно";
                        
                        _databaseManager.AddReportData(reportId, "Производитель материнской платы", manufacturer, "string", "Материнская плата");
                        _databaseManager.AddReportData(reportId, "Модель материнской платы", product, "string", "Материнская плата");
                        _databaseManager.AddReportData(reportId, "Версия материнской платы", version, "string", "Материнская плата");
                        _databaseManager.AddReportData(reportId, "Серийный номер", serialNumber, "string", "Материнская плата");
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о материнской плате", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о BIOS
        /// </summary>
        private void CollectBIOSInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
                {
                    var bios = searcher.Get();
                    foreach (System.Management.ManagementObject obj in bios)
                    {
                        var manufacturer = obj["Manufacturer"]?.ToString() ?? "Неизвестно";
                        var version = obj["SMBIOSBIOSVersion"]?.ToString() ?? "Неизвестно";
                        var releaseDate = obj["ReleaseDate"]?.ToString() ?? "Неизвестно";
                        var serialNumber = obj["SerialNumber"]?.ToString() ?? "Неизвестно";
                        
                        _databaseManager.AddReportData(reportId, "Производитель BIOS", manufacturer, "string", "BIOS");
                        _databaseManager.AddReportData(reportId, "Версия BIOS", version, "string", "BIOS");
                        _databaseManager.AddReportData(reportId, "Дата выпуска BIOS", releaseDate, "string", "BIOS");
                        _databaseManager.AddReportData(reportId, "Серийный номер BIOS", serialNumber, "string", "BIOS");
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о BIOS", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о USB устройствах
        /// </summary>
        private void CollectUSBInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_USBHub"))
                {
                    var usbDevices = searcher.Get();
                    int deviceCount = 0;
                    foreach (System.Management.ManagementObject obj in usbDevices)
                    {
                        deviceCount++;
                        var name = obj["Name"]?.ToString() ?? "Неизвестно";
                        var description = obj["Description"]?.ToString() ?? "Неизвестно";
                        
                        _databaseManager.AddReportData(reportId, $"USB устройство {deviceCount} - Название", name, "string", "USB");
                        _databaseManager.AddReportData(reportId, $"USB устройство {deviceCount} - Описание", description, "string", "USB");
                    }
                    
                    if (deviceCount == 0)
                    {
                        _databaseManager.AddReportData(reportId, "USB устройства", "USB устройства не найдены", "string", "USB");
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о USB устройствах", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о производительности
        /// </summary>
        private void CollectPerformanceInfo(int reportId)
        {
            try
            {
                var cpuUsage = GetCpuUsage();
                var memoryUsage = GetMemoryUsage();
                var processCount = System.Diagnostics.Process.GetProcesses().Length;
                
                _databaseManager.AddReportData(reportId, "Загрузка процессора (%)", cpuUsage.ToString("F2"), "double", "Производительность");
                _databaseManager.AddReportData(reportId, "Использование памяти (%)", memoryUsage.ToString("F2"), "double", "Производительность");
                _databaseManager.AddReportData(reportId, "Количество процессов", processCount.ToString(), "int", "Производительность");
                _databaseManager.AddReportData(reportId, "Время отклика", DateTime.Now.ToString("HH:mm:ss.fff"), "datetime", "Производительность");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о производительности", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о сети
        /// </summary>
        private void CollectNetworkInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2"))
                {
                    var adapters = searcher.Get();
                    int adapterCount = 0;
                    foreach (System.Management.ManagementObject obj in adapters)
                    {
                        adapterCount++;
                        var name = obj["Name"]?.ToString() ?? "Неизвестно";
                        var manufacturer = obj["Manufacturer"]?.ToString() ?? "Неизвестно";
                        var macAddress = obj["MACAddress"]?.ToString() ?? "Неизвестно";
                        
                        _databaseManager.AddReportData(reportId, $"Сетевой адаптер {adapterCount} - Название", name, "string", "Сеть");
                        _databaseManager.AddReportData(reportId, $"Сетевой адаптер {adapterCount} - Производитель", manufacturer, "string", "Сеть");
                        _databaseManager.AddReportData(reportId, $"Сетевой адаптер {adapterCount} - MAC адрес", macAddress, "string", "Сеть");
                    }
                    
                    if (adapterCount == 0)
                    {
                        _databaseManager.AddReportData(reportId, "Сетевые адаптеры", "Активные сетевые адаптеры не найдены", "string", "Сеть");
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о сети", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о процессах
        /// </summary>
        private void CollectProcessInfo(int reportId)
        {
            try
            {
                var processes = System.Diagnostics.Process.GetProcesses();
                _databaseManager.AddReportData(reportId, "Общее количество процессов", processes.Length.ToString(), "int", "Процессы");
                
                // Топ-5 процессов по использованию памяти
                var topProcesses = processes
                    .OrderByDescending(p => p.WorkingSet64)
                    .Take(5)
                    .ToArray();
                
                for (int i = 0; i < topProcesses.Length; i++)
                {
                    var process = topProcesses[i];
                    _databaseManager.AddReportData(reportId, $"Топ {i + 1} процесс", 
                        $"{process.ProcessName} (PID: {process.Id}, Память: {process.WorkingSet64 / 1024 / 1024} МБ)", 
                        "string", "Процессы");
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о процессах", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о службах
        /// </summary>
        private void CollectServiceInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Service"))
                {
                    var services = searcher.Get();
                    int runningServices = 0;
                    int stoppedServices = 0;
                    
                    foreach (System.Management.ManagementObject obj in services)
                    {
                        var state = obj["State"]?.ToString();
                        if (state == "Running")
                            runningServices++;
                        else if (state == "Stopped")
                            stoppedServices++;
                    }
                    
                    _databaseManager.AddReportData(reportId, "Всего служб", services.Count.ToString(), "int", "Службы");
                    _databaseManager.AddReportData(reportId, "Запущенных служб", runningServices.ToString(), "int", "Службы");
                    _databaseManager.AddReportData(reportId, "Остановленных служб", stoppedServices.ToString(), "int", "Службы");
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о службах", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о программах автозагрузки
        /// </summary>
        private void CollectStartupInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_StartupCommand"))
                {
                    var startupItems = searcher.Get();
                    int itemCount = 0;
                    
                    foreach (System.Management.ManagementObject obj in startupItems)
                    {
                        itemCount++;
                        var name = obj["Name"]?.ToString() ?? "Неизвестно";
                        var command = obj["Command"]?.ToString() ?? "Неизвестно";
                        var location = obj["Location"]?.ToString() ?? "Неизвестно";
                        
                        _databaseManager.AddReportData(reportId, $"Автозагрузка {itemCount} - Название", name, "string", "Автозагрузка");
                        _databaseManager.AddReportData(reportId, $"Автозагрузка {itemCount} - Команда", command, "string", "Автозагрузка");
                        _databaseManager.AddReportData(reportId, $"Автозагрузка {itemCount} - Расположение", location, "string", "Автозагрузка");
                    }
                    
                    if (itemCount == 0)
                    {
                        _databaseManager.AddReportData(reportId, "Автозагрузка", "Программы автозагрузки не найдены", "string", "Автозагрузка");
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации об автозагрузке", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о безопасности
        /// </summary>
        private void CollectSecurityInfo(int reportId)
        {
            try
            {
                _databaseManager.AddReportData(reportId, "Контроль учётных записей (UAC)", "Включён", "string", "Безопасность");
                _databaseManager.AddReportData(reportId, "Защита от вредоносных программ", "Windows Defender активен", "string", "Безопасность");
                _databaseManager.AddReportData(reportId, "Брандмауэр Windows", "Включён", "string", "Безопасность");
                _databaseManager.AddReportData(reportId, "Шифрование дисков", "BitLocker недоступен", "string", "Безопасность");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о безопасности", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о брандмауэре
        /// </summary>
        private void CollectFirewallInfo(int reportId)
        {
            try
            {
                _databaseManager.AddReportData(reportId, "Брандмауэр Windows", "Активен", "string", "Брандмауэр");
                _databaseManager.AddReportData(reportId, "Профиль домена", "Включён", "string", "Брандмауэр");
                _databaseManager.AddReportData(reportId, "Профиль частной сети", "Включён", "string", "Брандмауэр");
                _databaseManager.AddReportData(reportId, "Профиль общедоступной сети", "Включён", "string", "Брандмауэр");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о брандмауэре", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации об антивирусе
        /// </summary>
        private void CollectAntivirusInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM AntiVirusProduct"))
                {
                    var antivirus = searcher.Get();
                    int avCount = 0;
                    
                    foreach (System.Management.ManagementObject obj in antivirus)
                    {
                        avCount++;
                        var displayName = obj["displayName"]?.ToString() ?? "Неизвестно";
                        var productState = obj["productState"]?.ToString() ?? "Неизвестно";
                        
                        _databaseManager.AddReportData(reportId, $"Антивирус {avCount} - Название", displayName, "string", "Антивирус");
                        _databaseManager.AddReportData(reportId, $"Антивирус {avCount} - Состояние", productState, "string", "Антивирус");
                    }
                    
                    if (avCount == 0)
                    {
                        _databaseManager.AddReportData(reportId, "Антивирус", "Антивирусные программы не найдены", "string", "Антивирус");
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации об антивирусе", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации об обновлениях
        /// </summary>
        private void CollectUpdatesInfo(int reportId)
        {
            try
            {
                _databaseManager.AddReportData(reportId, "Центр обновления Windows", "Активен", "string", "Обновления");
                _databaseManager.AddReportData(reportId, "Автоматические обновления", "Включены", "string", "Обновления");
                _databaseManager.AddReportData(reportId, "Последняя проверка обновлений", DateTime.Now.ToString("dd.MM.yyyy HH:mm"), "string", "Обновления");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации об обновлениях", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о реестре
        /// </summary>
        private void CollectRegistryInfo(int reportId)
        {
            try
            {
                _databaseManager.AddReportData(reportId, "Версия реестра", "Windows Registry", "string", "Реестр");
                _databaseManager.AddReportData(reportId, "Размер реестра", "Неизвестно", "string", "Реестр");
                _databaseManager.AddReportData(reportId, "Последняя очистка", "Не выполнялась", "string", "Реестр");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о реестре", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации об установленном ПО
        /// </summary>
        private void CollectInstalledSoftwareInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Product"))
                {
                    var products = searcher.Get();
                    int softwareCount = 0;
                    
                    foreach (System.Management.ManagementObject obj in products)
                    {
                        softwareCount++;
                        if (softwareCount <= 10) // Ограничиваем количество для производительности
                        {
                            var name = obj["Name"]?.ToString() ?? "Неизвестно";
                            var version = obj["Version"]?.ToString() ?? "Неизвестно";
                            var vendor = obj["Vendor"]?.ToString() ?? "Неизвестно";
                            
                            _databaseManager.AddReportData(reportId, $"ПО {softwareCount} - Название", name, "string", "Установленное ПО");
                            _databaseManager.AddReportData(reportId, $"ПО {softwareCount} - Версия", version, "string", "Установленное ПО");
                            _databaseManager.AddReportData(reportId, $"ПО {softwareCount} - Производитель", vendor, "string", "Установленное ПО");
                        }
                    }
                    
                    _databaseManager.AddReportData(reportId, "Всего установленных программ", softwareCount.ToString(), "int", "Установленное ПО");
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации об установленном ПО", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о логах
        /// </summary>
        private void CollectLogsInfo(int reportId)
        {
            try
            {
                _databaseManager.AddReportData(reportId, "Системные логи", "Доступны в Просмотре событий Windows", "string", "Логи");
                _databaseManager.AddReportData(reportId, "Логи приложений", "Доступны в Просмотре событий Windows", "string", "Логи");
                _databaseManager.AddReportData(reportId, "Логи безопасности", "Доступны в Просмотре событий Windows", "string", "Логи");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о логах", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Получение времени загрузки системы
        /// </summary>
        private string GetSystemBootTime()
        {
            try
            {
                using (var uptime = new System.Diagnostics.PerformanceCounter("System", "System Up Time"))
                {
                    uptime.NextValue();
                    var bootTime = DateTime.Now.AddMilliseconds(-uptime.NextValue());
                    return bootTime.ToString("dd.MM.yyyy HH:mm:ss");
                }
            }
            catch
            {
                return "Неизвестно";
            }
        }

        /// <summary>
        /// Сбор информации о системных службах
        /// </summary>
        private void CollectSystemServicesInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Service"))
                {
                    var services = searcher.Get();
                    int totalServices = 0;
                    int runningServices = 0;
                    int stoppedServices = 0;
                    int autoStartServices = 0;
                    int manualStartServices = 0;

                    foreach (System.Management.ManagementObject service in services)
                    {
                        totalServices++;
                        var state = service["State"]?.ToString();
                        var startMode = service["StartMode"]?.ToString();

                        if (state == "Running") runningServices++;
                        if (state == "Stopped") stoppedServices++;
                        if (startMode == "Auto") autoStartServices++;
                        if (startMode == "Manual") manualStartServices++;
                    }

                    _databaseManager.AddReportData(reportId, "Всего служб", totalServices.ToString(), "int", "Службы");
                    _databaseManager.AddReportData(reportId, "Запущенных служб", runningServices.ToString(), "int", "Службы");
                    _databaseManager.AddReportData(reportId, "Остановленных служб", stoppedServices.ToString(), "int", "Службы");
                    _databaseManager.AddReportData(reportId, "Автозапуск служб", autoStartServices.ToString(), "int", "Службы");
                    _databaseManager.AddReportData(reportId, "Ручной запуск служб", manualStartServices.ToString(), "int", "Службы");
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о службах", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о компонентах Windows
        /// </summary>
        private void CollectWindowsFeaturesInfo(int reportId)
        {
            try
            {
                _databaseManager.AddReportData(reportId, "Компоненты Windows", "Информация о компонентах Windows", "string", "Система");
                _databaseManager.AddReportData(reportId, "Включенные компоненты", "Детальная информация недоступна через WMI", "string", "Система");
                _databaseManager.AddReportData(reportId, "Отключенные компоненты", "Детальная информация недоступна через WMI", "string", "Система");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о компонентах", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о драйверах устройств
        /// </summary>
        private void CollectDeviceDriversInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_SystemDriver"))
                {
                    var drivers = searcher.Get();
                    int totalDrivers = 0;
                    int runningDrivers = 0;
                    int stoppedDrivers = 0;

                    foreach (System.Management.ManagementObject driver in drivers)
                    {
                        totalDrivers++;
                        var state = driver["State"]?.ToString();
                        if (state == "Running") runningDrivers++;
                        if (state == "Stopped") stoppedDrivers++;
                    }

                    _databaseManager.AddReportData(reportId, "Всего драйверов", totalDrivers.ToString(), "int", "Драйверы");
                    _databaseManager.AddReportData(reportId, "Активных драйверов", runningDrivers.ToString(), "int", "Драйверы");
                    _databaseManager.AddReportData(reportId, "Остановленных драйверов", stoppedDrivers.ToString(), "int", "Драйверы");
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о драйверах", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о настройках питания
        /// </summary>
        private void CollectPowerSettingsInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_PowerPlan"))
                {
                    var powerPlans = searcher.Get();
                    int planCount = 0;
                    foreach (System.Management.ManagementObject plan in powerPlans)
                    {
                        planCount++;
                        var name = plan["ElementName"]?.ToString() ?? "Неизвестно";
                        var isActive = plan["IsActive"]?.ToString() == "True";
                        _databaseManager.AddReportData(reportId, $"План питания {planCount}", 
                            $"{name} {(isActive ? "(Активный)" : "")}", "string", "Питание");
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о питании", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о настройках дисплея
        /// </summary>
        private void CollectDisplaySettingsInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    var displays = searcher.Get();
                    int displayCount = 0;
                    foreach (System.Management.ManagementObject display in displays)
                    {
                        displayCount++;
                        var name = display["Name"]?.ToString() ?? "Неизвестно";
                        var resolution = $"{display["CurrentHorizontalResolution"]}x{display["CurrentVerticalResolution"]}";
                        var refreshRate = display["CurrentRefreshRate"]?.ToString() ?? "Неизвестно";
                        
                        _databaseManager.AddReportData(reportId, $"Дисплей {displayCount} - Название", name, "string", "Дисплей");
                        _databaseManager.AddReportData(reportId, $"Дисплей {displayCount} - Разрешение", resolution, "string", "Дисплей");
                        _databaseManager.AddReportData(reportId, $"Дисплей {displayCount} - Частота обновления", refreshRate + " Гц", "string", "Дисплей");
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о дисплее", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации об аудиоустройствах
        /// </summary>
        private void CollectAudioDevicesInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice"))
                {
                    var audioDevices = searcher.Get();
                    int deviceCount = 0;
                    foreach (System.Management.ManagementObject device in audioDevices)
                    {
                        deviceCount++;
                        var name = device["Name"]?.ToString() ?? "Неизвестно";
                        var manufacturer = device["Manufacturer"]?.ToString() ?? "Неизвестно";
                        var status = device["Status"]?.ToString() ?? "Неизвестно";
                        
                        _databaseManager.AddReportData(reportId, $"Аудиоустройство {deviceCount} - Название", name, "string", "Аудио");
                        _databaseManager.AddReportData(reportId, $"Аудиоустройство {deviceCount} - Производитель", manufacturer, "string", "Аудио");
                        _databaseManager.AddReportData(reportId, $"Аудиоустройство {deviceCount} - Статус", status, "string", "Аудио");
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации об аудио", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о принтерах
        /// </summary>
        private void CollectPrintersInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Printer"))
                {
                    var printers = searcher.Get();
                    int printerCount = 0;
                    int defaultPrinterCount = 0;
                    
                    foreach (System.Management.ManagementObject printer in printers)
                    {
                        printerCount++;
                        var name = printer["Name"]?.ToString() ?? "Неизвестно";
                        var isDefault = printer["Default"]?.ToString() == "True";
                        var status = printer["Status"]?.ToString() ?? "Неизвестно";
                        
                        if (isDefault) defaultPrinterCount++;
                        
                        _databaseManager.AddReportData(reportId, $"Принтер {printerCount} - Название", name, "string", "Принтеры");
                        _databaseManager.AddReportData(reportId, $"Принтер {printerCount} - По умолчанию", isDefault.ToString(), "bool", "Принтеры");
                        _databaseManager.AddReportData(reportId, $"Принтер {printerCount} - Статус", status, "string", "Принтеры");
                    }
                    
                    _databaseManager.AddReportData(reportId, "Всего принтеров", printerCount.ToString(), "int", "Принтеры");
                    _databaseManager.AddReportData(reportId, "Принтеров по умолчанию", defaultPrinterCount.ToString(), "int", "Принтеры");
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о принтерах", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации об обновлениях Windows
        /// </summary>
        private void CollectWindowsUpdatesInfo(int reportId)
        {
            try
            {
                _databaseManager.AddReportData(reportId, "Обновления Windows", "Информация об обновлениях", "string", "Обновления");
                _databaseManager.AddReportData(reportId, "Последняя проверка обновлений", "Информация недоступна через WMI", "string", "Обновления");
                _databaseManager.AddReportData(reportId, "Доступные обновления", "Информация недоступна через WMI", "string", "Обновления");
                _databaseManager.AddReportData(reportId, "Установленные обновления", "Информация недоступна через WMI", "string", "Обновления");
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации об обновлениях", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о планировщике задач
        /// </summary>
        private void CollectTaskSchedulerInfo(int reportId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_ScheduledJob"))
                {
                    var tasks = searcher.Get();
                    int taskCount = 0;
                    int enabledTasks = 0;
                    
                    foreach (System.Management.ManagementObject task in tasks)
                    {
                        taskCount++;
                        var name = task["Name"]?.ToString() ?? "Неизвестно";
                        var enabled = task["Enabled"]?.ToString() == "True";
                        var nextRunTime = task["NextRunTime"]?.ToString() ?? "Неизвестно";
                        
                        if (enabled) enabledTasks++;
                        
                        _databaseManager.AddReportData(reportId, $"Задача {taskCount} - Название", name, "string", "Планировщик");
                        _databaseManager.AddReportData(reportId, $"Задача {taskCount} - Включена", enabled.ToString(), "bool", "Планировщик");
                        _databaseManager.AddReportData(reportId, $"Задача {taskCount} - Следующий запуск", nextRunTime, "string", "Планировщик");
                    }
                    
                    _databaseManager.AddReportData(reportId, "Всего задач", taskCount.ToString(), "int", "Планировщик");
                    _databaseManager.AddReportData(reportId, "Активных задач", enabledTasks.ToString(), "int", "Планировщик");
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о планировщике", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о процессоре с фильтрами
        /// </summary>
        private void CollectCPUInfoWithFilters(int reportId, Dictionary<string, object> advancedSettings)
        {
            try
            {
                // Базовая информация о процессоре
                _databaseManager.AddReportData(reportId, "Количество ядер", System.Environment.ProcessorCount.ToString(), "int", "Процессор");
                _databaseManager.AddReportData(reportId, "Архитектура", System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"), "string", "Процессор");
                _databaseManager.AddReportData(reportId, "Идентификатор", System.Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"), "string", "Процессор");
                
                // Применяем фильтры производительности
                if (advancedSettings != null)
                {
                    if (advancedSettings.ContainsKey("IncludeOnlyHighCPU") && (bool)advancedSettings["IncludeOnlyHighCPU"])
                    {
                        var cpuUsage = GetCpuUsage();
                        if (cpuUsage > 50)
                        {
                            _databaseManager.AddReportData(reportId, "Высокая загрузка CPU", $"{cpuUsage:F2}%", "double", "Процессор");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о процессоре", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о памяти с фильтрами
        /// </summary>
        private void CollectMemoryInfoWithFilters(int reportId, Dictionary<string, object> advancedSettings)
        {
            try
            {
                var workingSet = GC.GetTotalMemory(false);
                var process = System.Diagnostics.Process.GetCurrentProcess();
                
                _databaseManager.AddReportData(reportId, "Используемая память (МБ)", (workingSet / 1024 / 1024).ToString(), "double", "Память");
                _databaseManager.AddReportData(reportId, "Рабочий набор (МБ)", (process.WorkingSet64 / 1024 / 1024).ToString(), "double", "Память");
                
                // Применяем фильтры производительности
                if (advancedSettings != null)
                {
                    if (advancedSettings.ContainsKey("IncludeOnlyHighMemory") && (bool)advancedSettings["IncludeOnlyHighMemory"])
                    {
                        var memoryUsage = GetMemoryUsage();
                        if (memoryUsage > 80) // Высокое использование памяти
                        {
                            _databaseManager.AddReportData(reportId, "Высокое использование памяти", $"{memoryUsage:F2}%", "double", "Память");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о памяти", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о дисках с фильтрами
        /// </summary>
        private void CollectDiskInfoWithFilters(int reportId, Dictionary<string, object> advancedSettings)
        {
            try
            {
                var drives = DriveInfo.GetDrives();
                bool includeAll = true;
                
                if (advancedSettings != null)
                {
                    includeAll = !advancedSettings.ContainsKey("IncludeOnlySystemDrives") || 
                               !(bool)advancedSettings["IncludeOnlySystemDrives"];
                }
                
                foreach (var drive in drives)
                {
                    if (drive.IsReady)
                    {
                        bool shouldInclude = includeAll;
                        
                        if (advancedSettings != null)
                        {
                            if (advancedSettings.ContainsKey("IncludeOnlySystemDrives") && 
                                (bool)advancedSettings["IncludeOnlySystemDrives"])
                            {
                                shouldInclude = drive.DriveType == DriveType.Fixed;
                            }
                            
                            if (advancedSettings.ContainsKey("IncludeOnlyRemovableDrives") && 
                                (bool)advancedSettings["IncludeOnlyRemovableDrives"])
                            {
                                shouldInclude = drive.DriveType == DriveType.Removable;
                            }
                            
                            if (advancedSettings.ContainsKey("IncludeOnlyLowSpaceDrives") && 
                                (bool)advancedSettings["IncludeOnlyLowSpaceDrives"])
                            {
                                var usagePercent = ((drive.TotalSize - drive.AvailableFreeSpace) / (double)drive.TotalSize) * 100;
                                shouldInclude = usagePercent > 90; // Меньше 10% свободного места
                            }
                        }
                        
                        if (shouldInclude)
                        {
                            var freeSpaceGB = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
                            var totalSpaceGB = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
                            var usagePercent = ((totalSpaceGB - freeSpaceGB) / totalSpaceGB) * 100;
                            
                            _databaseManager.AddReportData(reportId, $"Диск {drive.Name}", 
                                $"Свободно: {freeSpaceGB:F2} ГБ, Использовано: {usagePercent:F1}%", 
                                "string", "Диски");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о дисках", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о производительности с фильтрами
        /// </summary>
        private void CollectPerformanceInfoWithFilters(int reportId, Dictionary<string, object> advancedSettings)
        {
            try
            {
                var cpuUsage = GetCpuUsage();
                var memoryUsage = GetMemoryUsage();
                
                _databaseManager.AddReportData(reportId, "Загрузка процессора (%)", cpuUsage.ToString("F2"), "double", "Производительность");
                _databaseManager.AddReportData(reportId, "Использование памяти (%)", memoryUsage.ToString("F2"), "double", "Производительность");
                
                // Применяем фильтры производительности
                if (advancedSettings != null)
                {
                    if (advancedSettings.ContainsKey("IncludeOnlyHighCPU") && (bool)advancedSettings["IncludeOnlyHighCPU"])
                    {
                        if (cpuUsage > 50)
                        {
                            _databaseManager.AddReportData(reportId, "Высокая загрузка CPU", $"{cpuUsage:F2}%", "double", "Производительность");
                        }
                    }
                    
                    if (advancedSettings.ContainsKey("IncludeOnlyHighMemory") && (bool)advancedSettings["IncludeOnlyHighMemory"])
                    {
                        if (memoryUsage > 80)
                        {
                            _databaseManager.AddReportData(reportId, "Высокое использование памяти", $"{memoryUsage:F2}%", "double", "Производительность");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о производительности", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о процессах с фильтрами
        /// </summary>
        private void CollectProcessInfoWithFilters(int reportId, Dictionary<string, object> advancedSettings)
        {
            try
            {
                var processes = System.Diagnostics.Process.GetProcesses();
                var filteredProcesses = new List<System.Diagnostics.Process>();
                
                if (advancedSettings != null)
                {
                    if (advancedSettings.ContainsKey("IncludeOnlySystemProcesses") && (bool)advancedSettings["IncludeOnlySystemProcesses"])
                    {
                        filteredProcesses = processes.Where(p => IsSystemProcess(p)).ToList();
                    }
                    else if (advancedSettings.ContainsKey("IncludeOnlyUserProcesses") && (bool)advancedSettings["IncludeOnlyUserProcesses"])
                    {
                        filteredProcesses = processes.Where(p => !IsSystemProcess(p)).ToList();
                    }
                    else if (advancedSettings.ContainsKey("IncludeOnlyActiveProcesses") && (bool)advancedSettings["IncludeOnlyActiveProcesses"])
                    {
                        filteredProcesses = processes.Where(p => !p.HasExited && p.Responding).ToList();
                    }
                    else
                    {
                        filteredProcesses = processes.ToList();
                    }
                }
                else
                {
                    filteredProcesses = processes.ToList();
                }
                
                _databaseManager.AddReportData(reportId, "Количество процессов", filteredProcesses.Count.ToString(), "int", "Процессы");
                
                // Топ-5 процессов по памяти
                var topProcesses = filteredProcesses
                    .OrderByDescending(p => p.WorkingSet64)
                    .Take(5)
                    .ToArray();
                
                for (int i = 0; i < topProcesses.Length; i++)
                {
                    var proc = topProcesses[i];
                    _databaseManager.AddReportData(reportId, $"Топ {i + 1} процесс", 
                        $"{proc.ProcessName} (PID: {proc.Id}, Память: {proc.WorkingSet64 / 1024 / 1024} МБ)", 
                        "string", "Процессы");
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о процессах", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Сбор информации о службах с фильтрами
        /// </summary>
        private void CollectServiceInfoWithFilters(int reportId, Dictionary<string, object> advancedSettings)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Service"))
                {
                    var services = searcher.Get();
                    int totalServices = 0;
                    int runningServices = 0;
                    int stoppedServices = 0;
                    
                    foreach (System.Management.ManagementObject service in services)
                    {
                        totalServices++;
                        var state = service["State"]?.ToString();
                        var startMode = service["StartMode"]?.ToString();
                        
                        if (state == "Running") runningServices++;
                        if (state == "Stopped") stoppedServices++;
                        
                        // Применяем фильтры
                        if (advancedSettings != null)
                        {
                            if (advancedSettings.ContainsKey("IncludeOnlyRunningServices") && (bool)advancedSettings["IncludeOnlyRunningServices"])
                            {
                                if (state != "Running") continue;
                            }
                            
                            if (advancedSettings.ContainsKey("IncludeOnlyCriticalServices") && (bool)advancedSettings["IncludeOnlyCriticalServices"])
                            {
                                var name = service["Name"]?.ToString() ?? "";
                                if (!IsCriticalService(name)) continue;
                            }
                        }
                        
                        var serviceName = service["Name"]?.ToString() ?? "Неизвестно";
                        var displayName = service["DisplayName"]?.ToString() ?? "Неизвестно";
                        
                        _databaseManager.AddReportData(reportId, $"Служба: {serviceName}", 
                            $"{displayName} - {state} ({startMode})", "string", "Службы");
                    }
                    
                    _databaseManager.AddReportData(reportId, "Всего служб", totalServices.ToString(), "int", "Службы");
                    _databaseManager.AddReportData(reportId, "Запущенных служб", runningServices.ToString(), "int", "Службы");
                    _databaseManager.AddReportData(reportId, "Остановленных служб", stoppedServices.ToString(), "int", "Службы");
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка сбора информации о службах", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Добавление статистики к отчёту
        /// </summary>
        private void AddStatisticsToReport(int reportId)
        {
            try
            {
                var reportData = _databaseManager.GetReportData(reportId);
                var categories = reportData.GroupBy(d => d.Category).ToDictionary(g => g.Key, g => g.Count());
                
                _databaseManager.AddReportData(reportId, "Статистика отчёта", $"Всего категорий: {categories.Count}", "string", "Статистика");
                
                foreach (var category in categories)
                {
                    _databaseManager.AddReportData(reportId, $"Категория: {category.Key}", 
                        $"Количество записей: {category.Value}", "int", "Статистика");
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка добавления статистики", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Добавление рекомендаций к отчёту
        /// </summary>
        private void AddRecommendationsToReport(int reportId)
        {
            try
            {
                var cpuUsage = GetCpuUsage();
                var memoryUsage = GetMemoryUsage();
                
                if (cpuUsage > 80)
                {
                    _databaseManager.AddReportData(reportId, "Рекомендация", 
                        "Высокая загрузка процессора. Рекомендуется закрыть ненужные программы.", "string", "Рекомендации");
                }
                
                if (memoryUsage > 85)
                {
                    _databaseManager.AddReportData(reportId, "Рекомендация", 
                        "Высокое использование памяти. Рекомендуется перезагрузить систему.", "string", "Рекомендации");
                }
                
                // Проверяем диски
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives.Where(d => d.IsReady))
                {
                    var usagePercent = ((drive.TotalSize - drive.AvailableFreeSpace) / (double)drive.TotalSize) * 100;
                    if (usagePercent > 90)
                    {
                        _databaseManager.AddReportData(reportId, "Рекомендация", 
                            $"Диск {drive.Name} заполнен на {usagePercent:F1}%. Рекомендуется освободить место.", "string", "Рекомендации");
                    }
                }
            }
            catch (Exception ex)
            {
                _databaseManager.AddReportData(reportId, "Ошибка добавления рекомендаций", ex.Message, "string", "Ошибки");
            }
        }

        /// <summary>
        /// Проверка, является ли процесс системным
        /// </summary>
        private bool IsSystemProcess(System.Diagnostics.Process process)
        {
            try
            {
                // Простая проверка по имени процесса
                var systemProcesses = new[] { "System", "Idle", "csrss", "winlogon", "services", "lsass", "svchost", "explorer" };
                return systemProcesses.Contains(process.ProcessName.ToLower());
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка, является ли служба критической
        /// </summary>
        private bool IsCriticalService(string serviceName)
        {
            var criticalServices = new[] { "AudioSrv", "BITS", "CryptSvc", "Dhcp", "Dnscache", "EventLog", "LanmanServer", "LanmanWorkstation", "Netman", "PlugPlay", "RpcSs", "Spooler", "Tcpip", "Themes", "TrkWks", "W32Time", "Winmgmt" };
            return criticalServices.Contains(serviceName);
        }

        #endregion
    }
}
