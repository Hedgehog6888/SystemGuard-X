using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Computer_Status_Viewer
{
    public class WidgetManager : IDisposable
    {
        private readonly Dictionary<string, WidgetWindow> widgets = new Dictionary<string, WidgetWindow>();
        private readonly DispatcherTimer updateTimer;
        private readonly SystemMonitor systemMonitor;
        private bool isDisposed = false;
        private int nextX = 0;

        private class SystemData
        {
            public double CpuUsage { get; set; }
            public double CpuSpeed { get; set; }
            public double RamUsage { get; set; }
            public double RamUsedMB { get; set; }
            public double RamTotalMB { get; set; }
            public double DiskFreeMB { get; set; }
            public double DiskTotalMB { get; set; }
            public double DiskUsagePercent { get; set; }
            public double GpuUsage { get; set; }
            public int GpuTemp { get; set; }
            public int Processes { get; set; }
            public int Threads { get; set; }
            public string Uptime { get; set; }
            public int RecycleBinItemCount { get; set; }
            public long RecycleBinSize { get; set; }
        }

        private SystemData currentData;

        public WidgetManager()
        {
            systemMonitor = new SystemMonitor();
            updateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            updateTimer.Tick += UpdateWidgets;
            updateTimer.Start();
        }

        public void CreateWidget1() => CreateWidget("Widget Basic", GetWidget1Data, 250, 255);
        public void CreateWidget2() => CreateWidget("Widget Advanced", GetWidget2Data, 250, 425); // Увеличиваем до 420
        public void CreateWidget3() => CreateWidget("SYSTEM 3", GetWidget3Data, 250, 220);

        private void CreateWidget(string title, Func<(string text, List<(int value, int max)> bars)> updateFunc, int width, int height)
        {
            if (widgets.ContainsKey(title))
            {
                widgets[title].Close();
                widgets.Remove(title);
                nextX -= width + 10;
                return;
            }

            var widget = new WidgetWindow(title, width, height, nextX);
            widgets[title] = widget;
            widget.Show();
            nextX += width + 10;
        }

        private void CollectSystemData()
        {
            var (ramUsed, ramTotal) = systemMonitor.GetRamDetails();
            var (diskFreeMB, diskTotalMB) = systemMonitor.GetDiskDetails();
            double diskUsagePercent = Math.Round(((diskTotalMB - diskFreeMB) / diskTotalMB) * 100, 1);
            var (itemCount, size) = systemMonitor.GetRecycleBinInfo();

            currentData = new SystemData
            {
                CpuUsage = systemMonitor.GetCpuUsage(),
                CpuSpeed = systemMonitor.GetCpuSpeed(),
                RamUsage = systemMonitor.GetRamUsage(),
                RamUsedMB = ramUsed,
                RamTotalMB = ramTotal,
                DiskFreeMB = diskFreeMB,
                DiskTotalMB = diskTotalMB,
                DiskUsagePercent = diskUsagePercent,
                GpuUsage = systemMonitor.GetGpuUsage(),
                GpuTemp = systemMonitor.GetGpuTemperature(),
                Processes = systemMonitor.GetActiveProcesses(),
                Threads = systemMonitor.GetThreadCount(),
                Uptime = systemMonitor.GetUptime(),
                RecycleBinItemCount = itemCount,
                RecycleBinSize = size
            };
        }

        public (string text, List<(int value, int max)> bars) GetWidget1Data()
        {
            string text = $"CPU USAGE: {currentData.CpuUsage:F1}%\n" +
                          $"{currentData.CpuSpeed:F2} GHz speed\n" +
                          $"MEMORY: {currentData.RamUsage:F1}%\n" +
                          $"{currentData.RamUsedMB:F1} MB used from {currentData.RamTotalMB:F1} MB\n" +
                          $"{currentData.Uptime}";

            return (text, new List<(int, int)>
            {
                ((int)currentData.CpuUsage, 100),
                ((int)currentData.RamUsage, 100)
            });
        }

        public (string text, List<(int value, int max)> bars) GetWidget2Data()
        {
            double diskFreeGB = currentData.DiskFreeMB / 1024.0;
            double diskTotalGB = currentData.DiskTotalMB / 1024.0;

            string text = $"CPU USAGE: {currentData.CpuUsage:F1}%\n" +
                          $"{currentData.CpuSpeed:F2} GHz speed\n" +
                          $"MEMORY: {currentData.RamUsage:F1}%\n" +
                          $"{currentData.RamUsedMB:F1} MB used from {currentData.RamTotalMB:F1} MB\n" +
                          $"HARD DISK DRIVE: {currentData.DiskUsagePercent:F1}%\n" +
                          $"{diskFreeGB:F1} GB free from {diskTotalGB:F1} GB\n" +
                          $"GPU USAGE: {currentData.GpuUsage:F1}%\n" +
                          $"Temperature: {currentData.GpuTemp}°C\n" +
                          $"PROCESSES: {currentData.Processes}\n" +
                          $"THREADS: {currentData.Threads}\n" +
                          $"{currentData.Uptime}";

            return (text, new List<(int, int)>
            {
                ((int)currentData.CpuUsage, 100),
                ((int)currentData.RamUsage, 100),
                ((int)currentData.DiskUsagePercent, 100),
                ((int)currentData.GpuUsage, 100)
            });
        }

        public (string text, List<(int value, int max)> bars) GetWidget3Data()
        {
            string status = currentData.RecycleBinItemCount > 0 ? "FULL" : "EMPTY";
            double sizeMB = currentData.RecycleBinSize / (1024.0 * 1024.0);

            string text = $"{status}\nITEMS: {currentData.RecycleBinItemCount}  SIZE: {sizeMB:F1} MB";

            return (text, new List<(int, int)>()); // Прогресс-бар не нужен
        }

        private void UpdateWidgets(object sender, EventArgs e)
        {
            if (!isDisposed)
            {
                CollectSystemData();
                foreach (var widgetPair in widgets)
                {
                    var widget = widgetPair.Value;
                    var (text, bars) = widgetPair.Key switch
                    {
                        "Widget Basic" => GetWidget1Data(),
                        "Widget Advanced" => GetWidget2Data(),
                        "SYSTEM 3" => GetWidget3Data(),
                        _ => (string.Empty, new List<(int, int)>())
                    };
                    widget.UpdateContent(text, bars);
                }
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                updateTimer.Stop();
                foreach (var widget in widgets.Values)
                {
                    widget.Close();
                }
                widgets.Clear();
                systemMonitor.Dispose();
                isDisposed = true;
            }
        }
    }
}