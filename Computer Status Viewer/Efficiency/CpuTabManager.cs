using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace Computer_Status_Viewer
{
    public class CpuTabManager : IDisposable
    {
        public TabItem Tab { get; private set; }
        public ChartValues<double> CpuValues { get; } = new ChartValues<double>(Enumerable.Repeat(0.0, 60));
        private readonly PerformanceCounter cpuCounter;
        private DateTime bootTime;

        public CpuTabManager()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            InitializeBootTime();
            Tab = InitializeCpuTab();
        }

        public void WarmUpCounter()
        {
            cpuCounter.NextValue(); // Прогрев счетчика
        }

        private void InitializeBootTime()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string lastBootUpTime = obj["LastBootUpTime"]?.ToString();
                    bootTime = string.IsNullOrEmpty(lastBootUpTime)
                        ? DateTime.Now
                        : ManagementDateTimeConverter.ToDateTime(lastBootUpTime);
                }
            }
        }

        private TabItem InitializeCpuTab()
        {
            TabItem tab = new TabItem { Header = "ЦП" }; 
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
                Text = "ЦП",
                Foreground = Brushes.Black, 
                FontSize = 18, 
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 5, 10, 0)
            };
            Grid.SetColumn(titleBlock, 0);
            headerGrid.Children.Add(titleBlock);

            string cpuName = GetCpuName();
            TextBlock cpuNameBlock = new TextBlock
            {
                Text = cpuName,
                Foreground = Brushes.Black,
                FontSize = 18,
                FontWeight = FontWeights.Normal,
                Margin = new Thickness(0, 0, 10, 2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(cpuNameBlock, 2);
            headerGrid.Children.Add(cpuNameBlock);

            Grid.SetRow(headerGrid, 0);
            mainGrid.Children.Add(headerGrid);

            TextBlock usageLabel = new TextBlock { Text = "Использование процессора", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = new SolidColorBrush(Color.FromArgb(150, 105, 105, 105)), Margin = new Thickness(5, 5, 0, 2) };
            Grid.SetRow(usageLabel, 1);
            mainGrid.Children.Add(usageLabel);

            var chart = ChartHelper.SetupChart(CpuValues, Brushes.Blue, "Использование (%)");
            Grid.SetRow(chart, 2);
            mainGrid.Children.Add(chart);

            Grid dataGrid = new Grid();
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 300 });
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 200 });

            Grid leftGrid = new Grid();
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            SolidColorBrush labelColor = new SolidColorBrush(Color.FromRgb(105, 105, 105));
            SolidColorBrush valueColor = Brushes.Black;

            TextBlock usageLabelData = new TextBlock { Text = "Использование", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(5, 2, 10, 2) };
            Grid.SetRow(usageLabelData, 0); Grid.SetColumn(usageLabelData, 0); leftGrid.Children.Add(usageLabelData);

            TextBlock speedLabel = new TextBlock { Text = "Скорость", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(0, 2, 10, 2) };
            Grid.SetRow(speedLabel, 0); Grid.SetColumn(speedLabel, 1); leftGrid.Children.Add(speedLabel);

            TextBlock usageValue = new TextBlock { Text = "0 %", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(5, 0, 10, 2) };
            Grid.SetRow(usageValue, 1); Grid.SetColumn(usageValue, 0); leftGrid.Children.Add(usageValue);

            TextBlock speedValue = new TextBlock { Text = "0 ГГц", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(0, 0, 10, 2) };
            Grid.SetRow(speedValue, 1); Grid.SetColumn(speedValue, 1); leftGrid.Children.Add(speedValue);

            TextBlock processesLabel = new TextBlock { Text = "Процессы", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(5, 2, 10, 2) };
            Grid.SetRow(processesLabel, 2); Grid.SetColumn(processesLabel, 0); leftGrid.Children.Add(processesLabel);

            TextBlock threadsLabel = new TextBlock { Text = "Потоки", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(0, 2, 10, 2) };
            Grid.SetRow(threadsLabel, 2); Grid.SetColumn(threadsLabel, 1); leftGrid.Children.Add(threadsLabel);

            TextBlock handlesLabel = new TextBlock { Text = "Дескрипторы", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(0, 2, 10, 2) };
            Grid.SetRow(handlesLabel, 2); Grid.SetColumn(handlesLabel, 2); leftGrid.Children.Add(handlesLabel);

            TextBlock processesValue = new TextBlock { Text = "0", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(5, 0, 10, 2) };
            Grid.SetRow(processesValue, 3); Grid.SetColumn(processesValue, 0); leftGrid.Children.Add(processesValue);

            TextBlock threadsValue = new TextBlock { Text = "0", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(0, 0, 10, 2) };
            Grid.SetRow(threadsValue, 3); Grid.SetColumn(threadsValue, 1); leftGrid.Children.Add(threadsValue);

            TextBlock handlesValue = new TextBlock { Text = "0", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(0, 0, 10, 2) };
            Grid.SetRow(handlesValue, 3); Grid.SetColumn(handlesValue, 2); leftGrid.Children.Add(handlesValue);

            TextBlock uptimeLabel = new TextBlock { Text = "Время работы", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(5, 2, 10, 2) };
            Grid.SetRow(uptimeLabel, 4); Grid.SetColumn(uptimeLabel, 0); leftGrid.Children.Add(uptimeLabel);

            TextBlock uptimeValue = new TextBlock { Text = "0:00:00:00", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(5, 0, 10, 2) };
            Grid.SetRow(uptimeValue, 5); Grid.SetColumn(uptimeValue, 0); leftGrid.Children.Add(uptimeValue);

            Grid.SetColumn(leftGrid, 0);
            dataGrid.Children.Add(leftGrid);

            StackPanel rightPanel = new StackPanel { Margin = new Thickness(0, 5, 5, 5) };
            rightPanel.Children.Add(ChartHelper.CreateDataRow("Базовая скорость:", $"{GetBaseSpeed()} ГГц", true));
            rightPanel.Children.Add(ChartHelper.CreateDataRow("Сокетов:", "1", true));
            rightPanel.Children.Add(ChartHelper.CreateDataRow("Ядер:", $"{GetCores()}", true));
            rightPanel.Children.Add(ChartHelper.CreateDataRow("Логических процессоров:", $"{GetLogicalProcessors()}", true));
            Grid.SetColumn(rightPanel, 1);
            dataGrid.Children.Add(rightPanel);

            Grid.SetRow(dataGrid, 3);
            mainGrid.Children.Add(dataGrid);

            tab.Content = mainGrid;
            return tab;
        }

        public async Task Update()
        {
            // Сбор данных в фоновом потоке
            double usage = Math.Round(cpuCounter.NextValue(), 1);
            double speed = GetCurrentSpeed();
            int processes = GetProcesses();
            int threads = GetThreads();
            int handles = GetHandles();
            string uptime = GetUptime();

            for (int i = 0; i < CpuValues.Count - 1; i++)
            {
                CpuValues[i] = CpuValues[i + 1];
            }
            CpuValues[59] = usage;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var mainGrid = Tab.Content as Grid;
                if (mainGrid == null) return;

                var chart = mainGrid.Children.OfType<CartesianChart>().FirstOrDefault(c => Grid.GetRow(c) == 2);
                if (chart != null)
                {
                    chart.Series[0].Values = CpuValues;
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

                        var speedValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 1 && Grid.GetColumn(tb) == 1);
                        if (speedValue != null) speedValue.Text = $"{speed} ГГц";

                        var processesValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 3 && Grid.GetColumn(tb) == 0);
                        if (processesValue != null) processesValue.Text = $"{processes}";

                        var threadsValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 3 && Grid.GetColumn(tb) == 1);
                        if (threadsValue != null) threadsValue.Text = $"{threads}";

                        var handlesValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 3 && Grid.GetColumn(tb) == 2);
                        if (handlesValue != null) handlesValue.Text = $"{handles}";

                        var uptimeValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 5 && Grid.GetColumn(tb) == 0);
                        if (uptimeValue != null) uptimeValue.Text = uptime;
                    }
                }
            });
        }

        private string GetCpuName()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["Name"]?.ToString() ?? "Unknown CPU";
                }
            }
            return "Unknown CPU";
        }

        private double GetCurrentSpeed()
        {
            double percentProcessorPerformance = 0;
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PerfFormattedData_Counters_ProcessorInformation WHERE Name='_Total'"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    percentProcessorPerformance = Convert.ToDouble(obj["PercentProcessorPerformance"]);
                }
            }

            double maxClockSpeed = 0;
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    maxClockSpeed = Convert.ToDouble(obj["MaxClockSpeed"]) / 1000.0;
                }
            }

            return Math.Round(maxClockSpeed * (percentProcessorPerformance / 100.0), 2);
        }

        private double GetBaseSpeed()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Convert.ToDouble(obj["MaxClockSpeed"]) / 1000.0;
                }
            }
            return 0;
        }

        private int GetCores()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Convert.ToInt32(obj["NumberOfCores"]);
                }
            }
            return 0;
        }

        private int GetLogicalProcessors()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Convert.ToInt32(obj["NumberOfLogicalProcessors"]);
                }
            }
            return 0;
        }

        private int GetProcesses()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Convert.ToInt32(obj["NumberOfProcesses"]);
                }
            }
            return 0;
        }

        private int GetThreads()
        {
            return Process.GetProcesses().Sum(p => p.Threads.Count);
        }

        private int GetHandles()
        {
            return Process.GetProcesses().Sum(p => p.HandleCount);
        }

        private string GetUptime()
        {
            TimeSpan uptime = DateTime.Now - bootTime;
            return $"{(int)uptime.TotalDays}:{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
        }

        public void Dispose()
        {
            cpuCounter?.Dispose();
        }
    }
}