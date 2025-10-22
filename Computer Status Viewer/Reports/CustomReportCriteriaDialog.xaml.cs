using System;
using System.Collections.Generic;
using System.Windows;

namespace Computer_Status_Viewer.Reports
{
    /// <summary>
    /// Диалог для выбора критериев пользовательского отчёта
    /// </summary>
    public partial class CustomReportCriteriaDialog : Window
    {
        public string ReportTitle { get; private set; }
        public string ReportDescription { get; private set; }
        public List<string> SelectedCriteria { get; private set; }

        public CustomReportCriteriaDialog()
        {
            InitializeComponent();
            SelectedCriteria = new List<string>();
        }

        /// <summary>
        /// Обработчик кнопки "Создать"
        /// </summary>
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем название и описание отчёта
                ReportTitle = ReportTitleTextBox.Text.Trim();
                ReportDescription = ReportDescriptionTextBox.Text.Trim();

                if (string.IsNullOrEmpty(ReportTitle))
                {
                    MessageBox.Show("Пожалуйста, введите название отчёта.", "Ошибка", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Собираем выбранные критерии
                SelectedCriteria.Clear();

                // Системная информация
                if (IncludeOSInfo.IsChecked == true)
                    SelectedCriteria.Add("OS_INFO");
                if (IncludeComputerInfo.IsChecked == true)
                    SelectedCriteria.Add("COMPUTER_INFO");
                if (IncludeUserInfo.IsChecked == true)
                    SelectedCriteria.Add("USER_INFO");
                if (IncludeDateTimeInfo.IsChecked == true)
                    SelectedCriteria.Add("DATETIME_INFO");

                // Аппаратная информация
                if (IncludeCPUInfo.IsChecked == true)
                    SelectedCriteria.Add("CPU_INFO");
                if (IncludeMemoryInfo.IsChecked == true)
                    SelectedCriteria.Add("MEMORY_INFO");
                if (IncludeDiskInfo.IsChecked == true)
                    SelectedCriteria.Add("DISK_INFO");
                if (IncludeGPUInfo.IsChecked == true)
                    SelectedCriteria.Add("GPU_INFO");
                if (IncludeMotherboardInfo.IsChecked == true)
                    SelectedCriteria.Add("MOTHERBOARD_INFO");

                // Производительность
                if (IncludePerformanceInfo.IsChecked == true)
                    SelectedCriteria.Add("PERFORMANCE_INFO");
                if (IncludeNetworkInfo.IsChecked == true)
                    SelectedCriteria.Add("NETWORK_INFO");
                if (IncludeProcessInfo.IsChecked == true)
                    SelectedCriteria.Add("PROCESS_INFO");

                // Дополнительные опции
                if (IncludeScreenshots.IsChecked == true)
                    SelectedCriteria.Add("SCREENSHOTS");
                if (IncludeLogs.IsChecked == true)
                    SelectedCriteria.Add("LOGS");

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
        /// Обработчик кнопки "Отмена"
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
