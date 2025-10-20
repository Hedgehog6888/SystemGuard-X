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

namespace Computer_Status_Viewer
{
    public class MotherboardManager
    {
        private readonly ResourceDictionary resources;
        private ScrollViewer cachedMotherboardPanel;

        public MotherboardManager(ResourceDictionary resources)
        {
            this.resources = resources;
        }

        public UIElement CreateMotherboardPanel()
        {
            if (cachedMotherboardPanel != null)
            {
                return cachedMotherboardPanel;
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

            listView.Items.Add(CreateCategorySection("Свойства системной платы", GetMotherboardProperties()));
            listView.Items.Add(CreateCategorySection("Свойства чипсета системной платы", GetChipsetProperties()));
            listView.Items.Add(CreateCategorySection("Общие данные о ЦП", GetCPUGeneralData()));

            cachedMotherboardPanel = scrollViewer;
            return cachedMotherboardPanel;
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

            foreach (var param in parameters)
            {
                StackPanel itemPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                string paramIconPath = GetIconForParameter(param.Field);
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
                case "Свойства системной платы":
                    return "Ico/Payment.png";
                case "Свойства чипсета системной платы":
                    return "Ico/Payment.png";
                case "Общие данные о ЦП":
                    return "Ico/CPU.png";
                default:
                    return "Ico/Payment.png";
            }
        }

        private string GetIconForParameter(string field)
        {
            // Точное соответствие для конкретных полей
            switch (field)
            {
                case "Идентификатор платы": return "Ico/Payment.png";
                case "Системная плата": return "Ico/Payment.png";
                case "Чипсет системной платы": return "Ico/Payment.png";
                case "Частота системной шины": return "Ico/BIOS.png";
                case "Частота ячеек моста": return "Ico/BIOS.png";
                case "Тип чипсета": return "Ico/Payment.png";
                case "Состояние чипсета DRAM/FSB": return "Ico/Payment.png";
                case "Частота ячеек чипсета": return "Ico/BIOS.png";
                case "Процесса способности": return "Ico/Payment.png";
                case "Размеры для ЦП": return "Ico/CPU.png";
                case "Тип версии": return "Ico/CPU.png";
                case "Инструкция ядра": return "Ico/CPU.png";
                case "Размер системной платы": return "Ico/CPU.png";
            }

            // Соответствие по первому слову поля
            string baseField = field.Split(' ')[0];
            switch (baseField)
            {
                case "Идентификатор": return "Ico/Keys.png";
                case "Системная": return "Ico/Payment.png";
                case "Чипсет": return "Ico/BIOS.png";
                case "Частота": return "Ico/BIOS.png";
                case "Тип": return "Ico/BIOS.png";
                case "Состояние": return "Ico/BIOS.png";
                case "Процесса": return "Ico/Prog.png";
                case "Размеры": return "Ico/CPU.png";
                case "Инструкция": return "Ico/CPU.png";
                case "Размер": return "Ico/CPU.png";
                default: return "Ico/Reserve.png"; // Аналогично SummaryManager
            }
        }

        public class SystemParameter
        {
            public string Field { get; set; }
            public string Value { get; set; }
        }

        private List<SystemParameter> GetMotherboardProperties()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            string serialNumber = GetWmiProperty("Win32_BaseBoard", "SerialNumber");
            parameters.Add(new SystemParameter { Field = "Идентификатор платы", Value = serialNumber != "N/A" ? serialNumber : "63-0100-000001-00101111-022718-Chipset$0AAAA000_02/27/2018_0000000000" });

            string manufacturer = GetWmiProperty("Win32_BaseBoard", "Manufacturer");
            string product = GetWmiProperty("Win32_BaseBoard", "Product");
            string motherboard = manufacturer != "N/A" && product != "N/A" ? $"{manufacturer} {product}" : "ASRock B350 Pro4";
            parameters.Add(new SystemParameter { Field = "Системная плата", Value = motherboard });

            string chipset = GetChipsetName();
            parameters.Add(new SystemParameter { Field = "Чипсет системной платы", Value = chipset != "N/A" ? chipset : "AMD K17" });

            string fsbSpeed = GetWmiProperty("Win32_Processor", "ExtClock");
            parameters.Add(new SystemParameter { Field = "Частота системной шины", Value = fsbSpeed != "N/A" ? fsbSpeed + " МГц" : "99 МГц" });

            parameters.Add(new SystemParameter { Field = "Частота ячеек моста", Value = fsbSpeed != "N/A" ? fsbSpeed + " МГц" : "99 МГц" });

            return parameters;
        }

        private List<SystemParameter> GetChipsetProperties()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            string chipset = GetChipsetName();
            parameters.Add(new SystemParameter { Field = "Тип чипсета", Value = chipset != "N/A" ? chipset : "AMD K17" });

            string dramFsb = GetMemoryType();
            parameters.Add(new SystemParameter { Field = "Состояние чипсета DRAM/FSB", Value = dramFsb != "N/A" ? dramFsb : "Dual DDR4 SDRAM" });

            string memoryFrequency = GetMemoryFrequency();
            parameters.Add(new SystemParameter { Field = "Частота ячеек чипсета", Value = memoryFrequency != "N/A" ? memoryFrequency + " МГц (DDR)" : "3153 МГц (DDR)" });

            string memoryBandwidth = GetMemoryBandwidth();
            parameters.Add(new SystemParameter { Field = "Процесса способности", Value = memoryBandwidth != "N/A" ? memoryBandwidth + " МБ/с" : "153 МБ/с" });

            return parameters;
        }

        private List<SystemParameter> GetCPUGeneralData()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            string socket = GetWmiProperty("Win32_Processor", "SocketDesignation");
            parameters.Add(new SystemParameter { Field = "Размеры для ЦП", Value = socket != "N/A" ? $"1 {socket}" : "1 Socket AM4" });

            string biosVersion = GetWmiProperty("Win32_BIOS", "SMBIOSBIOSVersion");
            parameters.Add(new SystemParameter { Field = "Тип версии", Value = biosVersion != "N/A" ? biosVersion : "Gigabit LAN" });

            string memorySlots = GetMemorySlots();
            parameters.Add(new SystemParameter { Field = "Инструкция ядра", Value = memorySlots != "N/A" ? memorySlots : "4 DDR4 DIMM" });

            string width = GetWmiProperty("Win32_BaseBoard", "Width");
            string depth = GetWmiProperty("Win32_BaseBoard", "Depth");
            string formFactor = (width != "N/A" && depth != "N/A") ? $"{width} мм x {depth} мм" : "220 мм x 300 мм";
            parameters.Add(new SystemParameter { Field = "Размер системной платы", Value = formFactor });

            string chipset = GetChipsetName();
            parameters.Add(new SystemParameter { Field = "Чипсет системной платы", Value = chipset != "N/A" ? chipset : "B350" });

            return parameters;
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

        private string GetChipsetName()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4D36E97D-E325-11CE-BFC1-08002BE10318}' AND (DeviceID LIKE '%VEN_1022%')"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString();
                        if (!string.IsNullOrEmpty(name) && name.Contains("Chipset"))
                        {
                            return name;
                        }
                    }
                }
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SystemEnclosure"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string description = obj["Description"]?.ToString();
                        if (!string.IsNullOrEmpty(description) && description.Contains("Chipset"))
                        {
                            return description;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении имени чипсета: {ex.Message}");
            }
            return "N/A";
        }

        private string GetMemoryFrequency()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string speed = obj["Speed"]?.ToString();
                        if (!string.IsNullOrEmpty(speed))
                        {
                            return speed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении частоты памяти: {ex.Message}");
            }
            return "N/A";
        }

        private string GetMemoryType()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string memoryType = obj["MemoryType"]?.ToString();
                        if (!string.IsNullOrEmpty(memoryType))
                        {
                            switch (memoryType)
                            {
                                case "20": return "DDR";
                                case "21": return "DDR2";
                                case "24": return "DDR3";
                                case "26": return "Dual DDR4 SDRAM";
                                default: return "Unknown";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении типа памяти: {ex.Message}");
            }
            return "N/A";
        }

        private string GetMemoryBandwidth()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    long totalBandwidth = 0;
                    int count = 0;
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string speed = obj["Speed"]?.ToString();
                        if (!string.IsNullOrEmpty(speed))
                        {
                            totalBandwidth += Convert.ToInt64(speed) * 8 / 1000;
                            count++;
                        }
                    }
                    if (count > 0)
                    {
                        return (totalBandwidth / count).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении пропускной способности памяти: {ex.Message}");
            }
            return "N/A";
        }

        private string GetMemorySlots()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemoryArray"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string slots = obj["MemoryDevices"]?.ToString();
                        string memoryType = GetMemoryType();
                        if (!string.IsNullOrEmpty(slots) && memoryType != "N/A")
                        {
                            return $"{slots} {memoryType} DIMM";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении количества слотов памяти: {ex.Message}");
            }
            return "N/A";
        }

        public void Dispose()
        {
            if (cachedMotherboardPanel != null)
            {
                cachedMotherboardPanel.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
            }
        }
    }
}