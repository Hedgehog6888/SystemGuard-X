using System;
using System.Collections.Generic;
using System.Linq; // Для LINQ
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Computer_Status_Viewer
{
    public class BIOSManager
    {
        private readonly ResourceDictionary resources;
        private ScrollViewer cachedBIOSPanel;

        public BIOSManager(ResourceDictionary resources)
        {
            this.resources = resources;
        }

        public UIElement CreateBIOSPanel()
        {
            if (cachedBIOSPanel != null)
            {
                return cachedBIOSPanel;
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

            var biosSection = CreateCategorySection("Свойства BIOS", GetBIOSProperties());
            listView.Items.Add(biosSection);

            cachedBIOSPanel = scrollViewer;
            return cachedBIOSPanel;
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
                case "Свойства BIOS":
                    return "Ico/BIOS.png";
                default:
                    return "Ico/Reserve.png";
            }
        }

        private string GetIconForParameter(string field)
        {
            switch (field)
            {
                case "Тип BIOS":
                    return "Ico/BIOS.png";
                case "Версия BIOS":
                    return "Ico/BIOS.png";
                case "Версия AGESA":
                    return "Ico/CPU.png";
                case "SMBIOS Version":
                    return "Ico/BIOS.png";
                case "UEFI Boot":
                    return "Ico/BIOS.png";
                case "Secure Boot":
                    return "Ico/Castle.png";
                case "Дата BIOS системы":
                    return "Ico/Comp.png";
                case "Дата BIOS видеокарты":
                    return "Ico/Comp.png";
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

        private List<SystemParameter> GetBIOSProperties()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            try
            {
                // Тип BIOS
                string biosType = "N/A";
                string biosCharacteristics = GetWmiProperty("Win32_BIOS", "BIOSCharacteristics");
                if (biosCharacteristics != "N/A")
                {
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var characteristics = obj["BIOSCharacteristics"] as ushort[];
                            if (characteristics != null)
                            {
                                // Явно преобразуем массив в IEnumerable<ushort> и используем LINQ Contains
                                if (characteristics.AsEnumerable().Contains((ushort)40))
                                {
                                    biosType = "AMI UEFI";
                                }
                                else
                                {
                                    biosType = "Legacy BIOS";
                                }
                            }
                            else
                            {
                                biosType = "Неизвестно";
                            }
                        }
                    }
                }
                parameters.Add(new SystemParameter { Field = "Тип BIOS", Value = biosType });

                // Версия BIOS
                string biosVersion = GetWmiProperty("Win32_BIOS", "SMBIOSBIOSVersion");
                parameters.Add(new SystemParameter { Field = "Версия BIOS", Value = biosVersion });

                // Версия AGESA
                string agesaVersion = "N/A";
                if (biosVersion != "N/A" && biosVersion.Contains("AGESA"))
                {
                    agesaVersion = biosVersion.Split(new[] { "AGESA" }, StringSplitOptions.None).LastOrDefault()?.Trim() ?? "N/A";
                }
                parameters.Add(new SystemParameter { Field = "Версия AGESA", Value = agesaVersion });

                // SMBIOS Version
                string smbiosVersion = GetWmiProperty("Win32_BIOS", "SMBIOSMajorVersion") + "." + GetWmiProperty("Win32_BIOS", "SMBIOSMinorVersion");
                parameters.Add(new SystemParameter { Field = "SMBIOS Version", Value = smbiosVersion });

                // UEFI Boot
                string uefiBoot = "N/A";
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control"))
                    {
                        if (key != null)
                        {
                            object secureBoot = key.GetValue("SystemStartOptions");
                            if (secureBoot != null && secureBoot.ToString().ToLower().Contains("uefi"))
                            {
                                uefiBoot = "Да";
                            }
                            else
                            {
                                uefiBoot = "Нет";
                            }
                        }
                    }
                }
                catch
                {
                    uefiBoot = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "UEFI Boot", Value = uefiBoot });

                // Secure Boot
                string secureBootState = "N/A";
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State"))
                    {
                        if (key != null)
                        {
                            object secureBootValue = key.GetValue("UEFISecureBootEnabled");
                            if (secureBootValue != null && Convert.ToInt32(secureBootValue) == 1)
                            {
                                secureBootState = "Запрещено";
                            }
                            else
                            {
                                secureBootState = "Нет";
                            }
                        }
                    }
                }
                catch
                {
                    secureBootState = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "Secure Boot", Value = secureBootState });

                // Дата BIOS системы
                string biosDate = GetWmiProperty("Win32_BIOS", "ReleaseDate");
                if (biosDate != "N/A")
                {
                    try
                    {
                        DateTime date = ManagementDateTimeConverter.ToDateTime(biosDate);
                        biosDate = date.ToString("dd/MM/yyyy");
                    }
                    catch
                    {
                        biosDate = "N/A";
                    }
                }
                parameters.Add(new SystemParameter { Field = "Дата BIOS системы", Value = biosDate });

                // Дата BIOS видеокарты
                string videoBiosDate = "N/A";
                try
                {
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            string driverDate = obj["DriverDate"]?.ToString();
                            if (!string.IsNullOrEmpty(driverDate))
                            {
                                DateTime date = ManagementDateTimeConverter.ToDateTime(driverDate);
                                videoBiosDate = date.ToString("dd/MM/yyyy");
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    videoBiosDate = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "Дата BIOS видеокарты", Value = videoBiosDate });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных о BIOS: {ex.Message}");
                parameters.Add(new SystemParameter { Field = "Тип BIOS", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Версия BIOS", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Версия AGESA", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "SMBIOS Version", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "UEFI Boot", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Secure Boot", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Дата BIOS системы", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Дата BIOS видеокарты", Value = "N/A" });
            }

            return parameters;
        }

        public class SystemParameter
        {
            public string Field { get; set; }
            public string Value { get; set; }
        }
    }
}