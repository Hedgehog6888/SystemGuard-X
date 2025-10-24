using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Computer_Status_Viewer.Reports
{
    /// <summary>
    /// Расширенный диалог для выбора критериев пользовательского отчёта
    /// </summary>
    public partial class CustomReportCriteriaDialog : Window
    {
        public string ReportTitle { get; private set; }
        public string ReportDescription { get; private set; }
        public List<string> SelectedCriteria { get; private set; }
        public string ReportCategory { get; private set; }
        public string ReportFormat { get; private set; }
        public Dictionary<string, object> AdvancedSettings { get; private set; }

        public CustomReportCriteriaDialog()
        {
            InitializeComponent();
            SelectedCriteria = new List<string>();
            AdvancedSettings = new Dictionary<string, object>();
            InitializeDefaultSettings();
        }

        /// <summary>
        /// Инициализация настроек по умолчанию
        /// </summary>
        private void InitializeDefaultSettings()
        {
            // Устанавливаем даты по умолчанию
            StartDatePicker.SelectedDate = DateTime.Now.AddDays(-7);
            EndDatePicker.SelectedDate = DateTime.Now;
        }

        /// <summary>
        /// Обработчик кнопки "Создать"
        /// </summary>
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем основную информацию
                ReportTitle = ReportTitleTextBox.Text.Trim();
                ReportDescription = ReportDescriptionTextBox.Text.Trim();
                ReportCategory = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Общая диагностика";

                if (string.IsNullOrEmpty(ReportTitle))
                {
                    MessageBox.Show("Пожалуйста, введите название отчёта.", "Ошибка", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Собираем выбранные критерии
                CollectSelectedCriteria();

                // Собираем расширенные настройки
                CollectAdvancedSettings();

                // Определяем формат отчёта
                DetermineReportFormat();

                // Проверяем, что выбрано хотя бы один критерий
                if (SelectedCriteria.Count == 0)
                {
                    MessageBox.Show("Пожалуйста, выберите хотя бы один критерий для включения в отчёт.", 
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Закрываем диалог с результатом OK
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчёта: {ex.Message}", "Ошибка", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Сбор выбранных критериев
        /// </summary>
        private void CollectSelectedCriteria()
        {
            SelectedCriteria.Clear();

            // Системная информация
            if (IncludeOSInfo.IsChecked == true) SelectedCriteria.Add("OS_INFO");
            if (IncludeComputerInfo.IsChecked == true) SelectedCriteria.Add("COMPUTER_INFO");
            if (IncludeUserInfo.IsChecked == true) SelectedCriteria.Add("USER_INFO");
            if (IncludeDateTimeInfo.IsChecked == true) SelectedCriteria.Add("DATETIME_INFO");
            if (IncludeEnvironmentInfo.IsChecked == true) SelectedCriteria.Add("ENVIRONMENT_INFO");

            // Аппаратная информация
            if (IncludeCPUInfo.IsChecked == true) SelectedCriteria.Add("CPU_INFO");
            if (IncludeMemoryInfo.IsChecked == true) SelectedCriteria.Add("MEMORY_INFO");
            if (IncludeDiskInfo.IsChecked == true) SelectedCriteria.Add("DISK_INFO");
            if (IncludeGPUInfo.IsChecked == true) SelectedCriteria.Add("GPU_INFO");
            if (IncludeMotherboardInfo.IsChecked == true) SelectedCriteria.Add("MOTHERBOARD_INFO");
            if (IncludeBiosInfo.IsChecked == true) SelectedCriteria.Add("BIOS_INFO");
            if (IncludeUSBInfo.IsChecked == true) SelectedCriteria.Add("USB_INFO");

            // Производительность
            if (IncludePerformanceInfo.IsChecked == true) SelectedCriteria.Add("PERFORMANCE_INFO");
            if (IncludeNetworkInfo.IsChecked == true) SelectedCriteria.Add("NETWORK_INFO");
            if (IncludeProcessInfo.IsChecked == true) SelectedCriteria.Add("PROCESS_INFO");
            if (IncludeServiceInfo.IsChecked == true) SelectedCriteria.Add("SERVICE_INFO");
            if (IncludeStartupInfo.IsChecked == true) SelectedCriteria.Add("STARTUP_INFO");

            // Безопасность
            if (IncludeSecurityInfo.IsChecked == true) SelectedCriteria.Add("SECURITY_INFO");
            if (IncludeFirewallInfo.IsChecked == true) SelectedCriteria.Add("FIREWALL_INFO");
            if (IncludeAntivirusInfo.IsChecked == true) SelectedCriteria.Add("ANTIVIRUS_INFO");
            if (IncludeUpdatesInfo.IsChecked == true) SelectedCriteria.Add("UPDATES_INFO");

            // Дополнительные опции
            if (IncludeScreenshots.IsChecked == true) SelectedCriteria.Add("SCREENSHOTS");
            if (IncludeLogs.IsChecked == true) SelectedCriteria.Add("LOGS");
            if (IncludeRegistryInfo.IsChecked == true) SelectedCriteria.Add("REGISTRY_INFO");
            if (IncludeInstalledSoftware.IsChecked == true) SelectedCriteria.Add("INSTALLED_SOFTWARE");
            
            // Новые критерии
            if (IncludeSystemServices.IsChecked == true) SelectedCriteria.Add("SYSTEM_SERVICES");
            if (IncludeWindowsFeatures.IsChecked == true) SelectedCriteria.Add("WINDOWS_FEATURES");
            if (IncludeDeviceDrivers.IsChecked == true) SelectedCriteria.Add("DEVICE_DRIVERS");
            if (IncludePowerSettings.IsChecked == true) SelectedCriteria.Add("POWER_SETTINGS");
            if (IncludeDisplaySettings.IsChecked == true) SelectedCriteria.Add("DISPLAY_SETTINGS");
            if (IncludeAudioDevices.IsChecked == true) SelectedCriteria.Add("AUDIO_DEVICES");
            if (IncludePrinters.IsChecked == true) SelectedCriteria.Add("PRINTERS");
            if (IncludeWindowsUpdates.IsChecked == true) SelectedCriteria.Add("WINDOWS_UPDATES");
            if (IncludeTaskScheduler.IsChecked == true) SelectedCriteria.Add("TASK_SCHEDULER");
        }

        /// <summary>
        /// Сбор расширенных настроек
        /// </summary>
        private void CollectAdvancedSettings()
        {
            AdvancedSettings.Clear();

            // Временные фильтры
            AdvancedSettings["UseTimeFilter"] = UseTimeFilter.IsChecked == true;
            if (UseTimeFilter.IsChecked == true)
            {
                AdvancedSettings["StartDate"] = StartDatePicker.SelectedDate;
                AdvancedSettings["EndDate"] = EndDatePicker.SelectedDate;
            }

            // Фильтры производительности
            AdvancedSettings["IncludeOnlyHighCPU"] = IncludeOnlyHighCPU.IsChecked == true;
            AdvancedSettings["IncludeOnlyHighMemory"] = IncludeOnlyHighMemory.IsChecked == true;
            AdvancedSettings["IncludeOnlyRunningServices"] = IncludeOnlyRunningServices.IsChecked == true;

            // Фильтры дисков
            AdvancedSettings["IncludeOnlySystemDrives"] = IncludeOnlySystemDrives.IsChecked == true;
            AdvancedSettings["IncludeOnlyRemovableDrives"] = IncludeOnlyRemovableDrives.IsChecked == true;
            AdvancedSettings["IncludeOnlyLowSpaceDrives"] = IncludeOnlyLowSpaceDrives.IsChecked == true;

            // Новые фильтры процессов
            AdvancedSettings["IncludeOnlySystemProcesses"] = IncludeOnlySystemProcesses.IsChecked == true;
            AdvancedSettings["IncludeOnlyUserProcesses"] = IncludeOnlyUserProcesses.IsChecked == true;
            AdvancedSettings["IncludeOnlyActiveProcesses"] = IncludeOnlyActiveProcesses.IsChecked == true;
            
            // Новые фильтры сети
            AdvancedSettings["IncludeOnlyActiveNetworkAdapters"] = IncludeOnlyActiveNetworkAdapters.IsChecked == true;
            AdvancedSettings["IncludeOnlyWirelessAdapters"] = IncludeOnlyWirelessAdapters.IsChecked == true;
            AdvancedSettings["IncludeOnlyEthernetAdapters"] = IncludeOnlyEthernetAdapters.IsChecked == true;
            
            // Новые фильтры безопасности
            AdvancedSettings["IncludeOnlyActiveAntivirus"] = IncludeOnlyActiveAntivirus.IsChecked == true;
            AdvancedSettings["IncludeOnlyEnabledFirewall"] = IncludeOnlyEnabledFirewall.IsChecked == true;
            AdvancedSettings["IncludeOnlyCriticalServices"] = IncludeOnlyCriticalServices.IsChecked == true;
            
            // Настройки детализации
            AdvancedSettings["DetailLevel"] = (DetailLevelComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Базовая";
            AdvancedSettings["IncludeRawData"] = IncludeRawData.IsChecked == true;
            AdvancedSettings["IncludeStatistics"] = IncludeStatistics.IsChecked == true;
            AdvancedSettings["IncludeRecommendations"] = IncludeRecommendations.IsChecked == true;
            AdvancedSettings["IncludePerformanceMetrics"] = IncludePerformanceMetrics.IsChecked == true;
            AdvancedSettings["IncludeHealthChecks"] = IncludeHealthChecks.IsChecked == true;

            // Настройки уведомлений
            AdvancedSettings["NotifyOnCompletion"] = true; // По умолчанию включено
            AdvancedSettings["AutoOpenReport"] = true; // По умолчанию включено
        }

        /// <summary>
        /// Определение формата отчёта (по умолчанию txt)
        /// </summary>
        private void DetermineReportFormat()
        {
            ReportFormat = "txt";
        }

        /// <summary>
        /// Обработчик кнопки "Отмена"
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Обработчик выбора предустановки
        /// </summary>
        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = PresetComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;

            string preset = selectedItem.Content.ToString();
            
            switch (preset)
            {
                case "Быстрый обзор системы":
                    ApplyQuickSystemPreset();
                    break;
                case "Полная диагностика":
                    ApplyFullDiagnosticPreset();
                    break;
                case "Производительность":
                    ApplyPerformancePreset();
                    break;
                case "Безопасность":
                    ApplySecurityPreset();
                    break;
                case "Сетевая информация":
                    ApplyNetworkPreset();
                    break;
                case "Пользовательские данные":
                    ApplyUserDataPreset();
                    break;
            }
        }

        /// <summary>
        /// Применение предустановки "Быстрый обзор системы"
        /// </summary>
        private void ApplyQuickSystemPreset()
        {
            // Основная информация
            ReportTitleTextBox.Text = "Быстрый обзор системы";
            ReportDescriptionTextBox.Text = "Базовый обзор системной информации";
            CategoryComboBox.SelectedIndex = 0; // Общая диагностика

            // Критерии
            IncludeOSInfo.IsChecked = true;
            IncludeComputerInfo.IsChecked = true;
            IncludeUserInfo.IsChecked = true;
            IncludeDateTimeInfo.IsChecked = true;
            IncludeEnvironmentInfo.IsChecked = false;

            IncludeCPUInfo.IsChecked = true;
            IncludeMemoryInfo.IsChecked = true;
            IncludeDiskInfo.IsChecked = true;
            IncludeGPUInfo.IsChecked = false;
            IncludeMotherboardInfo.IsChecked = false;
            IncludeBiosInfo.IsChecked = false;
            IncludeUSBInfo.IsChecked = false;

            IncludePerformanceInfo.IsChecked = false;
            IncludeNetworkInfo.IsChecked = false;
            IncludeProcessInfo.IsChecked = false;
            IncludeServiceInfo.IsChecked = false;
            IncludeStartupInfo.IsChecked = false;

            IncludeSecurityInfo.IsChecked = false;
            IncludeFirewallInfo.IsChecked = false;
            IncludeAntivirusInfo.IsChecked = false;
            IncludeUpdatesInfo.IsChecked = false;

            IncludeScreenshots.IsChecked = false;
            IncludeLogs.IsChecked = false;
            IncludeRegistryInfo.IsChecked = false;
            IncludeInstalledSoftware.IsChecked = false;
            
            // Новые критерии
            IncludeSystemServices.IsChecked = false;
            IncludeWindowsFeatures.IsChecked = false;
            IncludeDeviceDrivers.IsChecked = false;
            IncludePowerSettings.IsChecked = false;
            IncludeDisplaySettings.IsChecked = false;
            IncludeAudioDevices.IsChecked = false;
            IncludePrinters.IsChecked = false;
            IncludeWindowsUpdates.IsChecked = false;
            IncludeTaskScheduler.IsChecked = false;
        }

        /// <summary>
        /// Применение предустановки "Полная диагностика"
        /// </summary>
        private void ApplyFullDiagnosticPreset()
        {
            ReportTitleTextBox.Text = "Полная диагностика системы";
            ReportDescriptionTextBox.Text = "Комплексная диагностика всех компонентов системы";
            CategoryComboBox.SelectedIndex = 0;

            // Выбираем всё
            SelectAllCheckboxes();
        }

        /// <summary>
        /// Применение предустановки "Производительность"
        /// </summary>
        private void ApplyPerformancePreset()
        {
            ReportTitleTextBox.Text = "Анализ производительности";
            ReportDescriptionTextBox.Text = "Детальный анализ производительности системы";
            CategoryComboBox.SelectedIndex = 1; // Производительность

            // Основные критерии
            IncludeOSInfo.IsChecked = true;
            IncludeComputerInfo.IsChecked = true;
            IncludeCPUInfo.IsChecked = true;
            IncludeMemoryInfo.IsChecked = true;
            IncludeDiskInfo.IsChecked = true;
            IncludePerformanceInfo.IsChecked = true;
            IncludeProcessInfo.IsChecked = true;
            IncludeServiceInfo.IsChecked = true;
            IncludeNetworkInfo.IsChecked = true;

            // Остальные выключаем
            IncludeGPUInfo.IsChecked = false;
            IncludeMotherboardInfo.IsChecked = false;
            IncludeSecurityInfo.IsChecked = false;
            IncludeScreenshots.IsChecked = false;
        }

        /// <summary>
        /// Применение предустановки "Безопасность"
        /// </summary>
        private void ApplySecurityPreset()
        {
            ReportTitleTextBox.Text = "Анализ безопасности";
            ReportDescriptionTextBox.Text = "Проверка настроек безопасности системы";
            CategoryComboBox.SelectedIndex = 2; // Безопасность

            IncludeOSInfo.IsChecked = true;
            IncludeComputerInfo.IsChecked = true;
            IncludeUserInfo.IsChecked = true;
            IncludeSecurityInfo.IsChecked = true;
            IncludeFirewallInfo.IsChecked = true;
            IncludeAntivirusInfo.IsChecked = true;
            IncludeUpdatesInfo.IsChecked = true;
            IncludeProcessInfo.IsChecked = true;
            IncludeServiceInfo.IsChecked = true;
            IncludeRegistryInfo.IsChecked = true;

            IncludeCPUInfo.IsChecked = false;
            IncludeMemoryInfo.IsChecked = false;
            IncludeDiskInfo.IsChecked = false;
            IncludePerformanceInfo.IsChecked = false;
        }

        /// <summary>
        /// Применение предустановки "Сетевая информация"
        /// </summary>
        private void ApplyNetworkPreset()
        {
            ReportTitleTextBox.Text = "Сетевая диагностика";
            ReportDescriptionTextBox.Text = "Анализ сетевых подключений и настроек";
            CategoryComboBox.SelectedIndex = 3; // Сетевая диагностика

            IncludeOSInfo.IsChecked = true;
            IncludeComputerInfo.IsChecked = true;
            IncludeNetworkInfo.IsChecked = true;
            IncludeProcessInfo.IsChecked = true;
            IncludeServiceInfo.IsChecked = true;

            IncludeCPUInfo.IsChecked = false;
            IncludeMemoryInfo.IsChecked = false;
            IncludeDiskInfo.IsChecked = false;
            IncludePerformanceInfo.IsChecked = false;
        }

        /// <summary>
        /// Применение предустановки "Пользовательские данные"
        /// </summary>
        private void ApplyUserDataPreset()
        {
            ReportTitleTextBox.Text = "Пользовательские данные";
            ReportDescriptionTextBox.Text = "Информация о пользователе и его настройках";
            CategoryComboBox.SelectedIndex = 0;

            IncludeUserInfo.IsChecked = true;
            IncludeEnvironmentInfo.IsChecked = true;
            IncludeInstalledSoftware.IsChecked = true;
            IncludeStartupInfo.IsChecked = true;
            IncludeRegistryInfo.IsChecked = true;

            IncludeOSInfo.IsChecked = false;
            IncludeComputerInfo.IsChecked = false;
            IncludeCPUInfo.IsChecked = false;
            IncludeMemoryInfo.IsChecked = false;
            IncludeDiskInfo.IsChecked = false;
        }

        /// <summary>
        /// Выбор всех чекбоксов
        /// </summary>
        private void SelectAllCheckboxes()
        {
            IncludeOSInfo.IsChecked = true;
            IncludeComputerInfo.IsChecked = true;
            IncludeUserInfo.IsChecked = true;
            IncludeDateTimeInfo.IsChecked = true;
            IncludeEnvironmentInfo.IsChecked = true;
            IncludeCPUInfo.IsChecked = true;
            IncludeMemoryInfo.IsChecked = true;
            IncludeDiskInfo.IsChecked = true;
            IncludeGPUInfo.IsChecked = true;
            IncludeMotherboardInfo.IsChecked = true;
            IncludeBiosInfo.IsChecked = true;
            IncludeUSBInfo.IsChecked = true;
            IncludePerformanceInfo.IsChecked = true;
            IncludeNetworkInfo.IsChecked = true;
            IncludeProcessInfo.IsChecked = true;
            IncludeServiceInfo.IsChecked = true;
            IncludeStartupInfo.IsChecked = true;
            IncludeSecurityInfo.IsChecked = true;
            IncludeFirewallInfo.IsChecked = true;
            IncludeAntivirusInfo.IsChecked = true;
            IncludeUpdatesInfo.IsChecked = true;
            IncludeScreenshots.IsChecked = true;
            IncludeLogs.IsChecked = true;
            IncludeRegistryInfo.IsChecked = true;
            IncludeInstalledSoftware.IsChecked = true;
            
            // Новые критерии
            IncludeSystemServices.IsChecked = true;
            IncludeWindowsFeatures.IsChecked = true;
            IncludeDeviceDrivers.IsChecked = true;
            IncludePowerSettings.IsChecked = true;
            IncludeDisplaySettings.IsChecked = true;
            IncludeAudioDevices.IsChecked = true;
            IncludePrinters.IsChecked = true;
            IncludeWindowsUpdates.IsChecked = true;
            IncludeTaskScheduler.IsChecked = true;
        }

        /// <summary>
        /// Очистка всех чекбоксов
        /// </summary>
        private void ClearAllCheckboxes()
        {
            IncludeOSInfo.IsChecked = false;
            IncludeComputerInfo.IsChecked = false;
            IncludeUserInfo.IsChecked = false;
            IncludeDateTimeInfo.IsChecked = false;
            IncludeEnvironmentInfo.IsChecked = false;
            IncludeCPUInfo.IsChecked = false;
            IncludeMemoryInfo.IsChecked = false;
            IncludeDiskInfo.IsChecked = false;
            IncludeGPUInfo.IsChecked = false;
            IncludeMotherboardInfo.IsChecked = false;
            IncludeBiosInfo.IsChecked = false;
            IncludeUSBInfo.IsChecked = false;
            IncludePerformanceInfo.IsChecked = false;
            IncludeNetworkInfo.IsChecked = false;
            IncludeProcessInfo.IsChecked = false;
            IncludeServiceInfo.IsChecked = false;
            IncludeStartupInfo.IsChecked = false;
            IncludeSecurityInfo.IsChecked = false;
            IncludeFirewallInfo.IsChecked = false;
            IncludeAntivirusInfo.IsChecked = false;
            IncludeUpdatesInfo.IsChecked = false;
            IncludeScreenshots.IsChecked = false;
            IncludeLogs.IsChecked = false;
            IncludeRegistryInfo.IsChecked = false;
            IncludeInstalledSoftware.IsChecked = false;
        }

        /// <summary>
        /// Обработчик кнопки "Выбрать всё"
        /// </summary>
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            SelectAllCheckboxes();
        }

        /// <summary>
        /// Обработчик кнопки "Очистить всё"
        /// </summary>
        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAllCheckboxes();
        }

        /// <summary>
        /// Обработчик кнопки "Предпросмотр"
        /// </summary>
        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Собираем текущие настройки для предпросмотра
                CollectSelectedCriteria();
                CollectAdvancedSettings();
                DetermineReportFormat();

                var previewWindow = new Window
                {
                    Title = "Предпросмотр отчёта",
                    Width = 600,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var scrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                };

                var stackPanel = new StackPanel { Margin = new Thickness(20) };

                // Заголовок
                var titleBlock = new TextBlock
                {
                    Text = "📋 Предпросмотр отчёта",
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 20),
                    Foreground = System.Windows.Media.Brushes.DarkBlue
                };
                stackPanel.Children.Add(titleBlock);

                // Основная информация
                var infoPanel = new StackPanel
                {
                    Background = System.Windows.Media.Brushes.LightBlue,
                    Margin = new Thickness(0, 0, 0, 20)
                };
                
                // Добавляем отступы через Border
                var infoBorder = new Border
                {
                    Child = infoPanel,
                    Padding = new Thickness(15)
                };

                infoPanel.Children.Add(CreatePreviewRow("Название:", ReportTitleTextBox.Text));
                infoPanel.Children.Add(CreatePreviewRow("Описание:", ReportDescriptionTextBox.Text));
                infoPanel.Children.Add(CreatePreviewRow("Категория:", (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Не выбрано"));
                infoPanel.Children.Add(CreatePreviewRow("Формат:", ReportFormat.ToUpper()));
                infoPanel.Children.Add(CreatePreviewRow("Критериев выбрано:", SelectedCriteria.Count.ToString()));

                stackPanel.Children.Add(infoBorder);

                // Выбранные критерии
                var criteriaHeader = new TextBlock
                {
                    Text = "✅ Выбранные критерии:",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = System.Windows.Media.Brushes.DarkGreen
                };
                stackPanel.Children.Add(criteriaHeader);

                if (SelectedCriteria.Count > 0)
                {
                    foreach (var criteria in SelectedCriteria)
                    {
                        var criteriaBlock = new TextBlock
                        {
                            Text = $"• {GetCriteriaDisplayName(criteria)}",
                            FontSize = 14,
                            Margin = new Thickness(20, 2, 0, 2),
                            Foreground = System.Windows.Media.Brushes.DarkSlateGray
                        };
                        stackPanel.Children.Add(criteriaBlock);
                    }
                }
                else
                {
                    var noCriteriaBlock = new TextBlock
                    {
                        Text = "❌ Критерии не выбраны",
                        FontSize = 14,
                        Margin = new Thickness(20, 2, 0, 2),
                        Foreground = System.Windows.Media.Brushes.Red,
                        FontStyle = FontStyles.Italic
                    };
                    stackPanel.Children.Add(noCriteriaBlock);
                }

                // Расширенные настройки
                if (AdvancedSettings.Count > 0)
                {
                    var settingsHeader = new TextBlock
                    {
                        Text = "⚙️ Расширенные настройки:",
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 20, 0, 10),
                        Foreground = System.Windows.Media.Brushes.DarkOrange
                    };
                    stackPanel.Children.Add(settingsHeader);

                    foreach (var setting in AdvancedSettings)
                    {
                        var settingBlock = new TextBlock
                        {
                            Text = $"• {setting.Key}: {setting.Value}",
                            FontSize = 12,
                            Margin = new Thickness(20, 2, 0, 2),
                            Foreground = System.Windows.Media.Brushes.DarkSlateGray
                        };
                        stackPanel.Children.Add(settingBlock);
                    }
                }

                scrollViewer.Content = stackPanel;
                previewWindow.Content = scrollViewer;

                previewWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании предпросмотра: {ex.Message}", "Ошибка", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Создание строки для предпросмотра
        /// </summary>
        private StackPanel CreatePreviewRow(string label, string value)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 0) };
            
            var labelBlock = new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.Bold,
                Width = 120,
                FontSize = 12
            };
            
            var valueBlock = new TextBlock
            {
                Text = value ?? "Не указано",
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.DarkSlateGray
            };
            
            panel.Children.Add(labelBlock);
            panel.Children.Add(valueBlock);
            
            return panel;
        }

        /// <summary>
        /// Получение отображаемого имени критерия
        /// </summary>
        private string GetCriteriaDisplayName(string criteria)
        {
            var displayNames = new Dictionary<string, string>
            {
                { "OS_INFO", "Информация об операционной системе" },
                { "COMPUTER_INFO", "Информация о компьютере" },
                { "USER_INFO", "Информация о пользователе" },
                { "DATETIME_INFO", "Дата и время создания" },
                { "ENVIRONMENT_INFO", "Переменные окружения" },
                { "CPU_INFO", "Информация о процессоре" },
                { "MEMORY_INFO", "Информация о памяти" },
                { "DISK_INFO", "Информация о дисках" },
                { "GPU_INFO", "Информация о видеокарте" },
                { "MOTHERBOARD_INFO", "Информация о материнской плате" },
                { "BIOS_INFO", "Информация о BIOS" },
                { "USB_INFO", "USB устройства" },
                { "PERFORMANCE_INFO", "Производительность системы" },
                { "NETWORK_INFO", "Сетевая информация" },
                { "PROCESS_INFO", "Информация о процессах" },
                { "SERVICE_INFO", "Системные службы" },
                { "STARTUP_INFO", "Программы автозагрузки" },
                { "SECURITY_INFO", "Настройки безопасности" },
                { "FIREWALL_INFO", "Настройки брандмауэра" },
                { "ANTIVIRUS_INFO", "Антивирусная защита" },
                { "UPDATES_INFO", "Обновления системы" },
                { "REGISTRY_INFO", "Ключевые записи реестра" },
                { "INSTALLED_SOFTWARE", "Установленное ПО" },
                { "SCREENSHOTS", "Скриншоты" },
                { "LOGS", "Системные логи" },
                { "SYSTEM_SERVICES", "Информация о службах Windows" },
                { "WINDOWS_FEATURES", "Компоненты Windows" },
                { "DEVICE_DRIVERS", "Драйверы устройств" },
                { "POWER_SETTINGS", "Настройки питания" },
                { "DISPLAY_SETTINGS", "Настройки дисплея" },
                { "AUDIO_DEVICES", "Аудиоустройства" },
                { "PRINTERS", "Принтеры и печать" },
                { "WINDOWS_UPDATES", "Обновления Windows" },
                { "TASK_SCHEDULER", "Планировщик задач" }
            };

            return displayNames.ContainsKey(criteria) ? displayNames[criteria] : criteria;
        }


    }
}
