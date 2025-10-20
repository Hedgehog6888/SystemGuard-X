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

namespace Computer_Status_Viewer
{
    public class SummaryManager
    {
        private readonly ResourceDictionary resources;
        private ScrollViewer cachedSummaryPanel;
        private TextBlock dateTimeTextBlock;
        private readonly Timer updateTimer;

        public SummaryManager(ResourceDictionary resources)
        {
            this.resources = resources;

            updateTimer = new Timer(1000);
            updateTimer.Elapsed += UpdateTimerElapsed;
            updateTimer.AutoReset = true;
        }

        public UIElement CreateSummaryPanel()
        {
            if (cachedSummaryPanel != null)
            {
                UpdateDateTime();
                StartUpdating();
                return cachedSummaryPanel;
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

            listView.Items.Add(CreateCategorySection("Компьютер", GetComputerParameters()));
            listView.Items.Add(CreateCategorySection("Системная плата", GetMotherboardParameters()));
            listView.Items.Add(CreateCategorySection("Дисплей", GetDisplayParameters()));
            listView.Items.Add(CreateCategorySection("Мультимедиа", GetMultimediaParameters()));
            listView.Items.Add(CreateCategorySection("Хранение данных", GetStorageParameters()));
            listView.Items.Add(CreateCategorySection("Разделы", GetPartitionParameters()));
            listView.Items.Add(CreateCategorySection("Ввод", GetInputParameters()));
            listView.Items.Add(CreateCategorySection("Сеть", GetNetworkParameters()));
            listView.Items.Add(CreateCategorySection("Периферийные устройства", GetPeripheralParameters()));
            listView.Items.Add(CreateCategorySection("DMI", GetDMIParameters()));

            cachedSummaryPanel = scrollViewer;

            var computerSection = listView.Items
                .OfType<StackPanel>()
                .FirstOrDefault(sp => ((sp.Children[0] as Border)?.Child as StackPanel)?.Children.OfType<TextBlock>().FirstOrDefault()?.Text == "Компьютер");

            if (computerSection != null)
            {
                var listBox = computerSection.Children.OfType<ListBox>().FirstOrDefault();
                if (listBox != null)
                {
                    var dateTimeItem = listBox.Items
                        .OfType<StackPanel>()
                        .FirstOrDefault(sp => (sp.Children[1] as TextBlock)?.Text == "Дата / Время");
                    if (dateTimeItem != null)
                    {
                        dateTimeTextBlock = dateTimeItem.Children[2] as TextBlock;
                    }
                }
            }

            StartUpdating();
            return cachedSummaryPanel;
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
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            if (dateTimeTextBlock != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    dateTimeTextBlock.Text = DateTime.Now.ToString("yyyy-MM-dd / HH:mm:ss");
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

            // Чередование фона
            listBox.ItemContainerStyleSelector = new AlternatingRowStyleSelector();

            listBoxItemStyle.Triggers.Add(new Trigger
            {
                Property = ListBoxItem.IsMouseOverProperty,
                Value = true,
                Setters = { new Setter(ListBoxItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(240, 248, 255))) } // AliceBlue
            });
            listBoxItemStyle.Triggers.Add(new Trigger
            {
                Property = ListBoxItem.IsSelectedProperty,
                Value = true,
                Setters = {
                    new Setter(ListBoxItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(173, 216, 230))), // LightBlue
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

        // Класс для чередования фона строк
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
                case "Компьютер": return "Ico/Comp.png";
                case "Системная плата": return "Ico/Payment.png";
                case "Дисплей": return "Ico/Display.png";
                case "Мультимедиа": return "Ico/Sound.png";
                case "Хранение данных": return "Ico/HardDisk.png";
                case "Разделы": return "Ico/HardDisk.png";
                case "Ввод": return "Ico/Enter.png";
                case "Сеть": return "Ico/Network.png";
                case "Периферийные устройства": return "Ico/Devices.png";
                case "DMI": return "Ico/BIOS.png";
                default: return "Ico/Reserve.png";
            }
        }

        private string GetIconForParameter(string field)
        {
            string baseField = field.Split(' ')[0];

            switch (field)
            {
                case "Тип компьютера": return "Ico/Comp.png";
                case "Операционная система": return "Ico/OS.png";
                case "DirectX": return "Ico/DX.png";
                case "Имя компьютера": return "Ico/Comp.png";
                case "Имя пользователя": return "Ico/Comp.png";
                case "Дата / Время": return "Ico/Time.png";
                case "Тип ЦП": return "Ico/CPU.png";
                case "Системная плата": return "Ico/Payment.png";
                case "Чипсет системной платы": return "Ico/BIOS.png";
                case "Тип BIOS": return "Ico/BIOS.png";
                case "3D-акселератор": return "Ico/Videocard.png";
                case "DMI BIOS": return "Ico/BIOS.png";
                case "DMI Система": return "Ico/Comp.png";
                case "DMI Процессор": return "Ico/CPU.png";
            }

            switch (baseField)
            {
                case "Видеоадаптер": return "Ico/Videocard.png";
                case "Монитор": return "Ico/Display.png";
                case "Звуковой": return "Ico/Sound.png";
                case "IDE": return "Ico/ExtendedDisk.png";
                case "SCSI": return "Ico/ExtendedDisk.png";
                case "Дисковый": return "Ico/HardDisk.png";
                case "Оптический": return "Ico/Disk.png";
                case "Раздел": return "Ico/HardDisk.png";
                case "Клавиатура": return "Ico/Keyboard.png";
                case "Мышь": return "Ico/Mouse.png";
                case "Сетевой": return "Ico/Network.png";
                case "USB": return "Ico/Usb.png";
                case "Принтер": return "Ico/Printer.png";
                default: return "Ico/Reserve.png";
            }
        }

        public class SystemParameter
        {
            public string Field { get; set; }
            public string Value { get; set; }
        }

        public List<SystemParameter> GetComputerParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            parameters.Add(new SystemParameter { Field = "Тип компьютера", Value = GetWmiProperty("Win32_ComputerSystem", "Model") });
            parameters.Add(new SystemParameter { Field = "Операционная система", Value = GetWmiProperty("Win32_OperatingSystem", "Caption") });
            parameters.Add(new SystemParameter { Field = "DirectX", Value = GetDirectXVersion() });
            parameters.Add(new SystemParameter { Field = "Имя компьютера", Value = Environment.MachineName });
            parameters.Add(new SystemParameter { Field = "Имя пользователя", Value = Environment.UserName });
            parameters.Add(new SystemParameter { Field = "Дата / Время", Value = DateTime.Now.ToString("yyyy-MM-dd / HH:mm:ss") });

            return parameters;
        }

        public List<SystemParameter> GetMotherboardParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            parameters.Add(new SystemParameter { Field = "Тип ЦП", Value = GetWmiProperty("Win32_Processor", "Name") });
            parameters.Add(new SystemParameter { Field = "Системная плата", Value = GetWmiProperty("Win32_BaseBoard", "Product") });
            parameters.Add(new SystemParameter { Field = "Чипсет системной платы", Value = GetWmiProperty("Win32_Chipset", "Caption") });
            parameters.Add(new SystemParameter { Field = "Тип BIOS", Value = GetWmiProperty("Win32_BIOS", "Manufacturer") + " " + GetWmiProperty("Win32_BIOS", "SMBIOSBIOSVersion") });

            return parameters;
        }

        public List<SystemParameter> GetDisplayParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            int videoAdapterIndex = 1;
            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT * FROM Win32_VideoController").Get())
            {
                parameters.Add(new SystemParameter { Field = $"Видеоадаптер {(videoAdapterIndex > 1 ? videoAdapterIndex.ToString() : "")}", Value = obj["Caption"]?.ToString() ?? "N/A" });
                videoAdapterIndex++;
            }

            parameters.Add(new SystemParameter { Field = "3D-акселератор", Value = GetWmiProperty("Win32_VideoController", "AdapterCompatibility") });

            int monitorIndex = 1;
            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor").Get())
            {
                parameters.Add(new SystemParameter { Field = $"Монитор {(monitorIndex > 1 ? monitorIndex.ToString() : "")}", Value = obj["Caption"]?.ToString() ?? "N/A" });
                monitorIndex++;
            }

            return parameters;
        }

        public List<SystemParameter> GetMultimediaParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            int audioIndex = 1;
            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice").Get())
            {
                parameters.Add(new SystemParameter { Field = $"Звуковой адаптер {(audioIndex > 1 ? audioIndex.ToString() : "")}", Value = obj["Caption"]?.ToString() ?? "N/A" });
                audioIndex++;
            }

            return parameters;
        }

        public List<SystemParameter> GetStorageParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            int ideIndex = 1;
            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT * FROM Win32_IDEController").Get())
            {
                parameters.Add(new SystemParameter { Field = $"IDE контроллер {(ideIndex > 1 ? ideIndex.ToString() : "")}", Value = obj["Caption"]?.ToString() ?? "N/A" });
                ideIndex++;
            }

            int scsiIndex = 1;
            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT * FROM Win32_SCSIController").Get())
            {
                parameters.Add(new SystemParameter { Field = $"SCSI контроллер {(scsiIndex > 1 ? scsiIndex.ToString() : "")}", Value = obj["Caption"]?.ToString() ?? "N/A" });
                scsiIndex++;
            }

            int diskIndex = 1;
            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive").Get())
            {
                parameters.Add(new SystemParameter { Field = $"Дисковый накопитель {(diskIndex > 1 ? diskIndex.ToString() : "")}", Value = obj["Caption"]?.ToString() ?? "N/A" });
                diskIndex++;
            }

            int opticalIndex = 1;
            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT * FROM Win32_CDROMDrive").Get())
            {
                parameters.Add(new SystemParameter { Field = $"Оптический накопитель {(opticalIndex > 1 ? opticalIndex.ToString() : "")}", Value = obj["Caption"]?.ToString() ?? "N/A" });
                opticalIndex++;
            }

            return parameters;
        }

        public List<SystemParameter> GetPartitionParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            int partitionIndex = 1;
            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DriveType=3").Get())
            {
                string size = obj["Size"] != null ? $"{Math.Round(Convert.ToDouble(obj["Size"]) / (1024 * 1024 * 1024), 2)} GB" : "N/A";
                parameters.Add(new SystemParameter { Field = $"Раздел {(partitionIndex > 1 ? partitionIndex.ToString() : "")}", Value = $"{obj["DeviceID"]} ({obj["FileSystem"]}, {size})" });
                partitionIndex++;
            }

            return parameters;
        }

        public List<SystemParameter> GetInputParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            int keyboardIndex = 1;
            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT * FROM Win32_Keyboard").Get())
            {
                parameters.Add(new SystemParameter { Field = $"Клавиатура {(keyboardIndex > 1 ? keyboardIndex.ToString() : "")}", Value = obj["Caption"]?.ToString() ?? "N/A" });
                keyboardIndex++;
            }

            int mouseIndex = 1;
            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT * FROM Win32_PointingDevice").Get())
            {
                parameters.Add(new SystemParameter { Field = $"Мышь {(mouseIndex > 1 ? mouseIndex.ToString() : "")}", Value = obj["Caption"]?.ToString() ?? "N/A" });
                mouseIndex++;
            }

            return parameters;
        }

        public List<SystemParameter> GetNetworkParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            int networkIndex = 1;
            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter=True").Get())
            {
                parameters.Add(new SystemParameter { Field = $"Сетевой адаптер {(networkIndex > 1 ? networkIndex.ToString() : "")}", Value = obj["Caption"]?.ToString() ?? "N/A" });
                networkIndex++;
            }

            return parameters;
        }

        public List<SystemParameter> GetPeripheralParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            int usbIndex = 1;
            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE '%USB%'").Get())
            {
                parameters.Add(new SystemParameter { Field = $"USB устройство {(usbIndex > 1 ? usbIndex.ToString() : "")}", Value = obj["Caption"]?.ToString() ?? "N/A" });
                usbIndex++;
            }

            int printerIndex = 1;
            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT * FROM Win32_Printer").Get())
            {
                parameters.Add(new SystemParameter { Field = $"Принтер {(printerIndex > 1 ? printerIndex.ToString() : "")}", Value = obj["Caption"]?.ToString() ?? "N/A" });
                printerIndex++;
            }

            return parameters;
        }

        public List<SystemParameter> GetDMIParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();

            parameters.Add(new SystemParameter { Field = "DMI BIOS", Value = GetWmiProperty("Win32_BIOS", "Manufacturer") + " " + GetWmiProperty("Win32_BIOS", "SMBIOSBIOSVersion") });
            parameters.Add(new SystemParameter { Field = "DMI Система", Value = GetWmiProperty("Win32_ComputerSystem", "Manufacturer") + " " + GetWmiProperty("Win32_ComputerSystem", "Model") });
            parameters.Add(new SystemParameter { Field = "DMI Процессор", Value = GetWmiProperty("Win32_Processor", "Name") });

            return parameters;
        }

        private string GetWmiProperty(string wmiClass, string wmiProperty)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {wmiProperty} FROM {wmiClass}"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj[wmiProperty]?.ToString() ?? "N/A";
                    }
                }
            }
            catch
            {
                return "N/A";
            }
            return "N/A";
        }

        private string GetDirectXVersion()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string version = obj["DriverVersion"]?.ToString();
                        if (!string.IsNullOrEmpty(version))
                        {
                            Version ver = new Version(version);
                            if (ver.Major >= 12)
                            {
                                return "DirectX 12.0";
                            }
                            else if (ver.Major >= 11)
                            {
                                return "DirectX 11.0";
                            }
                        }
                    }
                }
            }
            catch
            {
                return "N/A";
            }
            return "N/A";
        }

        public void Dispose()
        {
            updateTimer?.Stop();
            updateTimer?.Dispose();
            if (cachedSummaryPanel != null)
            {
                cachedSummaryPanel.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
            }
        }
    }
}