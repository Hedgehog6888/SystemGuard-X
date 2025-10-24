using System;
using System.Threading;
using System.Threading.Tasks;
using Computer_Status_Viewer.Properties;

namespace Computer_Status_Viewer.Reports
{
    /// <summary>
    /// Сервис для автоматического создания отчётов
    /// </summary>
    public class AutoReportService
    {
        private readonly ReportManager _reportManager;
        private Timer _timer;
        private bool _isRunning;
        private readonly object _lockObject = new object();

        public AutoReportService()
        {
            _reportManager = new ReportManager();
        }

        /// <summary>
        /// Запуск автоматического создания отчётов
        /// </summary>
        public void Start()
        {
            lock (_lockObject)
            {
                if (_isRunning)
                    return;

                _isRunning = true;
                StartTimer();
            }
        }

        /// <summary>
        /// Остановка автоматического создания отчётов
        /// </summary>
        public void Stop()
        {
            lock (_lockObject)
            {
                if (!_isRunning)
                    return;

                _isRunning = false;
                _timer?.Dispose();
                _timer = null;
            }
        }

        /// <summary>
        /// Запуск таймера для создания отчётов
        /// </summary>
        private void StartTimer()
        {
            var interval = GetIntervalFromSettings();
            _timer = new Timer(CreateReportsCallback, null, TimeSpan.Zero, interval);
        }

        /// <summary>
        /// Получение интервала из настроек
        /// </summary>
        private TimeSpan GetIntervalFromSettings()
        {
            var settings = Properties.Settings.Default;
            var intervalText = settings.ReportInterval;

            switch (intervalText)
            {
                case "Каждую минуту":
                    return TimeSpan.FromMinutes(1);
                case "Каждый час":
                    return TimeSpan.FromHours(1);
                case "Каждые 6 часов":
                    return TimeSpan.FromHours(6);
                case "Ежедневно":
                    return TimeSpan.FromDays(1);
                case "Еженедельно":
                    return TimeSpan.FromDays(7);
                default:
                    return TimeSpan.FromHours(1); // По умолчанию каждый час
            }
        }

        /// <summary>
        /// Обработчик создания отчётов
        /// </summary>
        private void CreateReportsCallback(object state)
        {
            if (!_isRunning)
                return;

            try
            {
                Task.Run(() => CreateReportsAsync());
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не останавливаем сервис
                System.Diagnostics.Debug.WriteLine($"Ошибка в AutoReportService: {ex.Message}");
            }
        }

        /// <summary>
        /// Асинхронное создание отчётов
        /// </summary>
        private async Task CreateReportsAsync()
        {
            try
            {
                var settings = Properties.Settings.Default;

                // Создаём системные отчёты, если включены
                if (settings.AutoCreateSystemReports)
                {
                    await CreateSystemReportAsync();
                }

                // Создаём отчёты о производительности, если включены
                if (settings.AutoCreatePerformanceReports)
                {
                    await CreatePerformanceReportAsync();
                }

                // Создаём отчёты о безопасности, если включены
                if (settings.AutoCreateSecurityReports)
                {
                    await CreateSecurityReportAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания автоматических отчётов: {ex.Message}");
                
                // Автоматические отчёты создаются без уведомлений об ошибках
            }
        }

        /// <summary>
        /// Создание системного отчёта
        /// </summary>
        private async Task CreateSystemReportAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var reportId = _reportManager.CreateSystemReport();
                    System.Diagnostics.Debug.WriteLine($"Создан автоматический системный отчёт ID: {reportId}");
                });

                // Автоматические отчёты создаются без уведомлений
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания системного отчёта: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Создание отчёта о производительности
        /// </summary>
        private async Task CreatePerformanceReportAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var reportId = _reportManager.CreatePerformanceReport();
                    System.Diagnostics.Debug.WriteLine($"Создан автоматический отчёт о производительности ID: {reportId}");
                });

                // Автоматические отчёты создаются без уведомлений
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания отчёта о производительности: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Создание отчёта о безопасности
        /// </summary>
        private async Task CreateSecurityReportAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var reportId = _reportManager.CreateSecurityReport();
                    System.Diagnostics.Debug.WriteLine($"Создан автоматический отчёт о безопасности ID: {reportId}");
                });

                // Автоматические отчёты создаются без уведомлений
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания отчёта о безопасности: {ex.Message}");
                throw;
            }
        }


        /// <summary>
        /// Обновление интервала таймера
        /// </summary>
        public void UpdateInterval()
        {
            lock (_lockObject)
            {
                if (_isRunning)
                {
                    _timer?.Dispose();
                    StartTimer();
                }
            }
        }

        /// <summary>
        /// Проверка, запущен ли сервис
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (_lockObject)
                {
                    return _isRunning;
                }
            }
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}
