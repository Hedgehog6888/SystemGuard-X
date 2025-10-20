using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Computer_Status_Viewer
{
    public class NavigationManager
    {
        private readonly TreeView treeView;
        private readonly ContentControl mainContentArea;
        private readonly TabControl performanceTabControl;
        private readonly TextBlock currentLocationTextBlock;
        private readonly SubcategoryManager subcategoryManager;
        private readonly SummaryManager summaryManager;
        private readonly CPUManager cpuManager;
        private readonly OSManager osManager;
        private readonly MotherboardManager motherboardManager;
        private readonly MemoryManager memoryManager;
        private readonly BIOSManager biosManager;
        private readonly PerformanceManager performanceManager;
        private readonly WorkTimeManager workTimeManager;
        private readonly RegionalSettingsManager regionalSettingsManager; // Новое поле

        public NavigationManager(
            TreeView treeView,
            ContentControl mainContentArea,
            TabControl performanceTabControl,
            TextBlock currentLocationTextBlock,
            SubcategoryManager subcategoryManager,
            SummaryManager summaryManager,
            CPUManager cpuManager,
            OSManager osManager,
            MotherboardManager motherboardManager,
            MemoryManager memoryManager,
            BIOSManager biosManager,
            PerformanceManager performanceManager,
            WorkTimeManager workTimeManager,
            RegionalSettingsManager regionalSettingsManager) // Добавляем в конструктор
        {
            this.treeView = treeView ?? throw new ArgumentNullException(nameof(treeView));
            this.mainContentArea = mainContentArea ?? throw new ArgumentNullException(nameof(mainContentArea));
            this.performanceTabControl = performanceTabControl ?? throw new ArgumentNullException(nameof(performanceTabControl));
            this.currentLocationTextBlock = currentLocationTextBlock ?? throw new ArgumentNullException(nameof(currentLocationTextBlock));
            this.subcategoryManager = subcategoryManager ?? throw new ArgumentNullException(nameof(subcategoryManager));
            this.summaryManager = summaryManager ?? throw new ArgumentNullException(nameof(summaryManager));
            this.cpuManager = cpuManager ?? throw new ArgumentNullException(nameof(cpuManager));
            this.osManager = osManager ?? throw new ArgumentNullException(nameof(osManager));
            this.motherboardManager = motherboardManager ?? throw new ArgumentNullException(nameof(motherboardManager));
            this.memoryManager = memoryManager ?? throw new ArgumentNullException(nameof(memoryManager));
            this.biosManager = biosManager ?? throw new ArgumentNullException(nameof(biosManager));
            this.performanceManager = performanceManager ?? throw new ArgumentNullException(nameof(performanceManager));
            this.workTimeManager = workTimeManager ?? throw new ArgumentNullException(nameof(workTimeManager));
            this.regionalSettingsManager = regionalSettingsManager ?? throw new ArgumentNullException(nameof(regionalSettingsManager));
        }

        public async void TreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem selectedItem)
            {
                string category = subcategoryManager.GetHeaderText(selectedItem);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Выбрана категория: {category}");

                // Останавливаем обновление всех менеджеров перед переключением
                summaryManager.StopUpdating();
                cpuManager.StopUpdating();
                osManager.StopUpdating();
                workTimeManager.StopUpdating();

                if (selectedItem.Parent is TreeViewItem parentItem)
                {
                    switch (category)
                    {
                        case "Производительность":
                            mainContentArea.Visibility = Visibility.Collapsed;
                            performanceTabControl.Visibility = Visibility.Visible;
                            await performanceManager.EnsureInitializedAsync();
                            break;

                        case "Суммарная информация":
                            mainContentArea.Content = summaryManager.CreateSummaryPanel();
                            mainContentArea.Visibility = Visibility.Visible;
                            performanceTabControl.Visibility = Visibility.Collapsed;
                            summaryManager.StartUpdating();
                            break;

                        case "ЦП":
                            mainContentArea.Content = cpuManager.CreateCPUPanel();
                            mainContentArea.Visibility = Visibility.Visible;
                            performanceTabControl.Visibility = Visibility.Collapsed;
                            cpuManager.StartUpdating();
                            break;

                        case "Системная плата":
                            mainContentArea.Content = motherboardManager.CreateMotherboardPanel();
                            mainContentArea.Visibility = Visibility.Visible;
                            performanceTabControl.Visibility = Visibility.Collapsed;
                            break;

                        case "Память":
                            mainContentArea.Content = memoryManager.CreateMemoryPanel();
                            mainContentArea.Visibility = Visibility.Visible;
                            performanceTabControl.Visibility = Visibility.Collapsed;
                            break;

                        case "BIOS":
                            mainContentArea.Content = biosManager.CreateBIOSPanel();
                            mainContentArea.Visibility = Visibility.Visible;
                            performanceTabControl.Visibility = Visibility.Collapsed;
                            break;

                        case "Операционная система":
                            try
                            {
                                mainContentArea.Content = osManager.CreateOSPanel();
                                mainContentArea.Visibility = Visibility.Visible;
                                performanceTabControl.Visibility = Visibility.Collapsed;
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Панель ОС успешно отображена");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ошибка при отображении панели ОС: {ex.Message}");
                                mainContentArea.Content = new TextBlock
                                {
                                    Text = "Ошибка при загрузке информации об ОС.",
                                    FontSize = 16,
                                    Margin = new Thickness(10)
                                };
                                mainContentArea.Visibility = Visibility.Visible;
                                performanceTabControl.Visibility = Visibility.Collapsed;
                            }
                            break;

                        case "Время работы":
                            mainContentArea.Content = workTimeManager.CreateWorkTimePanel();
                            mainContentArea.Visibility = Visibility.Visible;
                            performanceTabControl.Visibility = Visibility.Collapsed;
                            workTimeManager.StartUpdating();
                            break;

                        case "Региональные установки": // Добавляем обработку новой категории
                            mainContentArea.Content = regionalSettingsManager.CreateRegionalSettingsPanel();
                            mainContentArea.Visibility = Visibility.Visible;
                            performanceTabControl.Visibility = Visibility.Collapsed;
                            break;

                        default:
                            mainContentArea.Content = new TextBlock
                            {
                                Text = $"Информация для категории '{category}' будет добавлена позже!",
                                FontSize = 16,
                                Margin = new Thickness(10)
                            };
                            mainContentArea.Visibility = Visibility.Visible;
                            performanceTabControl.Visibility = Visibility.Collapsed;
                            break;
                    }
                    ExpandCategory(parentItem);
                }
                else
                {
                    subcategoryManager.UpdateSubcategoryButtons(selectedItem);
                    mainContentArea.Visibility = Visibility.Visible;
                    performanceTabControl.Visibility = Visibility.Collapsed;
                }

                UpdateCurrentLocation(selectedItem);
            }
        }

        private void ExpandCategory(TreeViewItem parentItem)
        {
            parentItem.IsExpanded = true;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Категория '{subcategoryManager.GetHeaderText(parentItem)}' развернута");
        }

        private void UpdateCurrentLocation(TreeViewItem selectedItem)
        {
            string location = subcategoryManager.GetHeaderText(selectedItem);
            if (selectedItem.Parent is TreeViewItem parentItem)
            {
                string parentCategory = subcategoryManager.GetHeaderText(parentItem);
                location = $"{parentCategory} > {location}";
            }
            currentLocationTextBlock.Text = location;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Текущее местоположение обновлено: {location}");
        }
    }
}