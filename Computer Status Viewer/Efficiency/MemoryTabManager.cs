using System;
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
    public class MemoryTabManager : IDisposable
    {
        public TabItem Tab { get; private set; }
        public ChartValues<double> MemoryValues { get; } = new ChartValues<double>(Enumerable.Repeat(0.0, 60));

        public MemoryTabManager()
        {
            Tab = InitializeMemoryTab();
        }

        public void WarmUpCounter()
        {
            GetTotalMemory();
        }

        private TabItem InitializeMemoryTab()
        {
            TabItem tab = new TabItem { Header = "Память" }; // Простая строка для заголовка вкладки
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
                Text = "Память",
                Foreground = Brushes.Black,
                FontSize = 18,
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 5, 10, 0)
            };
            Grid.SetColumn(titleBlock, 0);
            headerGrid.Children.Add(titleBlock);

            double totalMemory = GetTotalMemory();
            TextBlock memoryTotalBlock = new TextBlock
            {
                Text = $"{totalMemory} ГБ",
                Foreground = Brushes.Black,
                FontSize = 18,
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 10, 2)
            };
            Grid.SetColumn(memoryTotalBlock, 2);
            headerGrid.Children.Add(memoryTotalBlock);

            Grid.SetRow(headerGrid, 0);
            mainGrid.Children.Add(headerGrid);

            TextBlock usageLabel = new TextBlock { Text = "Использование памяти", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = new SolidColorBrush(Color.FromArgb(150, 105, 105, 105)), Margin = new Thickness(5, 5, 0, 2) };
            Grid.SetRow(usageLabel, 1);
            mainGrid.Children.Add(usageLabel);

            var chart = ChartHelper.SetupChart(MemoryValues, Brushes.Green, "Использование (%)");
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

            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            SolidColorBrush labelColor = new SolidColorBrush(Color.FromRgb(105, 105, 105));
            SolidColorBrush valueColor = Brushes.Black;

            TextBlock usedLabel = new TextBlock { Text = "Используется", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(5, 2, 10, 2) };
            Grid.SetRow(usedLabel, 0); Grid.SetColumn(usedLabel, 0); leftGrid.Children.Add(usedLabel);

            TextBlock availableLabel = new TextBlock { Text = "Доступно", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(0, 2, 10, 2) };
            Grid.SetRow(availableLabel, 0); Grid.SetColumn(availableLabel, 1); leftGrid.Children.Add(availableLabel);

            TextBlock usedValue = new TextBlock { Text = "0 ГБ (0 %)", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(5, 0, 10, 2) };
            Grid.SetRow(usedValue, 1); Grid.SetColumn(usedValue, 0); leftGrid.Children.Add(usedValue);

            TextBlock availableValue = new TextBlock { Text = "0 ГБ", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(0, 0, 10, 2) };
            Grid.SetRow(availableValue, 1); Grid.SetColumn(availableValue, 1); leftGrid.Children.Add(availableValue);

            TextBlock committedLabel = new TextBlock { Text = "Выделено", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(5, 2, 10, 2) };
            Grid.SetRow(committedLabel, 2); Grid.SetColumn(committedLabel, 0); leftGrid.Children.Add(committedLabel);

            TextBlock committedValue = new TextBlock { Text = "0/0 ГБ", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(5, 0, 10, 2) };
            Grid.SetRow(committedValue, 3); Grid.SetColumn(committedValue, 0); leftGrid.Children.Add(committedValue);

            Grid.SetColumn(leftGrid, 0);
            dataGrid.Children.Add(leftGrid);

            StackPanel rightPanel = new StackPanel { Margin = new Thickness(0, 5, 5, 5) };
            rightPanel.Children.Add(ChartHelper.CreateDataRow("Скорость:", $"{GetMemorySpeed()} МТ/с", true));
            rightPanel.Children.Add(ChartHelper.CreateDataRow("Форм-фактор:", GetFormFactor(), true));
            Grid.SetColumn(rightPanel, 1);
            dataGrid.Children.Add(rightPanel);

            Grid.SetRow(dataGrid, 3);
            mainGrid.Children.Add(dataGrid);

            tab.Content = mainGrid;
            return tab;
        }

        public async Task Update()
        {
            double total = GetTotalMemory();
            double available = GetAvailableMemory();
            double used = total - available;
            double committed = GetCommittedMemory();
            double totalCommittedLimit = GetTotalCommittedLimit();

            for (int i = 0; i < MemoryValues.Count - 1; i++)
            {
                MemoryValues[i] = MemoryValues[i + 1];
            }
            MemoryValues[59] = used / total * 100;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var mainGrid = Tab.Content as Grid;
                if (mainGrid == null) return;

                var chart = mainGrid.Children.OfType<CartesianChart>().FirstOrDefault(c => Grid.GetRow(c) == 2);
                if (chart != null)
                {
                    chart.Series[0].Values = MemoryValues;
                    chart.Update(false, true);
                }

                var dataGrid = mainGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == 3);
                if (dataGrid != null)
                {
                    var leftGrid = dataGrid.Children.OfType<Grid>().FirstOrDefault(sp => Grid.GetColumn(sp) == 0);
                    if (leftGrid != null)
                    {
                        var usedValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 1 && Grid.GetColumn(tb) == 0);
                        if (usedValue != null) usedValue.Text = $"{used} ГБ ({(used / total * 100):F1} %)";

                        var availableValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 1 && Grid.GetColumn(tb) == 1);
                        if (availableValue != null) availableValue.Text = $"{available} ГБ";

                        var committedValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 3 && Grid.GetColumn(tb) == 0);
                        if (committedValue != null) committedValue.Text = $"{committed}/{totalCommittedLimit} ГБ";
                    }
                }
            });
        }

        private double GetTotalMemory()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Math.Round(Convert.ToDouble(obj["TotalVisibleMemorySize"]) / 1024 / 1024, 1);
                }
            }
            return 0;
        }

        private double GetAvailableMemory()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Math.Round(Convert.ToDouble(obj["FreePhysicalMemory"]) / 1024 / 1024, 1);
                }
            }
            return 0;
        }

        private double GetCommittedMemory()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    double totalVirtual = Convert.ToDouble(obj["TotalVirtualMemorySize"]) / 1024 / 1024;
                    double freeVirtual = Convert.ToDouble(obj["FreeVirtualMemory"]) / 1024 / 1024;
                    return Math.Round(totalVirtual - freeVirtual, 1);
                }
            }
            return 0;
        }

        private double GetTotalCommittedLimit()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Math.Round(Convert.ToDouble(obj["TotalVirtualMemorySize"]) / 1024 / 1024, 1);
                }
            }
            return 0;
        }

        private int GetMemorySpeed()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Convert.ToInt32(obj["Speed"]);
                }
            }
            return 0;
        }

        private string GetFormFactor()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    int formFactor = Convert.ToInt32(obj["FormFactor"]);
                    return formFactor switch
                    {
                        8 => "DIMM",
                        12 => "SODIMM",
                        _ => "Unknown"
                    };
                }
            }
            return "Unknown";
        }

        public void Dispose()
        {
        }
    }
}