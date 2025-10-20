using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace Computer_Status_Viewer
{
    public class PerformanceManager : IDisposable
    {
        private readonly TabControl performanceTabControl;
        private readonly CpuTabManager cpuTabManager;
        private readonly MemoryTabManager memoryTabManager;
        private GpuTabManager gpuTabManager;
        private NetworkTabManager networkTabManager;
        private readonly List<DiskTabManager> diskTabManagers = new List<DiskTabManager>();
        private readonly Timer timer;
        private bool isDisposed = false;
        private bool isInitialized = false;

        public PerformanceManager(TabControl tabControl)
        {
            performanceTabControl = tabControl ?? throw new ArgumentNullException(nameof(tabControl));
            cpuTabManager = new CpuTabManager();
            memoryTabManager = new MemoryTabManager();

            timer = new Timer(500);
            timer.Elapsed += TimerElapsed;
            timer.AutoReset = true;

            // Прогрев счетчиков в фоне
            Task.Run(() =>
            {
                cpuTabManager.WarmUpCounter();
                memoryTabManager.WarmUpCounter();
            });
        }

        public async Task EnsureInitializedAsync()
        {
            if (!isInitialized)
            {
                await InitializeAsync();
                isInitialized = true;
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => InitializePerformanceTab());
                await UpdateAllTabsAsync(); // Первоначальное обновление
                timer.Start(); // Запускаем таймер
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ошибка в InitializeAsync: {ex.Message}");
                throw;
            }
        }

        private void InitializePerformanceTab()
        {
            var stopwatch = Stopwatch.StartNew();
            performanceTabControl.Items.Clear();
            performanceTabControl.Items.Add(cpuTabManager.Tab);
            performanceTabControl.Items.Add(memoryTabManager.Tab);

            try
            {
                gpuTabManager = new GpuTabManager();
                if (gpuTabManager.HasGpu && gpuTabManager.Tab != null)
                    performanceTabControl.Items.Add(gpuTabManager.Tab);
                else
                {
                    gpuTabManager.Dispose();
                    gpuTabManager = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ошибка в инициализации GPU: {ex.Message}");
                gpuTabManager?.Dispose();
                gpuTabManager = null;
            }

            try
            {
                networkTabManager = new NetworkTabManager();
                if (networkTabManager.HasNetwork && networkTabManager.Tab != null)
                    performanceTabControl.Items.Add(networkTabManager.Tab);
                else
                {
                    networkTabManager.Dispose();
                    networkTabManager = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ошибка в инициализации Network: {ex.Message}");
                networkTabManager?.Dispose();
                networkTabManager = null;
            }

            try
            {
                var diskInstances = GetDiskInstances();
                int diskNumber = 0;
                foreach (var diskInstance in diskInstances)
                {
                    try
                    {
                        var diskTabManager = new DiskTabManager(diskInstance, diskNumber);
                        diskTabManagers.Add(diskTabManager);
                        performanceTabControl.Items.Add(diskTabManager.Tab);
                        Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Добавлена вкладка: {diskTabManager.Tab.Header}");
                        diskNumber++;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ошибка инициализации диска {diskInstance}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ошибка в обработке дисков: {ex.Message}");
            }

            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] InitializePerformanceTab завершено за {stopwatch.ElapsedMilliseconds} мс");
            stopwatch.Stop();
        }

        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (isDisposed) return;

            var stopwatch = Stopwatch.StartNew();
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Начало TimerElapsed");

            try
            {
                await UpdateAllTabsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ошибка в TimerElapsed: {ex.Message}");
            }
            finally
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] TimerElapsed завершено за {stopwatch.ElapsedMilliseconds} мс");
                stopwatch.Stop();
            }
        }

        private async Task UpdateAllTabsAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Начало UpdateAllTabsAsync");

            try
            {
                var updateTasks = new List<Task>
                {
                    cpuTabManager.Update(),
                    memoryTabManager.Update()
                };

                if (gpuTabManager != null && gpuTabManager.HasGpu)
                    updateTasks.Add(gpuTabManager.Update());

                if (networkTabManager != null && networkTabManager.HasNetwork)
                    updateTasks.Add(networkTabManager.Update());

                updateTasks.AddRange(diskTabManagers.Select(d => d.Update()));

                await Task.WhenAll(updateTasks);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ошибка в UpdateAllTabsAsync: {ex.Message}");
            }
            finally
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] UpdateAllTabsAsync завершено за {stopwatch.ElapsedMilliseconds} мс");
                stopwatch.Stop();
            }
        }

        private List<string> GetDiskInstances()
        {
            var diskInstances = new List<string>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PerfFormattedData_PerfDisk_PhysicalDisk"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString();
                        if (!string.IsNullOrEmpty(name) && name != "_Total")
                            diskInstances.Add(name);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ошибка получения списка дисков: {ex.Message}");
            }
            return diskInstances;
        }

        public void Dispose()
        {
            if (isDisposed) return;
            timer?.Stop();
            timer?.Dispose();
            cpuTabManager?.Dispose();
            memoryTabManager?.Dispose();
            gpuTabManager?.Dispose();
            networkTabManager?.Dispose();
            foreach (var diskTabManager in diskTabManagers)
                diskTabManager?.Dispose();
            diskTabManagers.Clear();
            isDisposed = true;
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ресурсы освобождены");
        }
    }
}