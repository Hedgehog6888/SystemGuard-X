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
    public class OSManager
    {
        private readonly ResourceDictionary resources;
        private ScrollViewer cachedOSPanel;
        private DispatcherTimer updateTimer; // Таймер для обновления времени работы
        private TextBlock uptimeTextBlock; // Ссылка на TextBlock для времени работы
        private DateTime lastBootTime; // Время последней загрузки системы

        public OSManager(ResourceDictionary resources)
        {
            this.resources = resources;

            // Инициализируем таймер
            updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Обновляем каждую секунду
            };
            updateTimer.Tick += UpdateTimer_Tick;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (uptimeTextBlock != null)
            {
                try
                {
                    TimeSpan uptimeSpan = DateTime.Now - lastBootTime;
                    int totalSeconds = (int)uptimeSpan.TotalSeconds;
                    int days = uptimeSpan.Days;
                    int hours = uptimeSpan.Hours;
                    int minutes = uptimeSpan.Minutes;
                    int seconds = uptimeSpan.Seconds;
                    string uptime = $"{totalSeconds} сек ({days} дн., {hours} ч., {minutes} мин., {seconds} сек)";
                    uptimeTextBlock.Text = uptime;
                    Console.WriteLine($"Обновлено время работы: {uptime}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обновлении времени работы: {ex.Message}");
                    uptimeTextBlock.Text = "N/A";
                }
            }
        }

        public UIElement CreateOSPanel()
        {
            if (cachedOSPanel != null)
            {
                Console.WriteLine("Возвращаем кэшированную панель ОС");
                return cachedOSPanel;
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

            var osPropertiesSection = CreateCategorySection("Свойства операционной системы", GetOSProperties());
            var registeredUserSection = CreateCategorySection("Лицензионная информация", GetRegisteredUserProperties());
            var currentSessionSection = CreateCategorySection("Текущая сессия", GetCurrentSessionProperties());
            var componentsSection = CreateCategorySection("Версия компонентов", GetComponentVersions());

            Console.WriteLine("Добавляем категорию: Свойства операционной системы");
            listView.Items.Add(osPropertiesSection);

            Console.WriteLine("Добавляем категорию: Лицензионная информация");
            listView.Items.Add(registeredUserSection);

            Console.WriteLine("Добавляем категорию: Текущая сессия");
            listView.Items.Add(currentSessionSection);

            Console.WriteLine("Добавляем категорию: Версия компонентов");
            listView.Items.Add(componentsSection);

            cachedOSPanel = scrollViewer;
            Console.WriteLine("Панель ОС создана и кэширована");

            // Запускаем таймер после создания панели
            updateTimer.Start();
            Console.WriteLine("Таймер обновления времени работы запущен");

            return cachedOSPanel;
        }

        public void StopUpdating()
        {
            if (updateTimer != null && updateTimer.IsEnabled)
            {
                updateTimer.Stop();
                Console.WriteLine("Таймер обновления времени работы остановлен");
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

        private UIElement CreateCategorySection(string categoryName, List<SystemParameter> parameters)
        {
            Console.WriteLine($"Создаем категорию: {categoryName}, параметров: {parameters.Count}");

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

                // Если это поле "Время работы", используем сохраненный TextBlock
                if (param.Field == "Время работы" && param.TextBlock != null)
                {
                    itemPanel.Children.Add(param.TextBlock);
                }
                else
                {
                    itemPanel.Children.Add(new TextBlock
                    {
                        Text = param.Value,
                        Margin = new Thickness(0, 2, 0, 2),
                        FontSize = 13,
                        Foreground = Brushes.Black,
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    });
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
                case "Свойства операционной системы":
                    return "Ico/OS.png";
                case "Лицензионная информация":
                    return "Ico/Castle.png";
                case "Текущая сессия":
                    return "Ico/Watch.png";
                case "Версия компонентов":
                    return "Ico/OS.png";
                default:
                    return "Ico/Reserve.png";
            }
        }

        private string GetIconForParameter(string field)
        {
            switch (field)
            {
                case "Имя ОС":
                    return "Ico/OS.png";
                case "Язык ОС":
                    return "Ico/Region.png";
                case "Тип ядра ОС":
                    return "Ico/OS.png";
                case "Версия ОС":
                    return "Ico/OS.png";
                case "Основная папка ОС":
                    return "Ico/SystemFolder.png";
                case "Дата установки ОС":
                    return "Ico/Time.png";
                case "Зарегистрированный пользователь":
                    return "Ico/User.png";
                case "Зарегистрированная организация":
                    return "Ico/Institution.png";
                case "ID продукта":
                    return "Ico/Castle.png";
                case "Ключ продукта":
                    return "Ico/Castle.png";
                case "Активация продукта (WPA)":
                    return "Ico/Keys.png";
                case "Имя компьютера":
                    return "Ico/Comp.png";
                case "Имя пользователя":
                    return "Ico/User.png";
                case "Время работы":
                    return "Ico/Watch.png";
                case "Common Controls":
                    return "Ico/OS.png";
                case "Обозреватель Internet Explorer":
                    return "Ico/Edge.png";
                case "Просмотрщик Windows Mail":
                    return "Ico/Prog.png";
                case "Windows Messenger":
                    return "Ico/Prog.png";
                case "Internet Information Services (IIS)":
                    return "Ico/Prog.png";
                case "NET Framework":
                    return "Ico/OS.png";
                case "Kuerent Novell":
                    return "Ico/Prog.png";
                case "DirectX":
                    return "Ico/DX.png";
                case "OpenGL":
                    return "Ico/DX.png";
                case "ASPI":
                    return "Ico/Prog.png";
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

        private List<SystemParameter> GetOSProperties()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            try
            {
                string osName = GetWmiProperty("Win32_OperatingSystem", "Caption");
                parameters.Add(new SystemParameter { Field = "Имя ОС", Value = osName });

                string osLanguage = GetWmiProperty("Win32_OperatingSystem", "OSLanguage");
                if (osLanguage != "N/A")
                {
                    switch (osLanguage)
                    {
                        case "1049":
                            osLanguage = "Русский (Россия)";
                            break;
                        default:
                            osLanguage = "Неизвестный язык";
                            break;
                    }
                }
                parameters.Add(new SystemParameter { Field = "Язык ОС", Value = osLanguage });

                string osType = GetWmiProperty("Win32_OperatingSystem", "OSArchitecture");
                if (osType != "N/A")
                {
                    osType = $"Multiprocessor Free ({osType})";
                }
                parameters.Add(new SystemParameter { Field = "Тип ядра ОС", Value = osType });

                string osVersion = GetWmiProperty("Win32_OperatingSystem", "Version");
                string osBuild = GetWmiProperty("Win32_OperatingSystem", "BuildNumber");
                if (osVersion != "N/A" && osBuild != "N/A")
                {
                    osVersion = $"{osVersion}.{osBuild}";
                }
                parameters.Add(new SystemParameter { Field = "Версия ОС", Value = osVersion });

                string installDate = GetWmiProperty("Win32_OperatingSystem", "InstallDate");
                if (installDate != "N/A")
                {
                    try
                    {
                        DateTime date = ManagementDateTimeConverter.ToDateTime(installDate);
                        installDate = date.ToString("dd.MM.yyyy");
                    }
                    catch
                    {
                        installDate = "N/A";
                    }
                }
                parameters.Add(new SystemParameter { Field = "Дата установки ОС", Value = installDate });

                string osFolder = GetWmiProperty("Win32_OperatingSystem", "WindowsDirectory");
                parameters.Add(new SystemParameter { Field = "Основная папка ОС", Value = osFolder });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении свойств ОС: {ex.Message}");
                parameters.Add(new SystemParameter { Field = "Имя ОС", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Язык ОС", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Тип ядра ОС", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Версия ОС", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Дата установки ОС", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Основная папка ОС", Value = "N/A" });
            }

            return parameters;
        }

        private List<SystemParameter> GetRegisteredUserProperties()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            try
            {
                string registeredUser = "N/A";
                try
                {
                    registeredUser = GetWmiProperty("Win32_OperatingSystem", "RegisteredUser");
                    Console.WriteLine($"WMI RegisteredUser: {registeredUser}");

                    if (string.IsNullOrEmpty(registeredUser) || registeredUser == "N/A")
                    {
                        using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                        {
                            if (key != null)
                            {
                                registeredUser = key.GetValue("RegisteredOwner")?.ToString() ?? "N/A";
                                Console.WriteLine($"Registry RegisteredOwner: {registeredUser}");
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(registeredUser) || registeredUser == "N/A")
                    {
                        registeredUser = Environment.UserName;
                        Console.WriteLine($"Environment.UserName: {registeredUser}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении Зарегистрированный пользователь: {ex.Message}");
                    registeredUser = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "Зарегистрированный пользователь", Value = registeredUser });

                string registeredOrg = "N/A";
                try
                {
                    registeredOrg = GetWmiProperty("Win32_OperatingSystem", "Organization");
                    Console.WriteLine($"WMI Organization: {registeredOrg}");

                    if (string.IsNullOrEmpty(registeredOrg) || registeredOrg == "N/A")
                    {
                        using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                        {
                            if (key != null)
                            {
                                registeredOrg = key.GetValue("RegisteredOrganization")?.ToString() ?? "N/A";
                                Console.WriteLine($"Registry RegisteredOrganization: {registeredOrg}");
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(registeredOrg) || registeredOrg == "N/A")
                    {
                        string userEmail = GetWmiProperty("Win32_UserAccount", "Description", $"Name = '{Environment.UserName}'");
                        if (!string.IsNullOrEmpty(userEmail) && userEmail.Contains("@"))
                        {
                            registeredOrg = userEmail;
                            Console.WriteLine($"UserAccount Description (email): {registeredOrg}");
                        }
                    }

                    if (string.IsNullOrEmpty(registeredOrg) || registeredOrg == "N/A")
                    {
                        using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\IdentityCRL\ValidatedEmail"))
                        {
                            if (key != null)
                            {
                                registeredOrg = key.GetValue("Email")?.ToString() ?? "N/A";
                                Console.WriteLine($"Microsoft Account Email: {registeredOrg}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении Зарегистрированной организации: {ex.Message}");
                    registeredOrg = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "Зарегистрированная организация", Value = registeredOrg });

                string productId = GetWmiProperty("Win32_OperatingSystem", "SerialNumber");
                Console.WriteLine($"ID продукта: {productId}");
                parameters.Add(new SystemParameter { Field = "ID продукта", Value = productId });

                string productKey = "N/A";
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                    {
                        if (key != null)
                        {
                            object digitalProductId = key.GetValue("DigitalProductId");
                            object digitalProductId4 = key.GetValue("DigitalProductId4");

                            if (digitalProductId4 != null)
                            {
                                productKey = DecodeProductKey((byte[])digitalProductId4, true);
                                Console.WriteLine($"Ключ продукта (DigitalProductId4): {productKey}");
                            }
                            else if (digitalProductId != null)
                            {
                                productKey = DecodeProductKey((byte[])digitalProductId, false);
                                Console.WriteLine($"Ключ продукта (DigitalProductId): {productKey}");
                            }
                        }
                    }

                    if (productKey == "N/A")
                    {
                        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM SoftwareLicensingProduct WHERE PartialProductKey IS NOT NULL"))
                        {
                            foreach (ManagementObject obj in searcher.Get())
                            {
                                productKey = obj["PartialProductKey"]?.ToString() ?? "N/A";
                                if (productKey != "N/A")
                                {
                                    productKey = $"XXXXX-XXXXX-XXXXX-XXXXX-{productKey}";
                                    Console.WriteLine($"Ключ продукта (WMI PartialProductKey): {productKey}");
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении ключа продукта: {ex.Message}");
                    productKey = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "Ключ продукта", Value = productKey });

                string activationStatus = "N/A";
                try
                {
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM SoftwareLicensingProduct WHERE PartialProductKey IS NOT NULL"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            string licenseStatus = obj["LicenseStatus"]?.ToString();
                            if (licenseStatus == "1")
                            {
                                activationStatus = "Не требуется";
                            }
                            else
                            {
                                activationStatus = "Требуется";
                            }
                            Console.WriteLine($"Активация продукта (WPA): {activationStatus}");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при проверке активации: {ex.Message}");
                    activationStatus = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "Активация продукта (WPA)", Value = activationStatus });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных о зарегистрированном пользователе: {ex.Message}");
                parameters.Add(new SystemParameter { Field = "Зарегистрированный пользователь", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Зарегистрированная организация", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "ID продукта", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Ключ продукта", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Активация продукта (WPA)", Value = "N/A" });
            }

            Console.WriteLine($"Параметры для 'Зарегистрированный пользователь': {parameters.Count}");
            return parameters;
        }

        private List<SystemParameter> GetCurrentSessionProperties()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            try
            {
                string computerName = GetWmiProperty("Win32_ComputerSystem", "Name");
                parameters.Add(new SystemParameter { Field = "Имя компьютера", Value = computerName });

                string userName = GetWmiProperty("Win32_ComputerSystem", "UserName");
                if (userName != "N/A" && userName.Contains("\\"))
                {
                    userName = userName.Split('\\').Last();
                }
                parameters.Add(new SystemParameter { Field = "Имя пользователя", Value = userName });

                string uptime = "N/A";
                try
                {
                    string lastBoot = GetWmiProperty("Win32_OperatingSystem", "LastBootUpTime");
                    if (lastBoot != "N/A")
                    {
                        lastBootTime = ManagementDateTimeConverter.ToDateTime(lastBoot);
                        TimeSpan uptimeSpan = DateTime.Now - lastBootTime;
                        int totalSeconds = (int)uptimeSpan.TotalSeconds;
                        int days = uptimeSpan.Days;
                        int hours = uptimeSpan.Hours;
                        int minutes = uptimeSpan.Minutes;
                        int seconds = uptimeSpan.Seconds;
                        uptime = $"{totalSeconds} сек ({days} дн., {hours} ч., {minutes} мин., {seconds} сек)";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении времени работы: {ex.Message}");
                    uptime = "N/A";
                }

                // Создаем TextBlock для времени работы и сохраняем его
                uptimeTextBlock = new TextBlock
                {
                    Text = uptime,
                    Margin = new Thickness(0, 2, 0, 2),
                    FontSize = 13,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                parameters.Add(new SystemParameter { Field = "Время работы", Value = uptime, TextBlock = uptimeTextBlock });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных о текущей сессии: {ex.Message}");
                parameters.Add(new SystemParameter { Field = "Имя компьютера", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Имя пользователя", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Время работы", Value = "N/A" });
            }

            return parameters;
        }

        private List<SystemParameter> GetComponentVersions()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            try
            {
                string commonControls = "N/A";
                try
                {
                    string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
                    string comctlPath = System.IO.Path.Combine(systemPath, "comctl32.dll");
                    if (System.IO.File.Exists(comctlPath))
                    {
                        var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(comctlPath);
                        commonControls = versionInfo.FileVersion ?? "N/A";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении версии Common Controls: {ex.Message}");
                    commonControls = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "Common Controls", Value = commonControls });

                string ieVersion = "N/A";
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer"))
                    {
                        if (key != null)
                        {
                            ieVersion = key.GetValue("svcVersion")?.ToString() ?? key.GetValue("Version")?.ToString() ?? "N/A";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении версии IE: {ex.Message}");
                    ieVersion = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "Обозреватель Internet Explorer", Value = ieVersion });

                string windowsMail = "N/A";
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Mail"))
                    {
                        if (key != null)
                        {
                            windowsMail = key.GetValue("Version")?.ToString() ?? "N/A";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении версии Windows Mail: {ex.Message}");
                    windowsMail = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "Просмотрщик Windows Mail", Value = windowsMail });

                string windowsMessenger = "N/A";
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Messenger"))
                    {
                        if (key != null)
                        {
                            windowsMessenger = key.GetValue("Version")?.ToString() ?? "N/A";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении версии Windows Messenger: {ex.Message}");
                    windowsMessenger = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "Windows Messenger", Value = windowsMessenger });

                string iisVersion = "N/A";
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\InetStp"))
                    {
                        if (key != null)
                        {
                            iisVersion = key.GetValue("VersionString")?.ToString() ?? "N/A";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении версии IIS: {ex.Message}");
                    iisVersion = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "Internet Information Services (IIS)", Value = iisVersion });

                string netFramework = "N/A";
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"))
                    {
                        if (key != null)
                        {
                            string version = key.GetValue("Version")?.ToString();
                            string release = key.GetValue("Release")?.ToString();
                            if (version != null)
                            {
                                netFramework = $"{version} (Release {release})";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении версии .NET Framework: {ex.Message}");
                    netFramework = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "NET Framework", Value = netFramework });

                string kuerentNovell = "N/A";
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Novell"))
                    {
                        if (key != null)
                        {
                            kuerentNovell = key.GetValue("Version")?.ToString() ?? "N/A";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении версии Kuerent Novell: {ex.Message}");
                    kuerentNovell = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "Kuerent Novell", Value = kuerentNovell });

                string directX = "N/A";
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\DirectX"))
                    {
                        if (key != null)
                        {
                            directX = key.GetValue("Version")?.ToString() ?? "N/A";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении версии DirectX: {ex.Message}");
                    directX = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "DirectX", Value = directX });

                string openGL = "N/A";
                try
                {
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            string driverVersion = obj["DriverVersion"]?.ToString();
                            if (!string.IsNullOrEmpty(driverVersion))
                            {
                                openGL = driverVersion;
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении версии OpenGL: {ex.Message}");
                    openGL = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "OpenGL", Value = openGL });

                string aspi = "N/A";
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\ASPI"))
                    {
                        if (key != null)
                        {
                            aspi = key.GetValue("Version")?.ToString() ?? "N/A";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении версии ASPI: {ex.Message}");
                    aspi = "N/A";
                }
                parameters.Add(new SystemParameter { Field = "ASPI", Value = aspi });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении версий компонентов: {ex.Message}");
                parameters.Add(new SystemParameter { Field = "Common Controls", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Обозреватель Internet Explorer", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Просмотрщик Windows Mail", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Windows Messenger", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Internet Information Services (IIS)", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "NET Framework", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "Kuerent Novell", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "DirectX", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "OpenGL", Value = "N/A" });
                parameters.Add(new SystemParameter { Field = "ASPI", Value = "N/A" });
            }

            return parameters;
        }

        private string DecodeProductKey(byte[] digitalProductId, bool isDigitalProductId4)
        {
            try
            {
                const string chars = "BCDFGHJKMPQRTVWXY2346789";
                char[] key = new char[25];
                int offset = isDigitalProductId4 ? 1648 : 52;

                for (int i = 24; i >= 0; i--)
                {
                    int accumulator = 0;
                    for (int j = 14; j >= 0; j--)
                    {
                        accumulator = accumulator * 256;
                        accumulator += digitalProductId[offset + j];
                        digitalProductId[offset + j] = (byte)(accumulator / 24);
                        accumulator %= 24;
                    }
                    key[i] = chars[accumulator];
                }

                string productKey = new string(key);
                for (int i = 5; i < 25; i += 6)
                {
                    productKey = productKey.Insert(i, "-");
                }
                return productKey;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при декодировании ключа продукта: {ex.Message}");
                return "N/A";
            }
        }

        public class SystemParameter
        {
            public string Field { get; set; }
            public string Value { get; set; }
            public TextBlock TextBlock { get; set; } // Добавляем поле для хранения TextBlock
        }
    }
}