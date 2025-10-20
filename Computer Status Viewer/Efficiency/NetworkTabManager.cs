using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using LiveCharts;
using System.Management;
using LiveCharts.Wpf;

namespace Computer_Status_Viewer
{
    public class NetworkTabManager : IDisposable
    {
        public TabItem Tab { get; private set; }
        public ChartValues<double> NetworkSendValues { get; } = new ChartValues<double>(Enumerable.Repeat(0.0, 60));
        public ChartValues<double> NetworkReceiveValues { get; } = new ChartValues<double>(Enumerable.Repeat(0.0, 60));
        private NetworkInterface networkInterface;
        private long lastBytesSent;
        private long lastBytesReceived;
        private DateTime lastUpdateTime;
        private bool hasNetwork;

        public NetworkTabManager()
        {
            InitializeNetworkInterface();
            if (hasNetwork)
            {
                Tab = InitializeNetworkTab();
            }
        }

        private void InitializeNetworkInterface()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            networkInterface = interfaces
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                             (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                              ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                .OrderByDescending(ni => ni.GetIPv4Statistics().BytesReceived)
                .FirstOrDefault();

            if (networkInterface == null)
            {
                networkInterface = interfaces.FirstOrDefault(ni => ni.OperationalStatus == OperationalStatus.Up);
            }

            if (networkInterface != null)
            {
                hasNetwork = true;
                var stats = networkInterface.GetIPv4Statistics();
                lastBytesSent = stats.BytesSent;
                lastBytesReceived = stats.BytesReceived;
                lastUpdateTime = DateTime.Now;
                Console.WriteLine($"Сетевой интерфейс инициализирован: {networkInterface.Description}");
            }
            else
            {
                hasNetwork = false;
                Console.WriteLine("Не найдено активных сетевых интерфейсов.");
            }
        }

        private TabItem InitializeNetworkTab()
        {
            string name = networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet ? "Ethernet" : "Wi-Fi";
            TabItem tab = new TabItem { Header = name }; // Простая строка для заголовка вкладки
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
                Text = name,
                Foreground = Brushes.Black,
                FontSize = 18,
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 5, 10, 0)
            };
            Grid.SetColumn(titleBlock, 0);
            headerGrid.Children.Add(titleBlock);

            TextBlock adapterNameBlock = new TextBlock
            {
                Text = networkInterface.Description,
                Foreground = Brushes.Black,
                FontSize = 18,
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 10, 2)
            };
            Grid.SetColumn(adapterNameBlock, 2);
            headerGrid.Children.Add(adapterNameBlock);

            Grid.SetRow(headerGrid, 0);
            mainGrid.Children.Add(headerGrid);

            TextBlock bandwidthLabel = new TextBlock
            {
                Text = "Пропускная способность",
                FontSize = 14,
                FontWeight = FontWeights.Normal,
                Foreground = new SolidColorBrush(Color.FromArgb(150, 105, 105, 105)),
                Margin = new Thickness(5, 5, 0, 2)
            };
            Grid.SetRow(bandwidthLabel, 1);
            mainGrid.Children.Add(bandwidthLabel);

            var bandwidthChart = ChartHelper.SetupNetworkChart(NetworkSendValues, NetworkReceiveValues, "Скорость (Кбит/с или Мбит/с)");
            Grid.SetRow(bandwidthChart, 2);
            mainGrid.Children.Add(bandwidthChart); // Исправлено: добавление графика завершено

            Grid dataGrid = new Grid();
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 300 });
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 200 });

            Grid leftGrid = new Grid();
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            SolidColorBrush labelColor = new SolidColorBrush(Color.FromRgb(105, 105, 105));
            SolidColorBrush valueColor = Brushes.Black;

            Grid receiveSpeedGrid = new Grid();
            receiveSpeedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            receiveSpeedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Line receiveLine = new Line { X1 = 0, Y1 = 0, X2 = 20, Y2 = 0, Stroke = Brushes.Purple, StrokeThickness = 2, Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(receiveLine, 0);
            receiveSpeedGrid.Children.Add(receiveLine);

            TextBlock receiveSpeedLabel = new TextBlock { Text = "Скорость получения", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(0, 2, 10, 2) };
            Grid.SetColumn(receiveSpeedLabel, 1);
            receiveSpeedGrid.Children.Add(receiveSpeedLabel);

            Grid.SetRow(receiveSpeedGrid, 0);
            Grid.SetColumn(receiveSpeedGrid, 0);
            leftGrid.Children.Add(receiveSpeedGrid);

            Grid sendSpeedGrid = new Grid();
            sendSpeedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            sendSpeedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Line sendLine = new Line { X1 = 0, Y1 = 0, X2 = 20, Y2 = 0, Stroke = Brushes.Purple, StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 2, 2 }, Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(sendLine, 0);
            sendSpeedGrid.Children.Add(sendLine);

            TextBlock sendSpeedLabel = new TextBlock { Text = "Скорость отправки", FontWeight = FontWeights.Bold, Foreground = labelColor, Margin = new Thickness(0, 2, 10, 2) };
            Grid.SetColumn(sendSpeedLabel, 1);
            sendSpeedGrid.Children.Add(sendSpeedLabel);

            Grid.SetRow(sendSpeedGrid, 0);
            Grid.SetColumn(sendSpeedGrid, 1);
            leftGrid.Children.Add(sendSpeedGrid);

            TextBlock receiveSpeedValue = new TextBlock { Text = "0 Кбит/с", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(5, 0, 10, 2) };
            Grid.SetRow(receiveSpeedValue, 1);
            Grid.SetColumn(receiveSpeedValue, 0);
            leftGrid.Children.Add(receiveSpeedValue);

            TextBlock sendSpeedValue = new TextBlock { Text = "0 Кбит/с", Foreground = valueColor, FontSize = 14, FontWeight = FontWeights.Normal, Margin = new Thickness(5, 0, 10, 2) };
            Grid.SetRow(sendSpeedValue, 1);
            Grid.SetColumn(sendSpeedValue, 1);
            leftGrid.Children.Add(sendSpeedValue);

            Grid.SetColumn(leftGrid, 0);
            dataGrid.Children.Add(leftGrid);

            StackPanel rightPanel = new StackPanel { Margin = new Thickness(0, 5, 5, 5) };
            rightPanel.Children.Add(ChartHelper.CreateDataRow("Адаптер:", networkInterface.Description, true, 12));
            if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                rightPanel.Children.Add(ChartHelper.CreateDataRow("SSID:", GetSSID() ?? "N/A", true, 12));
            rightPanel.Children.Add(ChartHelper.CreateDataRow("Тип подключения:", networkInterface.NetworkInterfaceType.ToString(), true, 12));
            rightPanel.Children.Add(ChartHelper.CreateDataRow("IPv4-адрес:", GetIPv4Address() ?? "N/A", true, 12));
            rightPanel.Children.Add(ChartHelper.CreateDataRow("IPv6-адрес:", GetIPv6Address() ?? "N/A", true, 12));
            Grid.SetColumn(rightPanel, 1);
            dataGrid.Children.Add(rightPanel);

            Grid.SetRow(dataGrid, 3);
            mainGrid.Children.Add(dataGrid);

            tab.Content = mainGrid;
            return tab;
        }

        public async Task Update()
        {
            if (!hasNetwork || Tab == null) return;

            var stats = networkInterface.GetIPv4Statistics();
            var currentTime = DateTime.Now;
            double timeElapsed = (currentTime - lastUpdateTime).TotalSeconds;

            long bytesSent = stats.BytesSent;
            long bytesReceived = stats.BytesReceived;

            double sendSpeedKbps = Math.Round((bytesSent - lastBytesSent) * 8.0 / timeElapsed / 1000, 1);
            double receiveSpeedKbps = Math.Round((bytesReceived - lastBytesReceived) * 8.0 / timeElapsed / 1000, 1);

            lastBytesSent = bytesSent;
            lastBytesReceived = bytesReceived;
            lastUpdateTime = currentTime;

            for (int i = 0; i < NetworkSendValues.Count - 1; i++)
            {
                NetworkSendValues[i] = NetworkSendValues[i + 1];
                NetworkReceiveValues[i] = NetworkReceiveValues[i + 1];
            }
            NetworkSendValues[59] = sendSpeedKbps;
            NetworkReceiveValues[59] = receiveSpeedKbps;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var mainGrid = Tab.Content as Grid;
                if (mainGrid == null) return;

                var bandwidthChart = mainGrid.Children.OfType<CartesianChart>().FirstOrDefault(c => Grid.GetRow(c) == 2);
                if (bandwidthChart != null)
                {
                    bandwidthChart.Series[0].Values = NetworkReceiveValues;
                    bandwidthChart.Series[1].Values = NetworkSendValues;

                    double maxSpeedKbps = Math.Max(NetworkReceiveValues.Max(), NetworkSendValues.Max());
                    double newMax = Math.Max(100, Math.Ceiling(maxSpeedKbps / 100) * 100);
                    if (bandwidthChart.AxisY[0].MaxValue != newMax)
                    {
                        bandwidthChart.AxisY[0].MaxValue = newMax;
                        bandwidthChart.AxisY[0].LabelFormatter = value => value < 1000 ? $"{value:F1} Кбит/с" : $"{(value / 1000):F1} Мбит/с";
                    }
                    bandwidthChart.Update(true, true);
                }

                var dataGrid = mainGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == 3);
                if (dataGrid != null)
                {
                    var leftGrid = dataGrid.Children.OfType<Grid>().FirstOrDefault(sp => Grid.GetColumn(sp) == 0);
                    if (leftGrid != null)
                    {
                        var receiveSpeedValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 1 && Grid.GetColumn(tb) == 0);
                        if (receiveSpeedValue != null)
                            receiveSpeedValue.Text = receiveSpeedKbps < 1000 ? $"{receiveSpeedKbps:F1} Кбит/с" : $"{(receiveSpeedKbps / 1000):F1} Мбит/с";

                        var sendSpeedValue = leftGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == 1 && Grid.GetColumn(tb) == 1);
                        if (sendSpeedValue != null)
                            sendSpeedValue.Text = sendSpeedKbps < 1000 ? $"{sendSpeedKbps:F1} Кбит/с" : $"{(sendSpeedKbps / 1000):F1} Мбит/с";
                    }
                }
            });
        }

        private string GetSSID()
        {
            if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211) return null;
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM MSNdis_80211_ServiceSetIdentifier WHERE Active = TRUE"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        byte[] ssidBytes = (byte[])obj["Ndis80211SsId"];
                        if (ssidBytes != null && ssidBytes.Length > 0)
                        {
                            return System.Text.Encoding.ASCII.GetString(ssidBytes).TrimEnd('\0');
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения SSID: {ex.Message}");
            }
            return "Unknown";
        }

        private string GetIPv4Address()
        {
            var ipProps = networkInterface.GetIPProperties();
            var ipv4 = ipProps.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            return ipv4?.Address.ToString();
        }

        private string GetIPv6Address()
        {
            var ipProps = networkInterface.GetIPProperties();
            var ipv6 = ipProps.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
            return ipv6?.Address.ToString();
        }

        public bool HasNetwork => hasNetwork;

        public void Dispose()
        {
        }
    }
}