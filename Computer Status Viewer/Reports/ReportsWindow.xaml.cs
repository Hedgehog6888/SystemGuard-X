using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Computer_Status_Viewer.Models;

namespace Computer_Status_Viewer.Reports
{
    /// <summary>
    /// Окно для просмотра и управления отчётами
    /// </summary>
    public partial class ReportsWindow : Window
    {
        private ReportManager _reportManager;
        private Report _selectedReport;

        public ReportsWindow()
        {
            InitializeComponent();
            _reportManager = new ReportManager();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadReports();
        }

        /// <summary>
        /// Загрузка списка отчётов
        /// </summary>
        private void LoadReports()
        {
            try
            {
                var reports = _reportManager.GetAllReports();
                ReportsListBox.ItemsSource = reports;
                ReportsCountText.Text = $"Отчётов: {reports.Count}";
                StatusText.Text = "Отчёты загружены";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка загрузки отчётов: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки отчётов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработчик выбора отчёта
        /// </summary>
        private void ReportsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReportsListBox.SelectedItem is Report selectedReport)
            {
                _selectedReport = selectedReport;
                ShowReportDetails(selectedReport);
                DeleteReportButton.Visibility = Visibility.Visible;
            }
            else
            {
                _selectedReport = null;
                ShowEmptyDetails();
                DeleteReportButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Показать детали отчёта
        /// </summary>
        private void ShowReportDetails(Report report)
        {
            try
            {
                ReportDetailsPanel.Children.Clear();

                // Основная информация об отчёте
                var mainInfoPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
                
                var titleBlock = new TextBlock
                {
                    Text = report.Title,
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                mainInfoPanel.Children.Add(titleBlock);

                var descriptionBlock = new TextBlock
                {
                    Text = report.Description ?? "Без описания",
                    FontSize = 14,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                mainInfoPanel.Children.Add(descriptionBlock);

                var infoGrid = new Grid();
                infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var leftPanel = new StackPanel();
                var rightPanel = new StackPanel();

                // Левая колонка
                leftPanel.Children.Add(CreateInfoRow("Тип отчёта:", report.ReportType?.Name ?? "Неизвестно"));
                leftPanel.Children.Add(CreateInfoRow("Категория:", report.ReportType?.Category ?? "Неизвестно"));
                leftPanel.Children.Add(CreateInfoRow("Статус:", report.Status));
                leftPanel.Children.Add(CreateInfoRow("Тип:", report.IsAutomatic ? "Автоматический" : "Пользовательский"));

                // Правая колонка
                rightPanel.Children.Add(CreateInfoRow("Создан:", report.CreatedDate.ToString("dd.MM.yyyy HH:mm:ss")));
                if (report.ModifiedDate.HasValue)
                {
                    rightPanel.Children.Add(CreateInfoRow("Изменён:", report.ModifiedDate.Value.ToString("dd.MM.yyyy HH:mm:ss")));
                }
                if (!string.IsNullOrEmpty(report.FilePath))
                {
                    rightPanel.Children.Add(CreateInfoRow("Файл:", Path.GetFileName(report.FilePath)));
                }

                Grid.SetColumn(leftPanel, 0);
                Grid.SetColumn(rightPanel, 1);
                infoGrid.Children.Add(leftPanel);
                infoGrid.Children.Add(rightPanel);

                mainInfoPanel.Children.Add(infoGrid);
                ReportDetailsPanel.Children.Add(mainInfoPanel);

                // Загрузка данных отчёта
                LoadReportData(report.Id);
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка загрузки деталей отчёта: {ex.Message}";
            }
        }

        /// <summary>
        /// Загрузка данных отчёта
        /// </summary>
        private void LoadReportData(int reportId)
        {
            try
            {
                var reportData = _reportManager.GetReportData(reportId);
                
                if (reportData.Any())
                {
                    var dataHeader = new TextBlock
                    {
                        Text = "Данные отчёта:",
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 20, 0, 10)
                    };
                    ReportDetailsPanel.Children.Add(dataHeader);

                    // Группировка данных по категориям
                    var groupedData = reportData.GroupBy(d => d.Category ?? "Без категории");
                    
                    foreach (var group in groupedData)
                    {
                        var categoryPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
                        
                        var categoryHeader = new TextBlock
                        {
                            Text = $"📁 {group.Key}",
                            FontSize = 14,
                            FontWeight = FontWeights.Medium,
                            Foreground = System.Windows.Media.Brushes.DarkBlue,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        categoryPanel.Children.Add(categoryHeader);

                        var dataGrid = new DataGrid
                        {
                            ItemsSource = group.Select(d => new
                            {
                                Параметр = d.Key,
                                Значение = d.Value,
                                Тип = d.DataType,
                                Время = d.Timestamp.ToString("HH:mm:ss")
                            }).ToList(),
                            AutoGenerateColumns = true,
                            CanUserAddRows = false,
                            CanUserDeleteRows = false,
                            IsReadOnly = true,
                            HeadersVisibility = DataGridHeadersVisibility.Column,
                            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
                            AlternatingRowBackground = System.Windows.Media.Brushes.LightGray,
                            RowBackground = System.Windows.Media.Brushes.White,
                            Margin = new Thickness(10, 0, 0, 0)
                        };
                        
                        categoryPanel.Children.Add(dataGrid);
                        ReportDetailsPanel.Children.Add(categoryPanel);
                    }
                }
                else
                {
                    var noDataText = new TextBlock
                    {
                        Text = "Данные отчёта отсутствуют",
                        FontSize = 14,
                        Foreground = System.Windows.Media.Brushes.Gray,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 20, 0, 0)
                    };
                    ReportDetailsPanel.Children.Add(noDataText);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка загрузки данных отчёта: {ex.Message}";
            }
        }

        /// <summary>
        /// Создание строки информации
        /// </summary>
        private StackPanel CreateInfoRow(string label, string value)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 0) };
            
            var labelBlock = new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.Medium,
                Width = 120,
                FontSize = 12
            };
            
            var valueBlock = new TextBlock
            {
                Text = value,
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.DarkSlateGray
            };
            
            panel.Children.Add(labelBlock);
            panel.Children.Add(valueBlock);
            
            return panel;
        }

        /// <summary>
        /// Показать пустые детали
        /// </summary>
        private void ShowEmptyDetails()
        {
            ReportDetailsPanel.Children.Clear();
            
            var emptyText = new TextBlock
            {
                Text = "Выберите отчёт для просмотра деталей",
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 50, 0, 0)
            };
            
            ReportDetailsPanel.Children.Add(emptyText);
        }

        /// <summary>
        /// Создание быстрого отчёта
        /// </summary>
        private void CreateQuickReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Создание быстрого отчёта...";
                int reportId = _reportManager.CreateQuickReport();
                StatusText.Text = $"Быстрый отчёт создан! ID: {reportId}";
                LoadReports();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка создания быстрого отчёта: {ex.Message}";
                MessageBox.Show($"Ошибка создания быстрого отчёта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Создание подробного отчёта
        /// </summary>
        private void CreateDetailedReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Создание подробного отчёта...";
                int reportId = _reportManager.CreateDetailedReport();
                StatusText.Text = $"Подробный отчёт создан! ID: {reportId}";
                LoadReports();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка создания подробного отчёта: {ex.Message}";
                MessageBox.Show($"Ошибка создания подробного отчёта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Создание пользовательского отчёта
        /// </summary>
        private void CreateCustomReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Открытие диалога выбора критериев...";
                
                // Открываем диалог выбора критериев для пользовательского отчёта
                var criteriaDialog = new CustomReportCriteriaDialog();
                var result = criteriaDialog.ShowDialog();
                
                if (result == true)
                {
                    StatusText.Text = "Создание пользовательского отчёта...";
                    
                    var selectedCriteria = criteriaDialog.SelectedCriteria;
                    var reportTitle = criteriaDialog.ReportTitle;
                    var reportDescription = criteriaDialog.ReportDescription;
                    
                    int reportId = _reportManager.CreateCustomReportWithCriteria(
                        reportTitle, 
                        reportDescription, 
                        selectedCriteria
                    );
                    
                    StatusText.Text = $"Пользовательский отчёт создан! ID: {reportId}";
                    LoadReports();
                }
                else
                {
                    StatusText.Text = "Создание отчёта отменено";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка создания пользовательского отчёта: {ex.Message}";
                MessageBox.Show($"Ошибка создания пользовательского отчёта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обновление списка отчётов
        /// </summary>

        /// <summary>
        /// Удаление отчёта
        /// </summary>
        private void DeleteReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedReport == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить отчёт '{_selectedReport.Title}'?\n\nЭто действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _reportManager.DeleteReport(_selectedReport.Id);
                    StatusText.Text = $"Отчёт '{_selectedReport.Title}' удалён";
                    LoadReports();
                    ShowEmptyDetails();
                    DeleteReportButton.Visibility = Visibility.Collapsed;
                    // Отчёт успешно удалён
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Ошибка удаления отчёта: {ex.Message}";
                    MessageBox.Show($"Ошибка удаления отчёта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
