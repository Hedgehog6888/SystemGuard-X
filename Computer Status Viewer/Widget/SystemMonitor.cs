using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using LibreHardwareMonitor.Hardware;
using Microsoft.Win32;

namespace Computer_Status_Viewer
{
    public class SystemMonitor : IDisposable
    {
        private readonly PerformanceCounter cpuCounter;
        private readonly PerformanceCounter ramCounter;
        private readonly Computer computer;
        private bool hasGpu;

        public SystemMonitor()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

            computer = new Computer
            {
                IsCpuEnabled = false,
                IsGpuEnabled = true,
                IsMemoryEnabled = false,
                IsMotherboardEnabled = false,
                IsControllerEnabled = false,
                IsNetworkEnabled = false,
                IsStorageEnabled = false
            };
            computer.Open();
            hasGpu = CheckGpuAvailability();
        }

        private bool CheckGpuAvailability()
        {
            var gpuHardware = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia ||
                                                                   h.HardwareType == HardwareType.GpuAmd ||
                                                                   h.HardwareType == HardwareType.GpuIntel);
            return gpuHardware != null;
        }

        public double GetCpuUsage() => Math.Round(cpuCounter.NextValue(), 1);
        public double GetCpuSpeed()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT CurrentClockSpeed FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Convert.ToDouble(obj["CurrentClockSpeed"]) / 1000.0;
                }
            }
            return 0;
        }
        public int GetCpuTemp() => 0;
        public int GetCoreCount() => Environment.ProcessorCount;

        public double GetRamUsage() => Math.Round(ramCounter.NextValue(), 1);
        public (double usedMB, double totalMB) GetRamDetails()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    double totalKB = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
                    double freeKB = Convert.ToDouble(obj["FreePhysicalMemory"]);
                    return ((totalKB - freeKB) / 1024.0, totalKB / 1024.0);
                }
            }
            return (0, 0);
        }

        public double GetDiskUsage()
        {
            var (freeMB, totalMB) = GetDiskDetails();
            return Math.Round(((totalMB - freeMB) / totalMB) * 100, 1);
        }
        public (double freeMB, double totalMB) GetDiskDetails()
        {
            DriveInfo drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.DriveType == DriveType.Fixed);
            if (drive != null)
            {
                return (drive.TotalFreeSpace / (1024.0 * 1024.0), drive.TotalSize / (1024.0 * 1024.0));
            }
            return (0, 0);
        }

        public double GetGpuUsage()
        {
            if (!hasGpu) return 0;
            var gpuHardware = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia ||
                                                                   h.HardwareType == HardwareType.GpuAmd ||
                                                                   h.HardwareType == HardwareType.GpuIntel);
            if (gpuHardware != null)
            {
                gpuHardware.Update();
                var usageSensor = gpuHardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name.Contains("GPU Core"));
                return usageSensor != null ? Math.Round(usageSensor.Value.GetValueOrDefault(), 1) : 0;
            }
            return 0;
        }

        public int GetGpuTemperature()
        {
            if (!hasGpu) return 0;
            var gpuHardware = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia ||
                                                                   h.HardwareType == HardwareType.GpuAmd ||
                                                                   h.HardwareType == HardwareType.GpuIntel);
            if (gpuHardware != null)
            {
                gpuHardware.Update();
                var tempSensor = gpuHardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Name.Contains("GPU Core"));
                return tempSensor != null ? (int)tempSensor.Value.GetValueOrDefault() : 0;
            }
            return 0;
        }

        public int GetActiveProcesses() => Process.GetProcesses().Length;
        public int GetThreadCount() => Process.GetProcesses().Sum(p => p.Threads.Count);

        public string GetUptime()
        {
            TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
            return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
        }

        public (int itemCount, long size) GetRecycleBinInfo()
        {
            int itemCount = 0;
            long totalSize = 0;

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_RecycleBin"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        itemCount++;
                        totalSize += Convert.ToInt64(obj["Size"]);
                    }
                }
            }
            catch
            {
                // Если WMI не работает, используем Shell
                var shell = new Shell32.Shell();
                var recycler = shell.NameSpace(Shell32.ShellSpecialFolderConstants.ssfBITBUCKET);
                itemCount = recycler.Items().Count;
                foreach (Shell32.FolderItem2 item in recycler.Items())
                {
                    totalSize += item.Size;
                }
            }

            return (itemCount, totalSize);
        }

        public void Dispose()
        {
            cpuCounter?.Dispose();
            ramCounter?.Dispose();
            computer?.Close();
        }
    }
}