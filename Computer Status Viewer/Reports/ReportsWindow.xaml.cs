using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        private AutoReportService _autoReportService;

        public ReportsWindow()
        {
            InitializeComponent();
            _reportManager = new ReportManager();
            _autoReportService = new AutoReportService();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadReports();
            UpdateStatistics();
            LoadSettings();
            SetupSettingsEventHandlers();
        }

        /// <summary>
        /// Обработчик нажатия клавиш в окне
        /// </summary>
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F5)
            {
                RefreshReportsButton_Click(sender, e);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Настройка обработчиков событий для настроек
        /// </summary>
        private void SetupSettingsEventHandlers()
        {
            // Обработчики для автоматических отчётов
            if (AutoCreateSystemReports != null)
                AutoCreateSystemReports.Checked += (s, e) => SaveSettings();
            if (AutoCreateSystemReports != null)
                AutoCreateSystemReports.Unchecked += (s, e) => SaveSettings();
            
            if (AutoCreatePerformanceReports != null)
                AutoCreatePerformanceReports.Checked += (s, e) => SaveSettings();
            if (AutoCreatePerformanceReports != null)
                AutoCreatePerformanceReports.Unchecked += (s, e) => SaveSettings();
            
            if (AutoCreateSecurityReports != null)
                AutoCreateSecurityReports.Checked += (s, e) => SaveSettings();
            if (AutoCreateSecurityReports != null)
                AutoCreateSecurityReports.Unchecked += (s, e) => SaveSettings();
            
            // Обработчик для интервала
            if (ReportIntervalComboBox != null)
                ReportIntervalComboBox.SelectionChanged += (s, e) => 
                {
                    SaveSettings();
                    _autoReportService?.UpdateInterval();
                };
            
            // Обработчики для экспорта
            if (AutoExportReports != null)
                AutoExportReports.Checked += (s, e) => SaveSettings();
            if (AutoExportReports != null)
                AutoExportReports.Unchecked += (s, e) => SaveSettings();
            
            if (IncludeChartsInExport != null)
                IncludeChartsInExport.Checked += (s, e) => SaveSettings();
            if (IncludeChartsInExport != null)
                IncludeChartsInExport.Unchecked += (s, e) => SaveSettings();
            
            if (CompressExports != null)
                CompressExports.Checked += (s, e) => SaveSettings();
            if (CompressExports != null)
                CompressExports.Unchecked += (s, e) => SaveSettings();
            
            if (DefaultExportFormatComboBox != null)
                DefaultExportFormatComboBox.SelectionChanged += (s, e) => SaveSettings();
            
            // Обработчики для уведомлений
            if (NotifyOnReportCompletion != null)
                NotifyOnReportCompletion.Checked += (s, e) => SaveSettings();
            if (NotifyOnReportCompletion != null)
                NotifyOnReportCompletion.Unchecked += (s, e) => SaveSettings();
            
            if (NotifyOnReportErrors != null)
                NotifyOnReportErrors.Checked += (s, e) => SaveSettings();
            if (NotifyOnReportErrors != null)
                NotifyOnReportErrors.Unchecked += (s, e) => SaveSettings();
            
            if (ShowReportPreview != null)
                ShowReportPreview.Checked += (s, e) => SaveSettings();
            if (ShowReportPreview != null)
                ShowReportPreview.Unchecked += (s, e) => SaveSettings();
        }

        /// <summary>
        /// Загрузка списка отчётов
        /// </summary>
        private void LoadReports()
        {
            try
            {
                if (_reportManager == null)
                {
                    if (StatusText != null) StatusText.Text = "Ошибка: ReportManager не инициализирован";
                    return;
                }

                var reports = _reportManager.GetAllReports();
                if (reports == null)
                {
                    if (StatusText != null) StatusText.Text = "Ошибка: Не удалось загрузить отчёты";
                    return;
                }

                if (ReportsListBox != null)
                {
                    ReportsListBox.ItemsSource = reports;
                    // Не очищаем выбранный элемент при загрузке отчётов
                    if (_selectedReport != null)
                    {
                        // Восстанавливаем выбранный элемент если он существует
                        var existingReport = reports.FirstOrDefault(r => r.Id == _selectedReport.Id);
                        if (existingReport != null)
                        {
                            ReportsListBox.SelectedItem = existingReport;
                        }
                    }
                }
                
                if (ReportsCountText != null)
                {
                    ReportsCountText.Text = $"Отчётов: {reports.Count}";
                }
                
                if (StatusText != null)
                {
                    if (StatusText != null) StatusText.Text = "Отчёты загружены";
                }
            }
            catch (Exception ex)
            {
                if (StatusText != null)
                {
                    if (StatusText != null) StatusText.Text = $"Ошибка загрузки отчётов: {ex.Message}";
                }
                MessageBox.Show($"Ошибка загрузки отчётов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработчик выбора отчёта
        /// </summary>
        private void ReportsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = ReportsListBox?.SelectedItems;
            if (selectedItems != null && selectedItems.Count > 0)
            {
                // Если выделен только один элемент, показываем его детали
                if (selectedItems.Count == 1 && selectedItems[0] is Report singleReport)
                {
                    _selectedReport = singleReport;
                    ShowReportDetails(singleReport);
                }
                else
                {
                    // Если выделено несколько элементов, показываем общую информацию
                    ShowMultipleSelectionDetails(selectedItems.Cast<Report>().ToList());
                }
                
                // Показываем кнопку удаления для любого количества выделенных элементов
                if (DeleteReportButton != null)
                {
                    DeleteReportButton.Visibility = Visibility.Visible;
                    DeleteReportButton.Content = selectedItems.Count == 1 ? 
                        "🗑️ Удалить" : 
                        $"🗑️ Удалить ({selectedItems.Count})";
                }
                
                // Кнопка экспорта только для одного элемента
                if (ExportButton != null)
                    ExportButton.Visibility = selectedItems.Count == 1 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                // Если ничего не выделено, скрываем кнопки
                if (DeleteReportButton != null)
                    DeleteReportButton.Visibility = Visibility.Collapsed;
                if (ExportButton != null)
                    ExportButton.Visibility = Visibility.Collapsed;
                ShowEmptyDetails();
                _selectedReport = null;
            }
        }

        /// <summary>
        /// Показать детали отчёта
        /// </summary>
        private void ShowReportDetails(Report report)
        {
            try
            {
                if (ReportDetailsPanel != null) ReportDetailsPanel.Children.Clear();

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
                if (ReportDetailsPanel != null) ReportDetailsPanel.Children.Add(mainInfoPanel);

                // Загрузка данных отчёта
                LoadReportData(report.Id);
            }
            catch (Exception ex)
            {
                if (StatusText != null) StatusText.Text = $"Ошибка загрузки деталей отчёта: {ex.Message}";
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
                    if (ReportDetailsPanel != null) ReportDetailsPanel.Children.Add(dataHeader);

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
                        if (ReportDetailsPanel != null) ReportDetailsPanel.Children.Add(categoryPanel);
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
                    if (ReportDetailsPanel != null) ReportDetailsPanel.Children.Add(noDataText);
                }
            }
            catch (Exception ex)
            {
                if (StatusText != null) StatusText.Text = $"Ошибка загрузки данных отчёта: {ex.Message}";
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
            if (ReportDetailsPanel != null) ReportDetailsPanel.Children.Clear();
            
            var emptyText = new TextBlock
            {
                Text = "Выберите отчёт для просмотра деталей",
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 50, 0, 0)
            };
            
            if (ReportDetailsPanel != null) ReportDetailsPanel.Children.Add(emptyText);
        }

        /// <summary>
        /// Показать детали множественного выделения
        /// </summary>
        private void ShowMultipleSelectionDetails(List<Report> selectedReports)
        {
            try
            {
                if (ReportDetailsPanel != null) ReportDetailsPanel.Children.Clear();

                var mainInfoPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
                
                var titleBlock = new TextBlock
                {
                    Text = $"Выделено отчётов: {selectedReports.Count}",
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                mainInfoPanel.Children.Add(titleBlock);

                // Статистика по выделенным отчётам
                var automaticCount = selectedReports.Count(r => r.IsAutomatic);
                var customCount = selectedReports.Count(r => !r.IsAutomatic);
                var completedCount = selectedReports.Count(r => r.Status == "Завершён");

                var statsText = new TextBlock
                {
                    Text = $"Автоматических: {automaticCount}\nПользовательских: {customCount}\nЗавершённых: {completedCount}",
                    FontSize = 14,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    Margin = new Thickness(0, 0, 0, 15)
                };
                mainInfoPanel.Children.Add(statsText);

                // Список выделенных отчётов
                var listHeader = new TextBlock
                {
                    Text = "Выделенные отчёты:",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                };
                mainInfoPanel.Children.Add(listHeader);

                var reportsList = new StackPanel();
                foreach (var report in selectedReports.Take(10)) // Показываем максимум 10 отчётов
                {
                    var reportItem = new TextBlock
                    {
                        Text = $"• {report.Title} ({report.Status})",
                        FontSize = 12,
                        Margin = new Thickness(10, 2, 0, 2),
                        Foreground = System.Windows.Media.Brushes.DarkSlateGray
                    };
                    reportsList.Children.Add(reportItem);
                }

                if (selectedReports.Count > 10)
                {
                    var moreText = new TextBlock
                    {
                        Text = $"... и ещё {selectedReports.Count - 10} отчётов",
                        FontSize = 12,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(10, 2, 0, 2),
                        Foreground = System.Windows.Media.Brushes.Gray
                    };
                    reportsList.Children.Add(moreText);
                }

                mainInfoPanel.Children.Add(reportsList);
                if (ReportDetailsPanel != null) ReportDetailsPanel.Children.Add(mainInfoPanel);
            }
            catch (Exception ex)
            {
                if (StatusText != null) StatusText.Text = $"Ошибка отображения множественного выделения: {ex.Message}";
            }
        }

        /// <summary>
        /// Обработчик нажатия клавиш в списке отчётов
        /// </summary>
        private void ReportsListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                DeleteSelectedReports();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.F5)
            {
                RefreshReportsButton_Click(sender, e);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Создание быстрого отчёта
        /// </summary>
        private void CreateQuickReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (StatusText != null) StatusText.Text = "Создание быстрого отчёта...";
                int reportId = _reportManager.CreateQuickReport();
                if (StatusText != null) StatusText.Text = $"Быстрый отчёт создан! ID: {reportId}";
                LoadReports();
            }
            catch (Exception ex)
            {
                if (StatusText != null) StatusText.Text = $"Ошибка создания быстрого отчёта: {ex.Message}";
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
                if (StatusText != null) StatusText.Text = "Создание подробного отчёта...";
                int reportId = _reportManager.CreateDetailedReport();
                if (StatusText != null) StatusText.Text = $"Подробный отчёт создан! ID: {reportId}";
                LoadReports();
            }
            catch (Exception ex)
            {
                if (StatusText != null) StatusText.Text = $"Ошибка создания подробного отчёта: {ex.Message}";
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
                if (StatusText != null) StatusText.Text = "Открытие диалога выбора критериев...";
                
                // Открываем диалог выбора критериев для пользовательского отчёта
                var criteriaDialog = new CustomReportCriteriaDialog();
                var result = criteriaDialog.ShowDialog();
                
                if (result == true)
                {
                    if (StatusText != null) StatusText.Text = "Создание пользовательского отчёта...";
                    
                    var selectedCriteria = criteriaDialog.SelectedCriteria;
                    var reportTitle = criteriaDialog.ReportTitle;
                    var reportDescription = criteriaDialog.ReportDescription;
                    
                    int reportId = _reportManager.CreateCustomReportWithCriteria(
                        reportTitle, 
                        reportDescription, 
                        selectedCriteria,
                        criteriaDialog.AdvancedSettings
                    );
                    
                    if (StatusText != null) StatusText.Text = $"Пользовательский отчёт создан! ID: {reportId}";
                    LoadReports();
                }
                else
                {
                    if (StatusText != null) StatusText.Text = "Создание отчёта отменено";
                }
            }
            catch (Exception ex)
            {
                if (StatusText != null) StatusText.Text = $"Ошибка создания пользовательского отчёта: {ex.Message}";
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
            DeleteSelectedReports();
        }

        /// <summary>
        /// Обработчик кнопки "Обновить"
        /// </summary>
        private void RefreshReportsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Обновляем список отчётов
                LoadReports();
                
                // Обновляем статистику
                UpdateStatistics();
                
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Список отчётов обновлён пользователем");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления списка отчётов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления списка отчётов: {ex.Message}");
            }
        }


        /// <summary>
        /// Удаление выделенных отчётов
        /// </summary>
        private void DeleteSelectedReports()
        {
            var selectedItems = ReportsListBox?.SelectedItems;
            if (selectedItems == null || selectedItems.Count == 0) return;

            var selectedReports = selectedItems.Cast<Report>().ToList();
            var reportTitles = string.Join(", ", selectedReports.Take(3).Select(r => r.Title));
            if (selectedReports.Count > 3)
            {
                reportTitles += $" и ещё {selectedReports.Count - 3} отчётов";
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить {selectedReports.Count} отчёт(ов)?\n\n{reportTitles}\n\nЭто действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var deletedCount = 0;
                    var errors = new List<string>();

                    foreach (var report in selectedReports)
                    {
                        try
                        {
                            _reportManager.DeleteReport(report.Id);
                            deletedCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Ошибка удаления '{report.Title}': {ex.Message}");
                        }
                    }

                    if (deletedCount > 0)
                    {
                        if (StatusText != null) StatusText.Text = $"Удалено отчётов: {deletedCount}";
                        LoadReports();
                        UpdateStatistics();
                        ShowEmptyDetails();
                        if (DeleteReportButton != null)
                            DeleteReportButton.Visibility = Visibility.Collapsed;
                        if (ExportButton != null)
                            ExportButton.Visibility = Visibility.Collapsed;
                        _selectedReport = null;
                    }

                    if (errors.Any())
                    {
                        var errorMessage = string.Join("\n", errors);
                        MessageBox.Show($"Некоторые отчёты не удалось удалить:\n{errorMessage}", 
                                      "Ошибки при удалении", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    if (StatusText != null) StatusText.Text = $"Ошибка удаления отчётов: {ex.Message}";
                    MessageBox.Show($"Ошибка удаления отчётов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        /// <summary>
        /// Обработчик изменения фильтров
        /// </summary>
        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterReports();
        }

        /// <summary>
        /// Фильтрация отчётов
        /// </summary>
        private void FilterReports()
        {
            try
            {
                if (_reportManager == null)
                {
                    if (StatusText != null) StatusText.Text = "Ошибка: ReportManager не инициализирован";
                    return;
                }

                var allReports = _reportManager.GetAllReports();
                if (allReports == null)
                {
                    if (StatusText != null) StatusText.Text = "Ошибка: Не удалось загрузить отчёты";
                    return;
                }

                var filteredReports = allReports.AsEnumerable();


                // Фильтр по типу
                var typeFilter = (TypeFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (!string.IsNullOrEmpty(typeFilter))
                {
                    if (typeFilter == "Автоматические")
                    {
                        filteredReports = filteredReports.Where(r => r.IsAutomatic);
                    }
                    else if (typeFilter == "Пользовательские")
                    {
                        filteredReports = filteredReports.Where(r => !r.IsAutomatic);
                    }
                }

                // Фильтр по категории
                var categoryFilter = (CategoryFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "Все категории")
                {
                    filteredReports = filteredReports.Where(r => r.ReportType?.Name == categoryFilter);
                }

                if (ReportsListBox != null)
                {
                    ReportsListBox.ItemsSource = filteredReports.ToList();
                }
                
                if (ReportsCountText != null)
                {
                    ReportsCountText.Text = $"Отчётов: {filteredReports.Count()}";
                }
            }
            catch (Exception ex)
            {
                if (StatusText != null) StatusText.Text = $"Ошибка фильтрации: {ex.Message}";
            }
        }



        /// <summary>
        /// Обработчик прокрутки колесиком мыши
        /// </summary>
        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                if (e.Delta > 0)
                {
                    scrollViewer.LineUp();
                }
                else
                {
                    scrollViewer.LineDown();
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработчик кнопки экспорта
        /// </summary>
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedReport == null) return;

            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Экспорт отчёта",
                    Filter = "PDF файлы (*.pdf)|*.pdf|Текстовые файлы (*.txt)|*.txt|HTML файлы (*.html)|*.html|JSON файлы (*.json)|*.json|CSV файлы (*.csv)|*.csv",
                    DefaultExt = "pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var filePath = saveDialog.FileName;
                    var fileExtension = Path.GetExtension(filePath).ToLower();
                    
                    // Создаём экспортёр
                    var exporter = new ReportExporter(_reportManager);
                    
                    // Определяем формат по расширению файла
                    string format;
                    switch (fileExtension)
                    {
                        case ".pdf":
                            format = "pdf";
                            break;
                        case ".txt":
                            format = "txt";
                            break;
                        default:
                            // Для других форматов показываем сообщение о неподдерживаемом формате
                            MessageBox.Show($"Формат {fileExtension} пока не поддерживается. Используйте PDF или TXT.", 
                                          "Неподдерживаемый формат", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                    }

                    // Экспортируем отчёт
                    exporter.ExportReport(_selectedReport, filePath, format);
                    
                    if (StatusText != null) StatusText.Text = $"Отчёт экспортирован в {Path.GetFileName(filePath)}";
                    MessageBox.Show($"Отчёт успешно экспортирован в {filePath}", "Экспорт", 
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                if (StatusText != null) StatusText.Text = $"Ошибка экспорта: {ex.Message}";
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обновление статистики
        /// </summary>
        private void UpdateStatistics()
        {
            try
            {
                if (_reportManager == null)
                {
                    return;
                }

                var allReports = _reportManager.GetAllReports();
                if (allReports == null)
                {
                    return;
                }
                
                if (TotalReportsText != null)
                {
                    TotalReportsText.Text = $"Всего отчётов: {allReports.Count}";
                }
                
                if (CompletedReportsText != null)
                {
                    CompletedReportsText.Text = $"Завершённых: {allReports.Count(r => r.Status == "Завершён")}";
                }
                
                if (CustomReportsText != null)
                {
                    CustomReportsText.Text = $"Пользовательских: {allReports.Count(r => !r.IsAutomatic)}";
                }
                
                if (TodayReportsText != null)
                {
                    TodayReportsText.Text = $"Сегодня: {allReports.Count(r => r.CreatedDate.Date == DateTime.Today)}";
                }

                // Обновляем текст графика
                if (ReportsChartText != null)
                {
                    var reportTypes = allReports.GroupBy(r => r.ReportType?.Name ?? "Неизвестно")
                        .Select(g => $"{g.Key}: {g.Count()}")
                        .ToArray();
                    ReportsChartText.Text = string.Join("\n", reportTypes);
                }
            }
            catch (Exception ex)
            {
                if (StatusText != null)
                {
                    if (StatusText != null) StatusText.Text = $"Ошибка обновления статистики: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Загрузка настроек
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                var settings = Properties.Settings.Default;
                
                // Загрузка настроек автоматических отчётов
                if (AutoCreateSystemReports != null)
                    AutoCreateSystemReports.IsChecked = settings.AutoCreateSystemReports;
                if (AutoCreatePerformanceReports != null)
                    AutoCreatePerformanceReports.IsChecked = settings.AutoCreatePerformanceReports;
                if (AutoCreateSecurityReports != null)
                    AutoCreateSecurityReports.IsChecked = settings.AutoCreateSecurityReports;
                
                // Загрузка интервала создания отчётов
                if (ReportIntervalComboBox != null)
                {
                    var interval = settings.ReportInterval;
                    for (int i = 0; i < ReportIntervalComboBox.Items.Count; i++)
                    {
                        var item = ReportIntervalComboBox.Items[i] as ComboBoxItem;
                        if (item?.Content?.ToString() == interval)
                        {
                            ReportIntervalComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                    if (ReportIntervalComboBox.SelectedIndex == -1)
                        ReportIntervalComboBox.SelectedIndex = 3; // По умолчанию "Ежедневно"
                }

                // Загрузка настроек экспорта
                if (AutoExportReports != null)
                    AutoExportReports.IsChecked = settings.AutoExportReports;
                if (IncludeChartsInExport != null)
                    IncludeChartsInExport.IsChecked = settings.IncludeChartsInExport;
                if (CompressExports != null)
                    CompressExports.IsChecked = settings.CompressExports;
                
                if (DefaultExportFormatComboBox != null)
                {
                    var format = settings.DefaultExportFormat;
                    for (int i = 0; i < DefaultExportFormatComboBox.Items.Count; i++)
                    {
                        var item = DefaultExportFormatComboBox.Items[i] as ComboBoxItem;
                        if (item?.Content?.ToString() == format)
                        {
                            DefaultExportFormatComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                    if (DefaultExportFormatComboBox.SelectedIndex == -1)
                        DefaultExportFormatComboBox.SelectedIndex = 0; // По умолчанию "TXT"
                }

                // Загрузка настроек уведомлений
                if (NotifyOnReportCompletion != null)
                    NotifyOnReportCompletion.IsChecked = settings.NotifyOnReportCompletion;
                if (NotifyOnReportErrors != null)
                    NotifyOnReportErrors.IsChecked = settings.NotifyOnReportErrors;
                if (ShowReportPreview != null)
                    ShowReportPreview.IsChecked = settings.ShowReportPreview;
            }
            catch (Exception ex)
            {
                if (StatusText != null)
                {
                    if (StatusText != null) StatusText.Text = $"Ошибка загрузки настроек: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Сохранение настроек
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                var settings = Properties.Settings.Default;
                
                // Сохранение настроек автоматических отчётов
                if (AutoCreateSystemReports != null)
                    settings.AutoCreateSystemReports = AutoCreateSystemReports.IsChecked ?? false;
                if (AutoCreatePerformanceReports != null)
                    settings.AutoCreatePerformanceReports = AutoCreatePerformanceReports.IsChecked ?? false;
                if (AutoCreateSecurityReports != null)
                    settings.AutoCreateSecurityReports = AutoCreateSecurityReports.IsChecked ?? false;
                
                // Сохранение интервала создания отчётов
                if (ReportIntervalComboBox != null && ReportIntervalComboBox.SelectedItem is ComboBoxItem selectedItem)
                    settings.ReportInterval = selectedItem.Content?.ToString() ?? "Ежедневно";

                // Сохранение настроек экспорта
                if (AutoExportReports != null)
                    settings.AutoExportReports = AutoExportReports.IsChecked ?? false;
                if (IncludeChartsInExport != null)
                    settings.IncludeChartsInExport = IncludeChartsInExport.IsChecked ?? true;
                if (CompressExports != null)
                    settings.CompressExports = CompressExports.IsChecked ?? false;
                
                if (DefaultExportFormatComboBox != null && DefaultExportFormatComboBox.SelectedItem is ComboBoxItem selectedFormat)
                    settings.DefaultExportFormat = selectedFormat.Content?.ToString() ?? "TXT";

                // Сохранение настроек уведомлений
                if (NotifyOnReportCompletion != null)
                    settings.NotifyOnReportCompletion = NotifyOnReportCompletion.IsChecked ?? true;
                if (NotifyOnReportErrors != null)
                    settings.NotifyOnReportErrors = NotifyOnReportErrors.IsChecked ?? true;
                if (ShowReportPreview != null)
                    settings.ShowReportPreview = ShowReportPreview.IsChecked ?? true;
                
                settings.Save();
            }
            catch (Exception ex)
            {
                if (StatusText != null)
                {
                    if (StatusText != null) StatusText.Text = $"Ошибка сохранения настроек: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Обработчик закрытия окна
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            _autoReportService?.Stop();
            _autoReportService?.Dispose();
            base.OnClosed(e);
        }

    }
}
