using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Threading;

namespace Computer_Status_Viewer
{
    public class MemoryManager
    {
        private readonly ResourceDictionary resources;
        private ScrollViewer cachedMemoryPanel;
        private DispatcherTimer updateTimer;
        private ListBox externalMemoryListBox;
        private ListBox virtualMemoryListBox;

        public MemoryManager(ResourceDictionary resources)
        {
            this.resources = resources;

            updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            updateTimer.Tick += UpdateTimer_Tick;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (externalMemoryListBox != null)
            {
                var externalMemoryParameters = GetExternalMemoryProperties();
                UpdateListBox(externalMemoryListBox, externalMemoryParameters, "Внешняя память");
            }

            if (virtualMemoryListBox != null)
            {
                var virtualMemoryParameters = GetVirtualMemoryProperties();
                UpdateListBox(virtualMemoryListBox, virtualMemoryParameters, "Виртуальная память");
            }
        }

        private void UpdateListBox(ListBox listBox, List<SystemParameter> parameters, string category)
        {
            listBox.Items.Clear();
            foreach (var param in parameters)
            {
                StackPanel itemPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                string paramIconPath = GetIconForParameter(param.Field, category);
                if (!string.IsNullOrEmpty(paramIconPath))
                {
                    itemPanel.Children.Add(new Image
                    {
                        Source = new BitmapImage(new Uri(paramIconPath, UriKind.Relative)),
                        Width = 24,
                        Height = 24,
                        Margin = new Thickness(0, 2, 8, 2),
                        VerticalAlignment = VerticalAlignment.Center
                    });
                }

                itemPanel.Children.Add(new TextBlock
                {
                    Text = param.Field,
                    Margin = new Thickness(0, 2, 10, 2),
                    MinWidth = 200,
                    FontSize = 13,
                    HorizontalAlignment = HorizontalAlignment.Left
                });

                itemPanel.Children.Add(new TextBlock
                {
                    Text = param.Value,
                    Margin = new Thickness(0, 2, 0, 2),
                    FontSize = 13,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                });

                listBox.Items.Add(itemPanel);
            }
        }

        public UIElement CreateMemoryPanel()
        {
            if (cachedMemoryPanel != null)
            {
                return cachedMemoryPanel;
            }

            ListView listView = new ListView
            {
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                SelectionMode = SelectionMode.Single
            };

            Style listViewItemStyle = new Style(typeof(ListViewItem));
            listViewItemStyle.Setters.Add(new Setter(ListViewItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
            listViewItemStyle.Setters.Add(new Setter(ListViewItem.BackgroundProperty, Brushes.White));
            listViewItemStyle.Setters.Add(new Setter(ListViewItem.BorderThicknessProperty, new Thickness(0)));
            listView.ItemContainerStyle = listViewItemStyle;

            ScrollViewer scrollViewer = new ScrollViewer
            {
                Content = listView,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                PanningMode = PanningMode.VerticalOnly,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;

            var externalMemorySection = CreateCategorySection("Внешняя память", GetExternalMemoryProperties());
            listView.Items.Add(externalMemorySection);
            externalMemoryListBox = FindListBoxInSection(externalMemorySection);

            var virtualMemorySection = CreateCategorySection("Виртуальная память", GetVirtualMemoryProperties());
            listView.Items.Add(virtualMemorySection);
            virtualMemoryListBox = FindListBoxInSection(virtualMemorySection);

            listView.Items.Add(CreateCategorySection("Внутренняя память", GetInternalMemoryProperties()));
            listView.Items.Add(CreateCategorySection("Файлы подкачки", GetPageFileProperties()));
            listView.Items.Add(CreateCategorySection("Physical Address Extension (PAE)", GetPAEProperties()));

            cachedMemoryPanel = scrollViewer;

            updateTimer.Start();

            return cachedMemoryPanel;
        }

        private ListBox FindListBoxInSection(UIElement section)
        {
            if (section is StackPanel stackPanel)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is ListBox listBox)
                    {
                        return listBox;
                    }
                }
            }
            return null;
        }

        private List<SystemParameter> GetVirtualMemoryProperties()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            try
            {
                long totalVirtualMemory = 0;
                long freeVirtualMemory = 0;

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        totalVirtualMemory = Convert.ToInt64(obj["TotalVirtualMemorySize"]) * 1024;
                        freeVirtualMemory = Convert.ToInt64(obj["FreeVirtualMemory"]) * 1024;
                    }
                }

                long usedVirtualMemory = totalVirtualMemory - freeVirtualMemory;
                double totalVirtualMemoryMB = totalVirtualMemory / (1024.0 * 1024.0);
                double usedVirtualMemoryMB = usedVirtualMemory / (1024.0 * 1024.0);
                double freeVirtualMemoryMB = freeVirtualMemory / (1024.0 * 1024.0);
                double virtualMemoryUsagePercent = totalVirtualMemory > 0 ? (usedVirtualMemoryMB / totalVirtualMemoryMB) * 100 : 0;

                parameters.Add(new SystemParameter { Field = "Всего", Value = $"{totalVirtualMemoryMB:F2} МБ" });
                parameters.Add(new SystemParameter { Field = "Свободно", Value = $"{freeVirtualMemoryMB:F2} МБ" });
                parameters.Add(new SystemParameter { Field = "Занято", Value = $"{usedVirtualMemoryMB:F2} МБ" });
                parameters.Add(new SystemParameter { Field = "Загрузка", Value = $"{virtualMemoryUsagePercent:F2}%" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных о виртуальной памяти: {ex.Message}");
                parameters.Add(new SystemParameter { Field = "Всего", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Свободно", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Занято", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Загрузка", Value = "N/A" });
            }

            return parameters;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            double scrollStep = 40;

            double newOffset = scrollViewer.VerticalOffset - (e.Delta > 0 ? scrollStep : -scrollStep);

            if (newOffset < 0) newOffset = 0;
            if (newOffset > scrollViewer.ScrollableHeight) newOffset = scrollViewer.ScrollableHeight;

            scrollViewer.ScrollToVerticalOffset(newOffset);
            e.Handled = true;
        }

        private UIElement CreateCategorySection(string categoryName, List<SystemParameter> parameters)
        {
            StackPanel sectionPanel = new StackPanel
            {
                Margin = new Thickness(0),
                Background = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            StackPanel headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            Border headerBorder = new Border
            {
                Child = headerPanel,
                Padding = new Thickness(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                CornerRadius = new CornerRadius(5),
                Background = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(70, 130, 180), 0.0),
                        new GradientStop(Color.FromRgb(0, 102, 204), 1.0)
                    }
                },
                Effect = new DropShadowEffect
                {
                    ShadowDepth = 2,
                    BlurRadius = 5,
                    Opacity = 0.3,
                    Color = Colors.Black
                }
            };

            string categoryIconPath = GetIconForCategory(categoryName);
            if (!string.IsNullOrEmpty(categoryIconPath))
            {
                headerPanel.Children.Add(new Image
                {
                    Source = new BitmapImage(new Uri(categoryIconPath, UriKind.Relative)),
                    Width = 27,
                    Height = 27,
                    Margin = new Thickness(0, 0, 8, 0)
                });
            }

            headerPanel.Children.Add(new TextBlock
            {
                Text = categoryName,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                FontFamily = new FontFamily("Segoe UI"),
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            });

            sectionPanel.Children.Add(headerBorder);

            ListBox listBox = new ListBox
            {
                Margin = new Thickness(8),
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                SelectionMode = SelectionMode.Single,
                Background = Brushes.Transparent
            };

            Style listBoxItemStyle = new Style(typeof(ListBoxItem));
            listBoxItemStyle.Setters.Add(new Setter(ListBoxItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
            listBoxItemStyle.Setters.Add(new Setter(ListBoxItem.BorderThicknessProperty, new Thickness(0, 0, 0, 1)));
            listBoxItemStyle.Setters.Add(new Setter(ListBoxItem.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(230, 230, 230))));
            listBoxItemStyle.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(5)));

            listBox.ItemContainerStyleSelector = new AlternatingRowStyleSelector();

            listBoxItemStyle.Triggers.Add(new Trigger
            {
                Property = ListBoxItem.IsMouseOverProperty,
                Value = true,
                Setters = { new Setter(ListBoxItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(240, 248, 255))) }
            });
            listBoxItemStyle.Triggers.Add(new Trigger
            {
                Property = ListBoxItem.IsSelectedProperty,
                Value = true,
                Setters = {
                    new Setter(ListBoxItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(173, 216, 230))),
                    new Setter(ListBoxItem.ForegroundProperty, Brushes.Black)
                }
            });

            listBox.ItemContainerStyle = listBoxItemStyle;

            UpdateListBox(listBox, parameters, categoryName);

            sectionPanel.Children.Add(listBox);
            return sectionPanel;
        }

        private class AlternatingRowStyleSelector : StyleSelector
        {
            public override Style SelectStyle(object item, DependencyObject container)
            {
                var style = new Style(typeof(ListBoxItem));
                style.Setters.Add(new Setter(ListBoxItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
                style.Setters.Add(new Setter(ListBoxItem.BorderThicknessProperty, new Thickness(0, 0, 0, 1)));
                style.Setters.Add(new Setter(ListBoxItem.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(230, 230, 230))));
                style.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(5)));

                var listBox = ItemsControl.ItemsControlFromItemContainer(container) as ListBox;
                if (listBox != null)
                {
                    int index = listBox.ItemContainerGenerator.IndexFromContainer(container);
                    style.Setters.Add(new Setter(ListBoxItem.BackgroundProperty,
                        index % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(250, 250, 250))));
                }

                style.Triggers.Add(new Trigger
                {
                    Property = ListBoxItem.IsMouseOverProperty,
                    Value = true,
                    Setters = { new Setter(ListBoxItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(240, 248, 255))) }
                });
                style.Triggers.Add(new Trigger
                {
                    Property = ListBoxItem.IsSelectedProperty,
                    Value = true,
                    Setters = {
                        new Setter(ListBoxItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(173, 216, 230))),
                        new Setter(ListBoxItem.ForegroundProperty, Brushes.Black)
                    }
                });

                return style;
            }
        }

        private string GetIconForCategory(string category)
        {
            switch (category)
            {
                case "Внешняя память":
                    return "Ico/Memory.png";
                case "Виртуальная память":
                    return "Ico/OS.png";
                case "Внутренняя память":
                    return "Ico/Memory.png";
                case "Файлы подкачки":
                    return "Ico/HardDisk.png";
                case "Physical Address Extension (PAE)":
                    return "Ico/Memory.png";
                default:
                    return "Ico/Reserve.png";
            }
        }

        private string GetIconForParameter(string field, string category)
        {
            if (category == "Виртуальная память")
            {
                switch (field)
                {
                    case "Всего":
                        return "Ico/OS.png";
                    case "Свободно":
                        return "Ico/OS.png";
                    case "Занято":
                        return "Ico/OS.png";
                    case "Загрузка":
                        return "Ico/OS.png";
                }
            }
            else if (category == "Внешняя память")
            {
                switch (field)
                {
                    case "Всего":
                        return "Ico/Memory.png";
                    case "Свободно":
                        return "Ico/Memory.png";
                    case "Занято":
                        return "Ico/Memory.png";
                    case "Загрузка":
                        return "Ico/Memory.png";
                }
            }

            switch (field)
            {
                case "Количество модулей":
                    return "Ico/Memory.png";
                case "Файл подкачки":
                    return "Ico/HardDisk.png";
                case "Текущий размер":
                    return "Ico/HardDisk.png";
                case "Максимальный размер":
                    return "Ico/HardDisk.png";
                case "Текущая/пиковая загрузка":
                    return "Ico/HardDisk.png";
                case "Поддерживается ОС":
                    return "Ico/OS.png";
                case "Поддерживается ЦП":
                    return "Ico/CPU.png";
                case "Активный":
                    return "Ico/Memory.png";
            }

            string baseField = field.Split(' ')[0];
            switch (baseField)
            {
                case "Модуль":
                    return "Ico/Module.png";
                default:
                    return "Ico/Reserve.png";
            }
        }

        private string GetWmiProperty(string wmiClass, string wmiProperty, string condition = null)
        {
            try
            {
                string query = $"SELECT {wmiProperty} FROM {wmiClass}";
                if (!string.IsNullOrEmpty(condition))
                {
                    query += $" WHERE {condition}";
                }

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj[wmiProperty]?.ToString() ?? "N/A";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении WMI свойства {wmiProperty} из {wmiClass}: {ex.Message}");
                return "N/A";
            }
            return "N/A";
        }

        private List<SystemParameter> GetExternalMemoryProperties()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            try
            {
                long totalMemory = 0;
                long usedMemory = 0;

                string totalPhysicalMemory = GetWmiProperty("Win32_ComputerSystem", "TotalPhysicalMemory");
                if (totalPhysicalMemory != "N/A")
                {
                    totalMemory = Convert.ToInt64(totalPhysicalMemory);
                }

                string freePhysicalMemory = GetWmiProperty("Win32_OperatingSystem", "FreePhysicalMemory");
                if (freePhysicalMemory != "N/A")
                {
                    long freeMemoryBytes = Convert.ToInt64(freePhysicalMemory) * 1024;
                    usedMemory = totalMemory - freeMemoryBytes;
                }

                double totalMemoryMB = totalMemory / (1024.0 * 1024.0);
                double usedMemoryMB = usedMemory / (1024.0 * 1024.0);
                double freeMemoryMB = totalMemoryMB - usedMemoryMB;
                double memoryUsagePercent = totalMemory > 0 ? (usedMemoryMB / totalMemoryMB) * 100 : 0;

                parameters.Add(new SystemParameter { Field = "Всего", Value = $"{totalMemoryMB:F2} МБ" });
                parameters.Add(new SystemParameter { Field = "Свободно", Value = $"{freeMemoryMB:F2} МБ" });
                parameters.Add(new SystemParameter { Field = "Занято", Value = $"{usedMemoryMB:F2} МБ" });
                parameters.Add(new SystemParameter { Field = "Загрузка", Value = $"{memoryUsagePercent:F2}%" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных о внешней памяти: {ex.Message}");
                parameters.Add(new SystemParameter { Field = "Всего", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Свободно", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Занято", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Загрузка", Value = "N/A" });
            }

            return parameters;
        }

        private List<SystemParameter> GetInternalMemoryProperties()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    int memoryCount = searcher.Get().Count;
                    parameters.Add(new SystemParameter { Field = "Количество модулей", Value = memoryCount.ToString() });

                    int index = 1;
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string bankLabel = obj["BankLabel"]?.ToString() ?? "N/A";
                        string capacity = (Convert.ToInt64(obj["Capacity"]) / (1024.0 * 1024.0)).ToString("F2") + " МБ";
                        string speed = obj["Speed"]?.ToString() + " МГц" ?? "N/A";
                        string manufacturer = obj["Manufacturer"]?.ToString() ?? "N/A";

                        parameters.Add(new SystemParameter { Field = $"Модуль {index} - Банк", Value = bankLabel });
                        parameters.Add(new SystemParameter { Field = $"Модуль {index} - Объем", Value = capacity });
                        parameters.Add(new SystemParameter { Field = $"Модуль {index} - Скорость", Value = speed });
                        parameters.Add(new SystemParameter { Field = $"Модуль {index} - Производитель", Value = manufacturer });
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных о внутренней памяти: {ex.Message}");
                parameters.Add(new SystemParameter { Field = "Количество модулей", Value = "N/A" });
            }

            return parameters;
        }

        private List<SystemParameter> GetPageFileProperties()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            try
            {
                bool pageFileEnabled = false;
                string autoManageStatus = "Неизвестно";
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        bool? autoManage = obj["AutomaticManagedPagefile"] as bool?;
                        if (autoManage.HasValue)
                        {
                            autoManageStatus = autoManage.Value ? "Да" : "Нет";
                            pageFileEnabled = autoManage.Value;
                        }
                        else
                        {
                            autoManageStatus = "N/A";
                        }
                    }
                }

                Console.WriteLine($"Автоматическое управление файлом подкачки: {autoManageStatus}");

                if (!pageFileEnabled)
                {
                    using (ManagementObjectSearcher pageFileSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PageFileSetting"))
                    {
                        var pageFileSettings = pageFileSearcher.Get();
                        Console.WriteLine($"Найдено настроек файла подкачки: {pageFileSettings.Count}");
                        if (pageFileSettings.Count > 0)
                        {
                            pageFileEnabled = true;
                        }
                    }
                }

                if (!pageFileEnabled)
                {
                    Console.WriteLine("Файл подкачки отключен.");
                    parameters.Add(new SystemParameter { Field = "Файл подкачки", Value = "Отключен" });
                    parameters.Add(new SystemParameter { Field = "Текущий размер", Value = "N/A" });
                    parameters.Add(new SystemParameter { Field = "Максимальный размер", Value = "N/A" });
                    parameters.Add(new SystemParameter { Field = "Текущая/пиковая загрузка", Value = "N/A" });
                    return parameters;
                }

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PageFileUsage"))
                {
                    var pageFiles = searcher.Get();
                    Console.WriteLine($"Найдено файлов подкачки: {pageFiles.Count}");
                    if (pageFiles.Count == 0)
                    {
                        parameters.Add(new SystemParameter { Field = "Файл подкачки", Value = "Не найден" });
                        parameters.Add(new SystemParameter { Field = "Текущий размер", Value = "N/A" });
                        parameters.Add(new SystemParameter { Field = "Максимальный размер", Value = "N/A" });
                        parameters.Add(new SystemParameter { Field = "Текущая/пиковая загрузка", Value = "N/A" });
                        return parameters;
                    }

                    foreach (ManagementObject obj in pageFiles)
                    {
                        string name = obj["Name"]?.ToString() ?? "N/A";
                        uint currentUsageMB = Convert.ToUInt32(obj["CurrentUsage"] ?? 0);
                        uint peakUsageMB = Convert.ToUInt32(obj["PeakUsage"] ?? 0);

                        Console.WriteLine($"Файл подкачки: {name}, Текущий размер: {currentUsageMB} МБ, Пиковый размер: {peakUsageMB} МБ");

                        string maxSizeMB = "Система управляет";
                        if (autoManageStatus != "Да")
                        {
                            using (ManagementObjectSearcher settingSearcher = new ManagementObjectSearcher($"SELECT * FROM Win32_PageFileSetting WHERE Name = '{name.Replace("\\", "\\\\")}'"))
                            {
                                foreach (ManagementObject setting in settingSearcher.Get())
                                {
                                    uint maxSize = Convert.ToUInt32(setting["MaximumSize"] ?? 0);
                                    maxSizeMB = maxSize > 0 ? $"{maxSize} МБ" : "N/A";
                                    Console.WriteLine($"Максимальный размер файла подкачки ({name}): {maxSizeMB}");
                                }
                            }
                        }

                        parameters.Add(new SystemParameter { Field = "Файл подкачки", Value = name });
                        parameters.Add(new SystemParameter { Field = "Текущий размер", Value = $"{currentUsageMB} МБ" });
                        parameters.Add(new SystemParameter { Field = "Максимальный размер", Value = maxSizeMB });
                        parameters.Add(new SystemParameter { Field = "Текущая/пиковая загрузка", Value = $"{currentUsageMB}/{peakUsageMB} МБ" });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных о файлах подкачки: {ex.Message}");
                parameters.Add(new SystemParameter { Field = "Файл подкачки", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Текущий размер", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Максимальный размер", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Текущая/пиковая загрузка", Value = "N/A" });
            }

            return parameters;
        }

        private List<SystemParameter> GetPAEProperties()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            try
            {
                string osPAESupport = "N/A";
                bool is64BitOS = Environment.Is64BitOperatingSystem;
                if (is64BitOS)
                {
                    osPAESupport = "Да";
                }
                else
                {
                    string paeEnabled = GetWmiProperty("Win32_OperatingSystem", "PAEEnabled");
                    osPAESupport = paeEnabled != "N/A" ? (paeEnabled == "True" ? "Да" : "Нет") : "N/A";
                }

                string cpuPAESupport = "N/A";
                string architecture = GetWmiProperty("Win32_Processor", "Architecture");
                if (architecture != "N/A")
                {
                    ushort archValue = Convert.ToUInt16(architecture);
                    if (archValue == 9)
                    {
                        cpuPAESupport = "Да";
                    }
                    else if (archValue == 0)
                    {
                        string addressWidth = GetWmiProperty("Win32_Processor", "AddressWidth");
                        string dataWidth = GetWmiProperty("Win32_Processor", "DataWidth");
                        if (addressWidth != "N/A" && dataWidth != "N/A")
                        {
                            uint addrWidth = Convert.ToUInt32(addressWidth);
                            uint dWidth = Convert.ToUInt32(dataWidth);
                            cpuPAESupport = (addrWidth >= 36 || dWidth >= 36) ? "Да" : "Нет";
                        }
                    }
                }

                string paeActive = "N/A";
                if (is64BitOS)
                {
                    paeActive = "Да";
                }
                else
                {
                    string paeEnabled = GetWmiProperty("Win32_OperatingSystem", "PAEEnabled");
                    if (paeEnabled != "N/A" && paeEnabled == "True")
                    {
                        paeActive = "Да";
                    }
                    else
                    {
                        try
                        {
                            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"))
                            {
                                if (key != null)
                                {
                                    object disablePAE = key.GetValue("DisablePAE");
                                    if (disablePAE != null && Convert.ToInt32(disablePAE) == 1)
                                    {
                                        paeActive = "Нет";
                                    }
                                    else
                                    {
                                        paeActive = cpuPAESupport == "Да" ? "Да" : "Нет";
                                    }
                                }
                                else
                                {
                                    paeActive = cpuPAESupport == "Да" ? "Да" : "Нет";
                                }
                            }
                        }
                        catch
                        {
                            paeActive = cpuPAESupport == "Да" ? "Да" : "Нет";
                        }
                    }
                }

                parameters.Add(new SystemParameter { Field = "Поддерживается ОС", Value = osPAESupport });
                parameters.Add(new SystemParameter { Field = "Поддерживается ЦП", Value = cpuPAESupport });
                parameters.Add(new SystemParameter { Field = "Активный", Value = paeActive });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных о PAE: {ex.Message}");
                parameters.Add(new SystemParameter { Field = "Поддерживается ОС", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Поддерживается ЦП", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Активный", Value = "N/A" });
            }

            return parameters;
        }

        public class SystemParameter
        {
            public string Field { get; set; }
            public string Value { get; set; }
        }

        public void Dispose()
        {
            if (updateTimer != null && updateTimer.IsEnabled)
            {
                updateTimer.Stop();
            }
        }
    }
}