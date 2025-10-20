using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;

namespace Computer_Status_Viewer
{
    public class DiskTabManager : IDisposable
    {
        public TabItem Tab { get; private set; }
        public ChartValues<double> DiskReadSpeedValues { get; } = new ChartValues<double>(Enumerable.Repeat(0.0, 60));
        public ChartValues<double> DiskWriteSpeedValues { get; } = new ChartValues<double>(Enumerable.Repeat(0.0, 60));
        private readonly PerformanceCounter diskReadCounter;
        private readonly PerformanceCounter diskWriteCounter;
        private readonly PerformanceCounter diskTimeCounter;
        private readonly string diskName;
        private readonly string displayName;

        public DiskTabManager(string diskInstanceName, int diskNumber)
        {
            diskName = diskInstanceName;
            string driveLetter = diskInstanceName.Split(' ')[1];
            displayName = $"Диск {diskNumber} ({driveLetter})";

            diskReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", diskInstanceName);
            diskWriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", diskInstanceName);
            diskTimeCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", diskInstanceName);

            Tab = InitializeDiskTab();
        }

        private DriveInfo GetDiskDrive(string instanceName)
        {
            string driveLetter = instanceName.Split(' ')[1];
            return DriveInfo.GetDrives().FirstOrDefault(d => d.Name.StartsWith(driveLetter, StringComparison.OrdinalIgnoreCase) && d.IsReady);
        }

        private TabItem InitializeDiskTab()
        {
            var diskDrive = GetDiskDrive(diskName);
            TabItem tab = new TabItem { Header = displayName }; // Простая строка для заголовка вкладки
            Grid mainGrid = new Grid();

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock titleBlock = new TextBlock
            {
                Text = displayName,
                Foreground = Brushes.Black,
                FontSize = 18,
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 5, 10, 0)
            };
            Grid.SetColumn(titleBlock, 0);
            headerGrid.Children.Add(titleBlock);

            string model = GetDiskModel();
            TextBlock diskModelBlock = new TextBlock
            {
                Text = model,
                Foreground = Brushes.Black,
                FontSize = 18,
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 10, 2)
            };
            Grid.SetColumn(diskModelBlock, 2);
            headerGrid.Children.Add(diskModelBlock);

            Grid.SetRow(headerGrid, 0);
            mainGrid.Children.Add(headerGrid);

            TextBlock transferRateLabel = new TextBlock { Text = "Скорость обмена данными с диском", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = new SolidColorBrush(Color.FromArgb(150, 105, 105, 105)), Margin = new Thickness(5, 5, 0, 2) };
            Grid.SetRow(transferRateLabel, 1);
            mainGrid.Children.Add(transferRateLabel);

            var transferRateChart = ChartHelper.SetupDiskChart(DiskReadSpeedValues, DiskWriteSpeedValues, "Скорость (КБ/с или МБ/с)");
            Grid.SetRow(transferRateChart, 2);
            mainGrid.Children.Add(transferRateChart);

            Grid dataGrid = new Grid();
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 300 });
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 200 });

            Grid leftGrid = new Grid();
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            SolidColorBrush labelColor = new SolidColorBrush(Color.FromRgb(105, 105, 105));
            SolidColorBrush valueColor = Brushes.Black;

            TextBlock activeTimeLabel = new TextBlock { Text = "Активное время", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(5, 2, 10, 2) };
            Grid.SetRow(activeTimeLabel, 0); Grid.SetColumn(activeTimeLabel, 0); leftGrid.Children.Add(activeTimeLabel);

            TextBlock activeTimeValue = new TextBlock { Text = "0 %", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(5, 0, 10, 2) };
            Grid.SetRow(activeTimeValue, 1); Grid.SetColumn(activeTimeValue, 0); leftGrid.Children.Add(activeTimeValue);

            Grid readSpeedGrid = new Grid();
            readSpeedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            readSpeedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Line readLine = new Line { X1 = 0, Y1 = 0, X2 = 20, Y2 = 0, Stroke = Brushes.Orange, StrokeThickness = 2, Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(readLine, 0); readSpeedGrid.Children.Add(readLine);

            TextBlock readSpeedLabel = new TextBlock { Text = "Скорость чтения", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(0, 2, 10, 2) };
            Grid.SetColumn(readSpeedLabel, 1); readSpeedGrid.Children.Add(readSpeedLabel);

            Grid.SetRow(readSpeedGrid, 2); Grid.SetColumn(readSpeedGrid, 0); leftGrid.Children.Add(readSpeedGrid);

            Grid writeSpeedGrid = new Grid();
            writeSpeedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            writeSpeedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Line writeLine = new Line { X1 = 0, Y1 = 0, X2 = 20, Y2 = 0, Stroke = Brushes.Orange, StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 2, 2 }, Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(writeLine, 0); writeSpeedGrid.Children.Add(writeLine);

            TextBlock writeSpeedLabel = new TextBlock { Text = "Скорость записи", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(0, 2, 10, 2) };
            Grid.SetColumn(writeSpeedLabel, 1); writeSpeedGrid.Children.Add(writeSpeedLabel);

            Grid.SetRow(writeSpeedGrid, 2); Grid.SetColumn(writeSpeedGrid, 1); leftGrid.Children.Add(writeSpeedGrid);

            TextBlock readSpeedValue = new TextBlock { Text = "0 КБ/с", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(5, 0, 10, 2) };
            Grid.SetRow(readSpeedValue, 3); Grid.SetColumn(readSpeedValue, 0); leftGrid.Children.Add(readSpeedValue);

            TextBlock writeSpeedValue = new TextBlock { Text = "0 КБ/с", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(5, 0, 10, 2) };
            Grid.SetRow(writeSpeedValue, 3); Grid.SetColumn(writeSpeedValue, 1); leftGrid.Children.Add(writeSpeedValue);

            Grid.SetColumn(leftGrid, 0);
            dataGrid.Children.Add(leftGrid);

            StackPanel rightPanel = new StackPanel { Margin = new Thickness(0, 5, 5, 5) };
            rightPanel.Children.Add(ChartHelper.CreateDataRow("Ёмкость:", diskDrive != null ? $"{Math.Round(diskDrive.TotalSize / 1024.0 / 1024 / 1024, 1)} ГБ" : "N/A", true, 12));
            rightPanel.Children.Add(ChartHelper.CreateDataRow("Формат:", diskDrive != null ? diskDrive.DriveFormat : "N/A", true, 12));
            rightPanel.Children.Add(ChartHelper.CreateDataRow("Системный диск:", IsSystemDisk() ? "Да" : "Нет", true, 12));
            rightPanel.Children.Add(ChartHelper.CreateDataRow("Файл подкачки:", HasPageFile() ? "Да" : "Нет", true, 12));
            Grid.SetColumn(rightPanel, 1);
            dataGrid.Children.Add(rightPanel);

            Grid.SetRow(dataGrid, 3);
            mainGrid.Children.Add(dataGrid);

            tab.Content = mainGrid;
            return tab;
        }

        public async Task Update()
        {
            if (Tab == null) return;

            double readSpeedKB = Math.Round(diskReadCounter.NextValue() / 1024, 1);
            double writeSpeedKB = Math.Round(diskWriteCounter.NextValue() / 1024, 1);
            double activeTime = Math.Round(diskTimeCounter.NextValue(), 1);

            for (int i = 0; i < DiskReadSpeedValues.Count - 1; i++)
            {
                DiskReadSpeedValues[i] = DiskReadSpeedValues[i + 1];
                DiskWriteSpeedValues[i] = DiskWriteSpeedValues[i + 1];
            }
            DiskReadSpeedValues[59] = readSpeedKB;
            DiskWriteSpeedValues[59] = writeSpeedKB;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var mainGrid = Tab.Content as Grid;
                if (mainGrid == null) return;

                var transferRateChart = mainGrid.Children.OfType<CartesianChart>().FirstOrDefault(c => Grid.GetRow(c) == 2);
                if (transferRateChart != null)
                {
                    transferRateChart.Series[0].Values = DiskReadSpeedValues;
                    transferRateChart.Series[1].Values = DiskWriteSpeedValues;

                    double maxSpeedKB = Math.Max(DiskReadSpeedValues.Max(), DiskWriteSpeedValues.Max());
                    double newMax = Math.Max(100, Math.Ceiling(maxSpeedKB / 100) * 100);
                    if (transferRateChart.AxisY[0].MaxValue != newMax)
                    {
                        transferRateChart.AxisY[0].MaxValue = newMax;
                        transferRateChart.AxisY[0].LabelFormatter = value => value < 1024 ? $"{value:F1} КБ/с" : $"{(value / 1024):F1} МБ/с";
                    }
                    transferRateChart.Update(true, true);
                }

                var dataGrid = mainGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == 3);
                if (dataGrid != null)
                {
                    var leftGrid = dataGrid.Children.OfType<Grid>().FirstOrDefault(sp => Grid.GetColumn(sp) == 0);
                    if (leftGrid != null)
                    {
                        var activeTimeValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 1 && Grid.GetColumn(tb) == 0);
                        if (activeTimeValue != null) activeTimeValue.Text = $"{activeTime} %";

                        var readSpeedValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 3 && Grid.GetColumn(tb) == 0);
                        if (readSpeedValue != null) readSpeedValue.Text = readSpeedKB < 1024 ? $"{readSpeedKB:F1} КБ/с" : $"{(readSpeedKB / 1024):F1} МБ/с";

                        var writeSpeedValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 3 && Grid.GetColumn(tb) == 1);
                        if (writeSpeedValue != null) writeSpeedValue.Text = writeSpeedKB < 1024 ? $"{writeSpeedKB:F1} КБ/с" : $"{(writeSpeedKB / 1024):F1} МБ/с";
                    }
                }
            });
        }

        private string GetDiskModel()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string deviceId = obj["DeviceID"]?.ToString();
                        if (deviceId != null && diskName.Contains(obj["Index"]?.ToString()))
                        {
                            return obj["Model"]?.ToString() ?? "Unknown";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения модели диска: {ex.Message}");
            }
            return "Unknown";
        }

        private bool IsSystemDisk()
        {
            string systemDrive = System.IO.Path.GetPathRoot(Environment.SystemDirectory).TrimEnd('\\');
            return diskName.Contains(systemDrive[0].ToString());
        }

        private bool HasPageFile()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string autoPageFile = obj.Properties["AutomaticManagedPagefile"]?.Value?.ToString();
                        string pageFileName = obj.Properties["PageFileName"]?.Value?.ToString();

                        if (string.IsNullOrEmpty(pageFileName) && string.IsNullOrEmpty(autoPageFile))
                        {
                            return false;
                        }

                        string pageFileDrive = autoPageFile == "True"
                            ? Environment.SystemDirectory[0].ToString()
                            : pageFileName?.Substring(0, 1);

                        return pageFileDrive != null && diskName.Contains(pageFileDrive);
                    }
                }
            }
            catch (ManagementException ex)
            {
                Console.WriteLine($"Ошибка WMI при проверке файла подкачки: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неизвестная ошибка при проверке файла подкачки: {ex.Message}");
            }
            return false;
        }

        public string DiskName => diskName;

        public void Dispose()
        {
            diskReadCounter?.Dispose();
            diskWriteCounter?.Dispose();
            diskTimeCounter?.Dispose();
        }
    }
}