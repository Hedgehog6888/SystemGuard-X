using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Management;

namespace Computer_Status_Viewer
{
    public class WorkTimeManager : IDisposable
    {
        private readonly ResourceDictionary resources;
        private ScrollViewer cachedWorkTimePanel;
        private TextBlock uptimeTextBlock;
        private TextBlock currentTimeTextBlock;
        private TextBlock totalUptimeTextBlock;
        private readonly Timer updateTimer;

        public WorkTimeManager(ResourceDictionary resources)
        {
            this.resources = resources;
            updateTimer = new Timer(1000);
            updateTimer.Elapsed += UpdateTimerElapsed;
            updateTimer.AutoReset = true;
        }

        public UIElement CreateWorkTimePanel()
        {
            if (cachedWorkTimePanel != null)
            {
                UpdateDynamicFields();
                StartUpdating();
                return cachedWorkTimePanel;
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
            listViewItemStyle.Triggers.Add(new Trigger
            {
                Property = ListViewItem.IsMouseOverProperty,
                Value = true,
                Setters = { new Setter(ListViewItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(240, 248, 255))) } // AliceBlue
            });
            listViewItemStyle.Triggers.Add(new Trigger
            {
                Property = ListViewItem.IsSelectedProperty,
                Value = true,
                Setters = {
                    new Setter(ListViewItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(173, 216, 230))), // LightBlue
                    new Setter(ListViewItem.ForegroundProperty, Brushes.Black)
                }
            });
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

            listView.Items.Add(CreateCategorySection("Текущая сессия", GetCurrentSessionParameters()));
            listView.Items.Add(CreateCategorySection("Статистика времени работы", GetWorkTimeStatisticsParameters()));

            cachedWorkTimePanel = scrollViewer;
            CacheDynamicTextBlocks(listView);

            StartUpdating();
            return cachedWorkTimePanel;
        }

        private void CacheDynamicTextBlocks(ListView listView)
        {
            var currentSessionSection = listView.Items
                .OfType<StackPanel>()
                .FirstOrDefault(sp => ((sp.Children[0] as Border)?.Child as StackPanel)?.Children.OfType<TextBlock>().FirstOrDefault()?.Text == "Текущая сессия");

            var statsSection = listView.Items
                .OfType<StackPanel>()
                .FirstOrDefault(sp => ((sp.Children[0] as Border)?.Child as StackPanel)?.Children.OfType<TextBlock>().FirstOrDefault()?.Text == "Статистика времени работы");

            if (currentSessionSection != null)
            {
                var listBox = currentSessionSection.Children.OfType<ListBox>().FirstOrDefault();
                if (listBox != null)
                {
                    uptimeTextBlock = listBox.Items
                        .OfType<StackPanel>()
                        .FirstOrDefault(sp => (sp.Children[1] as TextBlock)?.Text == "Время работы")?.Children[2] as TextBlock;

                    currentTimeTextBlock = listBox.Items
                        .OfType<StackPanel>()
                        .FirstOrDefault(sp => (sp.Children[1] as TextBlock)?.Text == "Текущее время")?.Children[2] as TextBlock;
                }
            }

            if (statsSection != null)
            {
                var listBox = statsSection.Children.OfType<ListBox>().FirstOrDefault();
                if (listBox != null)
                {
                    totalUptimeTextBlock = listBox.Items
                        .OfType<StackPanel>()
                        .FirstOrDefault(sp => (sp.Children[1] as TextBlock)?.Text == "Общее время работы")?.Children[2] as TextBlock;
                }
            }
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
            UpdateDynamicFields();
        }

        private void UpdateDynamicFields()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (uptimeTextBlock != null)
                    uptimeTextBlock.Text = GetSystemUptime();

                if (currentTimeTextBlock != null)
                    currentTimeTextBlock.Text = GetCurrentTime();

                if (totalUptimeTextBlock != null)
                    totalUptimeTextBlock.Text = FormatTimeSpan(GetTotalUptime());
            });
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
                case "Текущая сессия":
                    return "Ico/Time.png";
                case "Статистика времени работы":
                    return "Ico/Static.png";
                default:
                    return "Ico/Reserve.png";
            }
        }

        private string GetIconForParameter(string field)
        {
            switch (field)
            {
                case "Время последней загрузки":
                    return "Ico/GreenButton.png";
                case "Текущее время":
                    return "Ico/Watch.png";
                case "Время работы": 
                    return "Ico/Watch.png"; 
                case "Время первой загрузки":
                    return "Ico/GreenButton.png";
                case "Общее время работы":
                    return "Ico/GreenButton.png";
                default:
                    return "Ico/Reserve.png";
            }
        }

        public class SystemParameter
        {
            public string Field { get; set; }
            public string Value { get; set; }
        }

        public List<SystemParameter> GetCurrentSessionParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();
            DateTime bootTime = GetLastBootTime();

            Debug.WriteLine($"GetCurrentSessionParameters: BootTime={bootTime}");

            parameters.Add(new SystemParameter { Field = "Время последней загрузки", Value = bootTime.ToString("dd.MM.yyyy HH:mm:ss") });
            parameters.Add(new SystemParameter { Field = "Текущее время", Value = GetCurrentTime() });
            parameters.Add(new SystemParameter { Field = "Время работы", Value = GetSystemUptime() });

            return parameters;
        }

        public List<SystemParameter> GetWorkTimeStatisticsParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();
            DateTime firstBoot = GetFirstBootTime();
            TimeSpan totalUp = GetTotalUptime();

            Debug.WriteLine($"GetWorkTimeStatisticsParameters: FirstBoot={firstBoot}, TotalUptime={totalUp}");

            parameters.Add(new SystemParameter { Field = "Время первой загрузки", Value = firstBoot.ToString("dd.MM.yyyy HH:mm:ss") });
            parameters.Add(new SystemParameter { Field = "Общее время работы", Value = FormatTimeSpan(totalUp) });

            return parameters;
        }

        private string GetSystemUptime()
        {
            DateTime lastBoot = GetLastBootTime();
            TimeSpan uptime = DateTime.Now - lastBoot;
            Debug.WriteLine($"GetSystemUptime: LastBoot={lastBoot}, Uptime={uptime}");
            return FormatTimeSpan(uptime);
        }

        private string GetCurrentTime()
        {
            DateTime currentTime = DateTime.Now;
            Debug.WriteLine($"GetCurrentTime: CurrentTime={currentTime}");
            return currentTime.ToString("dd.MM.yyyy HH:mm:ss");
        }

        private DateTime GetLastBootTime()
        {
            try
            {
                TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
                DateTime bootTime = DateTime.Now - uptime;
                Debug.WriteLine($"GetLastBootTime (TickCount): BootTime={bootTime}, Uptime={uptime}");

                if (bootTime > DateTime.Now || bootTime < DateTime.Now.AddYears(-1))
                {
                    throw new Exception("Время загрузки через TickCount выглядит неправдоподобно");
                }

                return bootTime;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении времени последней загрузки через TickCount: {ex.Message}");
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            string lastBootUpTime = obj["LastBootUpTime"].ToString();
                            DateTime bootTime = ManagementDateTimeConverter.ToDateTime(lastBootUpTime);
                            Debug.WriteLine($"GetLastBootTime (WMI): BootTime={bootTime}");
                            return bootTime;
                        }
                    }
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine($"Ошибка при получении времени последней загрузки через WMI: {ex2.Message}");
                }
                DateTime fallback = DateTime.Now - TimeSpan.FromHours(1);
                Debug.WriteLine($"GetLastBootTime (Fallback): BootTime={fallback}");
                return fallback;
            }
        }

        private DateTime GetFirstBootTime()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT InstallDate FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string installDate = obj["InstallDate"].ToString();
                        DateTime firstBoot = ManagementDateTimeConverter.ToDateTime(installDate);
                        Debug.WriteLine($"GetFirstBootTime: FirstBoot (InstallDate)={firstBoot}");
                        return firstBoot;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении времени первой загрузки: {ex.Message}");
            }
            Debug.WriteLine("GetFirstBootTime: Не удалось определить время первой загрузки, возвращаем время последней загрузки");
            return GetLastBootTime();
        }

        private TimeSpan GetTotalUptime()
        {
            DateTime lastBoot = GetLastBootTime();
            TimeSpan totalUptime = DateTime.Now - lastBoot;
            Debug.WriteLine($"GetTotalUptime: LastBoot={lastBoot}, TotalUptime={totalUptime}");
            return totalUptime < TimeSpan.Zero ? TimeSpan.Zero : totalUptime;
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan < TimeSpan.Zero)
            {
                Debug.WriteLine("FormatTimeSpan: TimeSpan отрицательный, возвращаем 0");
                return "0 сек (0 дн., 0 ч., 0 мин., 0 сек)";
            }

            string result = $"{(int)timeSpan.TotalSeconds} сек ({timeSpan.Days} дн., {timeSpan.Hours} ч., {timeSpan.Minutes} мин., {timeSpan.Seconds} сек)";
            Debug.WriteLine($"FormatTimeSpan: TimeSpan={timeSpan}, Result={result}");
            return result;
        }

        public void Dispose()
        {
            updateTimer?.Stop();
            updateTimer?.Dispose();
            if (cachedWorkTimePanel != null)
            {
                cachedWorkTimePanel.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
            }
        }
    }
}