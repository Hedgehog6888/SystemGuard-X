using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Diagnostics; 

namespace Computer_Status_Viewer
{
    public class CPUManager
    {
        private readonly ResourceDictionary resources;
        private ScrollViewer cachedCPUPanel;
        private readonly Timer updateTimer;
        private readonly List<TextBlock> usageTextBlocks; 
        private readonly PerformanceCounter cpuCounter; 
        private readonly List<PerformanceCounter> coreCounters; 

        public CPUManager(ResourceDictionary resources)
        {
            this.resources = resources;
            usageTextBlocks = new List<TextBlock>();

            // Инициализация счетчиков производительности
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            coreCounters = new List<PerformanceCounter>();
            int coreCount = Environment.ProcessorCount;
            for (int i = 0; i < coreCount; i++)
            {
                coreCounters.Add(new PerformanceCounter("Processor", "% Processor Time", i.ToString()));
            }

            updateTimer = new Timer(1000); 
            updateTimer.Elapsed += UpdateTimerElapsed;
            updateTimer.AutoReset = true;
        }

        public UIElement CreateCPUPanel()
        {
            if (cachedCPUPanel != null)
            {
                StartUpdating();
                return cachedCPUPanel;
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

            // Добавляем секции на основе скриншота
            listView.Items.Add(CreateCategorySection("Свойства ЦП", GetCPUProperties()));
            listView.Items.Add(CreateCategorySection("Текущая производительность ЦП", GetCPUPerformance()));
            listView.Items.Add(CreateCategorySection("Загрузка ЦП", GetCPUUsage()));

            cachedCPUPanel = scrollViewer;
            StartUpdating();
            return cachedCPUPanel;
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

        private void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateCPUUsage();
        }

        private void UpdateCPUUsage()
        {
            var usageParameters = GetCPUUsage();
            if (usageTextBlocks.Count == usageParameters.Count)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < usageTextBlocks.Count; i++)
                    {
                        usageTextBlocks[i].Text = usageParameters[i].Value;
                    }
                });
            }
        }

        public void StartUpdating()
        {
            updateTimer.Start();
        }

        public void StopUpdating()
        {
            updateTimer.Stop();
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

                var valueTextBlock = new TextBlock
                {
                    Text = param.Value,
                    Margin = new Thickness(0, 2, 0, 2),
                    FontSize = 13,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                itemPanel.Children.Add(valueTextBlock);

                // Если это параметр загрузки ЦП, сохраняем TextBlock для динамического обновления
                if (categoryName == "Загрузка ЦП")
                {
                    usageTextBlocks.Add(valueTextBlock);
                }

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
                case "Свойства ЦП":
                case "Текущая производительность ЦП":
                case "Загрузка ЦП":
                    return "Ico/CPU.png";
                default:
                    return "Ico/Reserve.png";
            }
        }

        private string GetIconForParameter(string field)
        {
            // Для "Размер кристалла" и "Напряжение питания ядра" используем Напряжение.png
            if (field == "Размер кристалла" || field == "Напряжение питания ядра")
            {
                return "Ico/Voltage.png";
            }
            // Для всех остальных параметров используем CPU.png
            return "Ico/CPU.png";
        }

        public class SystemParameter
        {
            public string Field { get; set; }
            public string Value { get; set; }
        }

        private List<SystemParameter> GetCPUProperties()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            parameters.Add(new SystemParameter { Field = "Тип ЦП", Value = GetWmiProperty("Win32_Processor", "Name") });

            // Дополнительные свойства ЦП
            string cpuCores = GetWmiProperty("Win32_Processor", "NumberOfCores");
            string cpuThreads = GetWmiProperty("Win32_Processor", "NumberOfLogicalProcessors");
            parameters.Add(new SystemParameter { Field = "Ядер", Value = cpuCores });
            parameters.Add(new SystemParameter { Field = "Потоков", Value = cpuThreads });

            // Информация о кэше
            // Пробуем получить данные о кэше через Win32_CacheMemory, если Win32_Processor не возвращает
            string l1Cache = GetWmiProperty("Win32_Processor", "L1CacheSize");
            if (l1Cache == "N/A")
            {
                l1Cache = GetCacheSize("L1");
            }
            string l2Cache = GetWmiProperty("Win32_Processor", "L2CacheSize");
            if (l2Cache == "N/A")
            {
                l2Cache = GetCacheSize("L2");
            }
            string l3Cache = GetWmiProperty("Win32_Processor", "L3CacheSize");
            if (l3Cache == "N/A")
            {
                l3Cache = GetCacheSize("L3");
            }

            parameters.Add(new SystemParameter { Field = "Кэш L1 кода", Value = l1Cache != "N/A" ? l1Cache + " КБ" : "N/A" });
            parameters.Add(new SystemParameter { Field = "Кэш L1 данных", Value = l1Cache != "N/A" ? l1Cache + " КБ" : "N/A" });
            parameters.Add(new SystemParameter { Field = "Кэш L2", Value = l2Cache != "N/A" ? l2Cache + " КБ" : "N/A" });
            parameters.Add(new SystemParameter { Field = "Кэш L3", Value = l3Cache != "N/A" ? l3Cache + " МБ" : "N/A" });

            return parameters;
        }

        private List<SystemParameter> GetCPUPerformance()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            parameters.Add(new SystemParameter { Field = "Текущая скорость", Value = GetWmiProperty("Win32_Processor", "CurrentClockSpeed") + " МГц" });
            parameters.Add(new SystemParameter { Field = "Размеры корпуса", Value = "40 мм x 40 мм" }); // Статическое значение, как в скриншоте
            parameters.Add(new SystemParameter { Field = "Технологический процесс", Value = "12 нм, 14 нм CMOS, Cu" }); // Статическое значение, как в скриншоте
            parameters.Add(new SystemParameter { Field = "Размер кристалла", Value = "0.900 - 1.238 В" }); // Статическое значение, как в скриншоте
            parameters.Add(new SystemParameter { Field = "Напряжение питания ядра", Value = "65 Вт" }); // Статическое значение, как в скриншоте

            return parameters;
        }

        private List<SystemParameter> GetCPUUsage()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            // Общая загрузка ЦП
            float overallUsage = cpuCounter.NextValue();
            parameters.Add(new SystemParameter { Field = "ЦП 1", Value = Math.Round(overallUsage).ToString() + "%" });

            // Загрузка по ядрам
            for (int i = 0; i < coreCounters.Count; i++)
            {
                float coreUsage = coreCounters[i].NextValue();
                parameters.Add(new SystemParameter { Field = $"Ядро {i + 1} / SMT 1", Value = Math.Round(coreUsage).ToString() + "%" });
            }

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

        private string GetCacheSize(string cacheLevel)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_CacheMemory WHERE CacheType=3 AND Level='{cacheLevel}'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string size = obj["InstalledSize"]?.ToString();
                        if (!string.IsNullOrEmpty(size))
                        {
                            if (cacheLevel == "L3")
                            {
                                return (Convert.ToInt32(size) / 1024).ToString(); // Переводим в МБ для L3
                            }
                            return size; // Оставляем в КБ для L1 и L2
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении размера кэша {cacheLevel}: {ex.Message}");
            }
            return "N/A";
        }

        public void Dispose()
        {
            updateTimer?.Stop();
            updateTimer?.Dispose();
            if (cachedCPUPanel != null)
            {
                cachedCPUPanel.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
            }
            cpuCounter?.Dispose();
            foreach (var counter in coreCounters)
            {
                counter?.Dispose();
            }
        }
    }
}