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

            // Настройки детализации
            AdvancedSettings["DetailLevel"] = (DetailLevelComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Базовая";
            AdvancedSettings["IncludeRawData"] = IncludeRawData.IsChecked == true;
            AdvancedSettings["IncludeStatistics"] = IncludeStatistics.IsChecked == true;
            AdvancedSettings["IncludeRecommendations"] = IncludeRecommendations.IsChecked == true;

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

                string preview = $"Предпросмотр отчёта:\n\n";
                preview += $"Название: {ReportTitleTextBox.Text}\n";
                preview += $"Описание: {ReportDescriptionTextBox.Text}\n";
                preview += $"Категория: {(CategoryComboBox.SelectedItem as ComboBoxItem)?.Content}\n";
                preview += $"Формат: {ReportFormat.ToUpper()}\n";
                preview += $"Выбрано критериев: {SelectedCriteria.Count}\n\n";
                preview += "Выбранные критерии:\n";
                foreach (var criteria in SelectedCriteria)
                {
                    preview += $"• {criteria}\n";
                }

                MessageBox.Show(preview, "Предпросмотр отчёта", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании предпросмотра: {ex.Message}", "Ошибка", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
