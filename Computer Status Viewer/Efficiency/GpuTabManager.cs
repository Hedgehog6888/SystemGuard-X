using System;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibreHardwareMonitor.Hardware;
using LiveCharts;
using LiveCharts.Wpf;

namespace Computer_Status_Viewer
{
    public class GpuTabManager : IDisposable
    {
        public TabItem Tab { get; private set; }
        public ChartValues<double> GpuValues { get; } = new ChartValues<double>(Enumerable.Repeat(0.0, 60));
        private readonly Computer computer;
        private bool hasGpu;

        public GpuTabManager()
        {
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
            if (hasGpu)
            {
                Tab = InitializeGpuTab();
            }
        }

        private bool CheckGpuAvailability()
        {
            var gpuHardware = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia ||
                                                                   h.HardwareType == HardwareType.GpuAmd ||
                                                                   h.HardwareType == HardwareType.GpuIntel);
            return gpuHardware != null && !string.IsNullOrEmpty(GetGpuName()) && GetGpuName() != "Unknown GPU";
        }

        private TabItem InitializeGpuTab()
        {
            TabItem tab = new TabItem { Header = "GPU" }; // Простая строка для заголовка вкладки
            Grid mainGrid = new Grid();

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock titleBlock = new TextBlock
            {
                Text = "GPU",
                Foreground = Brushes.Black,
                FontSize = 18,
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 5, 10, 0)
            };
            Grid.SetColumn(titleBlock, 0);
            headerGrid.Children.Add(titleBlock);

            string gpuName = GetGpuName();
            TextBlock gpuNameBlock = new TextBlock
            {
                Text = gpuName,
                Foreground = Brushes.Black,
                FontSize = 18,
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 10, 2)
            };
            Grid.SetColumn(gpuNameBlock, 2);
            headerGrid.Children.Add(gpuNameBlock);

            Grid.SetRow(headerGrid, 0);
            mainGrid.Children.Add(headerGrid);

            TextBlock usageLabel = new TextBlock { Text = "Использование GPU", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = new SolidColorBrush(Color.FromArgb(150, 105, 105, 105)), Margin = new Thickness(5, 5, 0, 2) };
            Grid.SetRow(usageLabel, 1);
            mainGrid.Children.Add(usageLabel);

            var chart = ChartHelper.SetupChart(GpuValues, Brushes.Red, "Использование (%)");
            Grid.SetRow(chart, 2);
            mainGrid.Children.Add(chart);

            Grid dataGrid = new Grid();
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 300 });
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 200 });

            Grid leftGrid = new Grid();
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            SolidColorBrush labelColor = new SolidColorBrush(Color.FromRgb(105, 105, 105));
            SolidColorBrush valueColor = Brushes.Black;

            TextBlock usageLabelData = new TextBlock { Text = "Использование", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(5, 2, 10, 2) };
            Grid.SetRow(usageLabelData, 0); Grid.SetColumn(usageLabelData, 0); leftGrid.Children.Add(usageLabelData);

            TextBlock temperatureLabel = new TextBlock { Text = "Температура", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(5, 2, 10, 2) };
            Grid.SetRow(temperatureLabel, 0); Grid.SetColumn(temperatureLabel, 1); leftGrid.Children.Add(temperatureLabel);

            TextBlock usageValue = new TextBlock { Text = "0 %", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(5, 0, 10, 2) };
            Grid.SetRow(usageValue, 1); Grid.SetColumn(usageValue, 0); leftGrid.Children.Add(usageValue);

            TextBlock temperatureValue = new TextBlock { Text = "0°C", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(5, 0, 10, 2) };
            Grid.SetRow(temperatureValue, 1); Grid.SetColumn(temperatureValue, 1); leftGrid.Children.Add(temperatureValue);

            Grid.SetColumn(leftGrid, 0);
            dataGrid.Children.Add(leftGrid);

            StackPanel rightPanel = new StackPanel { Margin = new Thickness(0, 5, 5, 5) };
            rightPanel.Children.Add(ChartHelper.CreateDataRow("Версия драйвера:", GetDriverVersion() ?? "N/A", true));
            rightPanel.Children.Add(ChartHelper.CreateDataRow("Дата драйвера:", GetDriverDate() ?? "N/A", true));
            rightPanel.Children.Add(ChartHelper.CreateDataRow("DirectX:", "12.0", true));
            Grid.SetColumn(rightPanel, 1);
            dataGrid.Children.Add(rightPanel);

            Grid.SetRow(dataGrid, 3);
            mainGrid.Children.Add(dataGrid);

            tab.Content = mainGrid;
            return tab;
        }

        public async Task Update()
        {
            if (!hasGpu || Tab == null) return;

            double usage = GetGpuUsage();
            int temperature = GetGpuTemperature();

            for (int i = 0; i < GpuValues.Count - 1; i++)
            {
                GpuValues[i] = GpuValues[i + 1];
            }
            GpuValues[59] = usage;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var mainGrid = Tab.Content as Grid;
                if (mainGrid == null) return;

                var chart = mainGrid.Children.OfType<CartesianChart>().FirstOrDefault(c => Grid.GetRow(c) == 2);
                if (chart != null)
                {
                    chart.Series[0].Values = GpuValues;
                    chart.Update(false, true);
                }

                var dataGrid = mainGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == 3);
                if (dataGrid != null)
                {
                    var leftGrid = dataGrid.Children.OfType<Grid>().FirstOrDefault(sp => Grid.GetColumn(sp) == 0);
                    if (leftGrid != null)
                    {
                        var usageValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 1 && Grid.GetColumn(tb) == 0);
                        if (usageValue != null) usageValue.Text = $"{usage} %";

                        var temperatureValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 1 && Grid.GetColumn(tb) == 1);
                        if (temperatureValue != null) temperatureValue.Text = $"{temperature}°C";
                    }
                }
            });
        }

        private string GetGpuName()
        {
            var gpuHardware = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia ||
                                                                   h.HardwareType == HardwareType.GpuAmd ||
                                                                   h.HardwareType == HardwareType.GpuIntel);
            if (gpuHardware != null)
            {
                gpuHardware.Update();
                return gpuHardware.Name;
            }

            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["Name"]?.ToString() ?? "Unknown GPU";
                }
            }
            return "Unknown GPU";
        }

        private double GetGpuUsage()
        {
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

        private int GetGpuTemperature()
        {
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

        private string GetDriverVersion()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["DriverVersion"]?.ToString() ?? "Unknown";
                }
            }
            return "Unknown";
        }

        private string GetDriverDate()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string date = obj["DriverDate"]?.ToString();
                    if (string.IsNullOrEmpty(date) || date.Length < 8) return "Unknown";
                    return date.Substring(6, 2) + "." + date.Substring(4, 2) + "." + date.Substring(0, 4);
                }
            }
            return "Unknown";
        }

        public bool HasGpu => hasGpu;

        public void Dispose()
        {
            computer?.Close();
        }
    }
}