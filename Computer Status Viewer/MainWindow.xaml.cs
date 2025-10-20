using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Computer_Status_Viewer.Properties;
using WinForms = System.Windows.Forms;
using WPF = System.Windows;

namespace Computer_Status_Viewer
{
    public partial class MainWindow : Window
    {
        private bool isCpuWidgetActive = false;
        private bool isRamWidgetActive = false;
        private bool isDiskWidgetActive = false;

        private Lazy<NavigationManager> _navigationManager;
        private Lazy<SubcategoryManager> _subcategoryManager;
        private Lazy<PerformanceManager> _performanceManager;
        private Lazy<SummaryManager> _summaryManager;
        private Lazy<CPUManager> _cpuManager;
        private Lazy<MotherboardManager> _motherboardManager;
        private Lazy<MemoryManager> _memoryManager;
        private Lazy<BIOSManager> _biosManager;
        private Lazy<OSManager> _osManager;
        private Lazy<WorkTimeManager> _workTimeManager;
        private Lazy<RegionalSettingsManager> _regionalSettingsManager;
        private WidgetManager _widgetManager;
        private DispatcherTimer timer;
        private DateTime lastUpdateTime;

        private HashSet<string> loadedTabs = new HashSet<string>();
        private static readonly System.Drawing.Icon TrayIconImage = new System.Drawing.Icon(WPF.Application.GetResourceStream(new Uri("pack://application:,,,/Ico/Logo.ico")).Stream);

        private readonly List<Button> _subcategoryButtons = new List<Button>();
        public ICommand CopyLocationCommand { get; }

        public MainWindow(WidgetManager widgetManager)
        {
            InitializeComponent();

            timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            lastUpdateTime = DateTime.Now;
            timer.Start();

            _widgetManager = widgetManager ?? new WidgetManager();
            _summaryManager = new Lazy<SummaryManager>(() => new SummaryManager(Resources));
            _performanceManager = new Lazy<PerformanceManager>(() => new PerformanceManager(PerformanceTabControl));
            _cpuManager = new Lazy<CPUManager>(() => new CPUManager(Resources));
            _motherboardManager = new Lazy<MotherboardManager>(() => new MotherboardManager(Resources));
            _memoryManager = new Lazy<MemoryManager>(() => new MemoryManager(Resources));
            _biosManager = new Lazy<BIOSManager>(() => new BIOSManager(Resources));
            _osManager = new Lazy<OSManager>(() => new OSManager(Resources));
            _workTimeManager = new Lazy<WorkTimeManager>(() => new WorkTimeManager(Resources));
            _regionalSettingsManager = new Lazy<RegionalSettingsManager>(() => new RegionalSettingsManager(Resources));
            _subcategoryManager = new Lazy<SubcategoryManager>(() => new SubcategoryManager(MainContentArea, PerformanceTabControl, _performanceManager.Value, _summaryManager.Value, CategoryTreeView));
            _navigationManager = new Lazy<NavigationManager>(() => new NavigationManager(
                CategoryTreeView,
                MainContentArea,
                PerformanceTabControl,
                CurrentLocation,
                _subcategoryManager.Value,
                _summaryManager.Value,
                _cpuManager.Value,
                _osManager.Value,
                _motherboardManager.Value,
                _memoryManager.Value,
                _biosManager.Value,
                _performanceManager.Value,
                _workTimeManager.Value,
                _regionalSettingsManager.Value
            ));

            isCpuWidgetActive = Settings.Default.IsCpuWidgetActive;
            isRamWidgetActive = Settings.Default.IsRamWidgetActive;
            isDiskWidgetActive = Settings.Default.IsDiskWidgetActive;

            CopyLocationCommand = new RelayCommand(() =>
            {
                Clipboard.SetText(CurrentLocation.Text);
            });

            CategoryTreeView.SelectedItemChanged += TreeView_SelectedItemChanged;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            if ((now - lastUpdateTime).TotalSeconds > 1.1)
            {
                Debug.WriteLine($"[{now:HH:mm:ss.fff}] Time drift detected: {(now - lastUpdateTime).TotalSeconds:F3}s");
            }
            CurrentTime.Text = now.ToString("Дата: dd.MM.yyyy | Время: HH:mm:ss");
            lastUpdateTime = now;
            Debug.WriteLine($"[{now:HH:mm:ss.fff}] Time updated");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public Task InitializeAsync()
        {
            try
            {
                if (DesignerProperties.GetIsInDesignMode(this))
                {
                    PerformanceTabControl.DataContext = _performanceManager.Value;
                }

                CpuToggle.IsChecked = isCpuWidgetActive;
                RamToggle.IsChecked = isRamWidgetActive;
                DiskToggle.IsChecked = isDiskWidgetActive;

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ошибка в InitializeAsync: {ex}");
                throw;
            }
        }

        public async Task LoadContentAsync()
        {
            try
            {
                ShowComputerTiles();
                if (CategoryTreeView.Items.Count > 0)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ((TreeViewItem)CategoryTreeView.Items[0]).IsSelected = true;
                    }, DispatcherPriority.Background);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ошибка в LoadContentAsync: {ex}");
                throw;
            }
        }

        private void ShowComputerTiles()
        {
            MainContentArea.Content = null;
            MainContent.Visibility = Visibility.Visible;
            PerformanceTabControl.Visibility = Visibility.Collapsed;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var subcategories = new[]
            {
                new { Name = "Производительность", Icon = "Ico/Efficiency.png", Tooltip = "Просмотр текущей производительности системы" },
                new { Name = "Суммарная информация", Icon = "Ico/Comp.png", Tooltip = "Общая информация о компьютере" },
                new { Name = "ЦП", Icon = "Ico/CPU.png", Tooltip = "Информация о процессоре" },
                new { Name = "Системная плата", Icon = "Ico/Payment.png", Tooltip = "Информация о системной плате" },
                new { Name = "Память", Icon = "Ico/Memory.png", Tooltip = "Информация о памяти" },
                new { Name = "BIOS", Icon = "Ico/BIOS.png", Tooltip = "Информация о BIOS" },
                new { Name = "Операционная система", Icon = "Ico/OS.png", Tooltip = "Информация об операционной системе" },
                new { Name = "Время работы", Icon = "Ico/Time.png", Tooltip = "Информация о времени работы системы" },
                new { Name = "Региональные установки", Icon = "Ico/Region.png", Tooltip = "Информация о региональных настройках системы" },
                new { Name = "Разгон", Icon = "Ico/Overclocking.png", Tooltip = "Настройки разгона компонентов" },
                new { Name = "Датчики", Icon = "Ico/Sensors.png", Tooltip = "Данные с датчиков температуры и нагрузки" }
            };

            var imageSources = new BitmapImage[subcategories.Length];
            for (int i = 0; i < subcategories.Length; i++)
            {
                try
                {
                    var uri = new Uri($"pack://application:,,,/{subcategories[i].Icon}", UriKind.Absolute);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = uri;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imageSources[i] = bitmap;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка загрузки иконки {subcategories[i].Icon}: {ex.Message}");
                    imageSources[i] = null;
                }
            }

            _subcategoryButtons.Clear();

            for (int i = 0; i < subcategories.Length; i++)
            {
                var button = new Button
                {
                    Style = (Style)Application.Current.Resources["SubcategoryButtonStyle"],
                    Tag = subcategories[i].Name,
                    ToolTip = subcategories[i].Tooltip,
                    Opacity = 0
                };

                var buttonGrid = new Grid();
                buttonGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                buttonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                var image = new System.Windows.Controls.Image
                {
                    Source = imageSources[i],
                    Width = 48,
                    Height = 48,
                    Margin = new Thickness(0, 0, 0, 10),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(image, 0);

                var textBlock = new TextBlock
                {
                    Text = subcategories[i].Name,
                    Style = (Style)Application.Current.Resources["SubcategoryTextStyle"]
                };
                Grid.SetRow(textBlock, 1);

                if (subcategories[i].Name == "Производительность")
                {
                    var statusIndicator = new Ellipse
                    {
                        Style = (Style)Application.Current.Resources["StatusIndicatorStyle"],
                        Fill = GetPerformanceStatusColor()
                    };
                    buttonGrid.Children.Add(statusIndicator);
                }

                buttonGrid.Children.Add(image);
                buttonGrid.Children.Add(textBlock);
                button.Content = buttonGrid;

                button.Click += (s, e) =>
                {
                    var selectedButton = s as Button;
                    var selectedText = selectedButton.Tag.ToString();
                    Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Нажата кнопка: {selectedText}");
                    foreach (TreeViewItem item in ((TreeViewItem)CategoryTreeView.Items[0]).Items)
                    {
                        var header = (item.Header as StackPanel)?.Children[1] as TextBlock;
                        if (header != null && header.Text == selectedText)
                        {
                            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Найден элемент TreeView: {selectedText}");
                            item.IsSelected = true;
                            break;
                        }
                    }
                };

                var storyboard = new Storyboard();
                var opacityAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    BeginTime = TimeSpan.FromSeconds(i * 0.1)
                };
                Storyboard.SetTarget(opacityAnimation, button);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
                storyboard.Children.Add(opacityAnimation);
                button.Loaded += (s, e) => storyboard.Begin();

                _subcategoryButtons.Add(button);

                Grid.SetColumn(button, i % 2);
                Grid.SetRow(button, i / 2);
                grid.Children.Add(button);
            }

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = grid
            };

            MainContent.Content = scrollViewer;
            MainContentArea.Content = MainContent;
        }

        private System.Windows.Media.Brush GetPerformanceStatusColor()
        {
            bool isHighLoad = false; // Замените на реальную проверку
            return isHighLoad
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red)
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
        }

        private TreeViewItem _lastSelectedItem = null;

        private async void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = e.NewValue as TreeViewItem;
            if (selectedItem == null || selectedItem == _lastSelectedItem)
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Выбранный элемент null или не изменился");
                return;
            }

            _lastSelectedItem = selectedItem;

            var header = (selectedItem.Header as StackPanel)?.Children[1] as TextBlock;
            if (header == null)
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Не удалось получить заголовок элемента");
                return;
            }

            string selectedText = header.Text;
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Выбран элемент: {selectedText}");

            // Получаем иконку из TreeViewItem
            var icon = (selectedItem.Header as StackPanel)?.Children[0] as Image;
            if (icon != null && icon.Source != null)
            {
                CurrentIcon.Source = icon.Source;
            }
            else
            {
                CurrentIcon.Source = null; // Сбрасываем иконку, если её нет
            }
            CurrentLocation.Text = selectedText; // Обновляем текст пути

            foreach (var button in _subcategoryButtons)
            {
                button.Tag = button.Tag.ToString() == selectedText ? "Selected" : button.Tag.ToString();
            }

            string[] mainCategories = new[] { "Компьютер", "Системная плата", "Операционная система", "Дисплей", "Хранение данных", "Сеть", "Устройства", "Конфигурация" };
            string[] delayedItems = new[] { "Производительность", "Суммарная информация", "ЦП", "Системная плата", "Память", "BIOS", "Операционная система", "Время работы", "Региональные установки", "Разгон", "Датчики" };

            try
            {
                if (mainCategories.Contains(selectedText))
                {
                    Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Main category selected: {selectedText}");
                    await Dispatcher.InvokeAsync(() =>
                    {
                        MainContentArea.Content = null;
                        MainContent.Visibility = Visibility.Collapsed;
                        PerformanceTabControl.Visibility = Visibility.Collapsed;

                        var startTime = DateTime.Now;
                        _navigationManager.Value.TreeViewSelectedItemChanged(sender, e);
                        var duration = DateTime.Now - startTime;
                        Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] NavigationManager.TreeViewSelectedItemChanged took {duration.TotalMilliseconds:F2}ms");

                        MainContent.Visibility = Visibility.Visible;
                        PerformanceTabControl.Visibility = Visibility.Collapsed;
                    }, DispatcherPriority.Background);
                }
                else if (delayedItems.Contains(selectedText) && !loadedTabs.Contains(selectedText))
                {
                    Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Showing loading indicator for {selectedText}");
                    await Dispatcher.InvokeAsync(() =>
                    {
                        MainContentArea.Content = null;
                        LoadingIndicator.Visibility = Visibility.Visible;
                        MainContent.Visibility = Visibility.Collapsed;
                        PerformanceTabControl.Visibility = Visibility.Collapsed;
                    }, DispatcherPriority.Render);

                    Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Starting heavy load for {selectedText}");
                    await Task.Run(async () =>
                    {
                        await Task.Delay(1500);

                        await Dispatcher.InvokeAsync(() =>
                        {
                            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Updating UI after heavy load for {selectedText}");
                            var startTime = DateTime.Now;
                            _navigationManager.Value.TreeViewSelectedItemChanged(sender, e);
                            var duration = DateTime.Now - startTime;
                            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] NavigationManager.TreeViewSelectedItemChanged took {duration.TotalMilliseconds:F2}ms");

                            if (selectedText == "Производительность")
                            {
                                PerformanceTabControl.Visibility = Visibility.Visible;
                                MainContent.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                MainContent.Visibility = Visibility.Visible;
                                PerformanceTabControl.Visibility = Visibility.Collapsed;
                            }

                            loadedTabs.Add(selectedText);
                            LoadingIndicator.Visibility = Visibility.Collapsed;
                        }, DispatcherPriority.Background);
                    });
                }
                else
                {
                    Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Starting quick load or reload for {selectedText}");
                    await Dispatcher.InvokeAsync(() =>
                    {
                        MainContentArea.Content = null;
                        MainContent.Visibility = Visibility.Collapsed;
                        PerformanceTabControl.Visibility = Visibility.Collapsed;

                        Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Updating UI for {selectedText}");
                        var startTime = DateTime.Now;
                        _navigationManager.Value.TreeViewSelectedItemChanged(sender, e);
                        var duration = DateTime.Now - startTime;
                        Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] NavigationManager.TreeViewSelectedItemChanged took {duration.TotalMilliseconds:F2}ms");

                        if (selectedText == "Производительность")
                        {
                            PerformanceTabControl.Visibility = Visibility.Visible;
                            MainContent.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            MainContent.Visibility = Visibility.Visible;
                            PerformanceTabControl.Visibility = Visibility.Collapsed;
                        }
                    }, DispatcherPriority.Background);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ошибка в TreeView_SelectedItemChanged: {ex}");
            }
        }

        private void TreeView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is TreeView treeView && treeView.Parent is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void CpuWidgetButton_Click(object sender, RoutedEventArgs e)
        {
            isCpuWidgetActive = !isCpuWidgetActive;
            CpuToggle.IsChecked = isCpuWidgetActive;
            Settings.Default.IsCpuWidgetActive = isCpuWidgetActive;
            Settings.Default.Save();
            _widgetManager.CreateWidget1();
        }

        private void RamWidgetButton_Click(object sender, RoutedEventArgs e)
        {
            isRamWidgetActive = !isRamWidgetActive;
            RamToggle.IsChecked = isRamWidgetActive;
            Settings.Default.IsRamWidgetActive = isRamWidgetActive;
            Settings.Default.Save();
            _widgetManager.CreateWidget2();
        }

        private void DiskWidgetButton_Click(object sender, RoutedEventArgs e)
        {
            isDiskWidgetActive = !isDiskWidgetActive;
            DiskToggle.IsChecked = isDiskWidgetActive;
            Settings.Default.IsDiskWidgetActive = isDiskWidgetActive;
            Settings.Default.Save();
            _widgetManager.CreateWidget3();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_performanceManager.IsValueCreated) _performanceManager.Value.Dispose();
            if (_summaryManager.IsValueCreated) _summaryManager.Value.Dispose();
            if (_cpuManager.IsValueCreated) _cpuManager.Value.Dispose();
            if (_motherboardManager.IsValueCreated) _motherboardManager.Value.Dispose();
            if (_memoryManager.IsValueCreated) _memoryManager.Value.Dispose();
            if (_workTimeManager.IsValueCreated) _workTimeManager.Value.Dispose();
            if (_regionalSettingsManager.IsValueCreated) _regionalSettingsManager.Value.Dispose();
            base.OnClosed(e);
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute();

#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67
    }
}