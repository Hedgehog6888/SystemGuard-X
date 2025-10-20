using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Computer_Status_Viewer
{
    public class SubcategoryManager
    {
        private readonly ContentControl mainContentArea;
        private readonly TabControl performanceTabControl;
        private readonly PerformanceManager performanceManager;
        private readonly SummaryManager summaryManager;
        private readonly TreeView categoryTreeView;

        public SubcategoryManager(ContentControl mainContentArea, TabControl performanceTabControl,
            PerformanceManager performanceManager, SummaryManager summaryManager, TreeView categoryTreeView)
        {
            this.mainContentArea = mainContentArea;
            this.performanceTabControl = performanceTabControl;
            this.performanceManager = performanceManager;
            this.summaryManager = summaryManager;
            this.categoryTreeView = categoryTreeView;
        }

        public void CreateMainCategoryButtons()
        {
            WrapPanel wrapPanel = new WrapPanel { Orientation = Orientation.Horizontal };

            foreach (TreeViewItem mainItem in categoryTreeView.Items)
            {
                string mainCategory = GetHeaderText(mainItem);
                string imageSource = GetImageSource(mainItem);

                Button button = new Button
                {
                    Style = (Style)mainContentArea.FindResource("SubcategoryButtonStyle"),
                    Tag = mainItem
                };
                button.Click += (s, e) =>
                {
                    mainItem.IsSelected = true;
                    UpdateSubcategoryButtons(mainItem);
                };

                StackPanel buttonContent = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                try
                {
                    buttonContent.Children.Add(new Image
                    {
                        Source = new BitmapImage(new Uri(imageSource, UriKind.Relative)),
                        Width = 40,
                        Height = 40,
                        HorizontalAlignment = HorizontalAlignment.Center
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки изображения {imageSource}: {ex.Message}");
                    buttonContent.Children.Add(new Image
                    {
                        Source = new BitmapImage(new Uri("Ico/Comp.png", UriKind.Relative)),
                        Width = 40,
                        Height = 40,
                        HorizontalAlignment = HorizontalAlignment.Center
                    });
                }
                buttonContent.Children.Add(new TextBlock
                {
                    Style = (Style)mainContentArea.FindResource("SubcategoryTextStyle"),
                    Text = mainCategory
                });

                button.Content = buttonContent;
                wrapPanel.Children.Add(button);
            }

            mainContentArea.Content = wrapPanel;
        }

        public void UpdateSubcategoryButtons(TreeViewItem categoryItem)
        {
            WrapPanel wrapPanel = new WrapPanel { Orientation = Orientation.Horizontal };

            foreach (TreeViewItem subItem in categoryItem.Items)
            {
                string subCategory = GetHeaderText(subItem);
                string imageSource = GetImageSource(subItem);

                Button button = new Button
                {
                    Style = (Style)mainContentArea.FindResource("SubcategoryButtonStyle"),
                    Tag = subItem
                };
                button.Click += (s, e) =>
                {
                    string subCat = GetHeaderText(subItem);
                    if (subCat == "Суммарная информация")
                    {
                        mainContentArea.Content = summaryManager.CreateSummaryPanel();
                    }
                    else if (subCat == "Производительность")
                    {
                        mainContentArea.Visibility = Visibility.Collapsed;
                        performanceTabControl.Visibility = Visibility.Visible;
                        // Удалён вызов StartUpdatingPerformanceData
                    }
                    subItem.IsSelected = true;
                    ExpandCategory(categoryItem);
                };

                StackPanel buttonContent = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                try
                {
                    buttonContent.Children.Add(new Image
                    {
                        Source = new BitmapImage(new Uri(imageSource, UriKind.Relative)),
                        Width = 40,
                        Height = 40,
                        HorizontalAlignment = HorizontalAlignment.Center
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки изображения {imageSource}: {ex.Message}");
                    buttonContent.Children.Add(new Image
                    {
                        Source = new BitmapImage(new Uri("Ico/Comp.png", UriKind.Relative)),
                        Width = 40,
                        Height = 40,
                        HorizontalAlignment = HorizontalAlignment.Center
                    });
                }
                buttonContent.Children.Add(new TextBlock
                {
                    Style = (Style)mainContentArea.FindResource("SubcategoryTextStyle"),
                    Text = subCategory
                });

                button.Content = buttonContent;
                wrapPanel.Children.Add(button);
            }

            mainContentArea.Content = wrapPanel;
        }

        public string GetHeaderText(TreeViewItem item)
        {
            if (item.Header is StackPanel stackPanel)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is TextBlock textBlock)
                    {
                        return textBlock.Text;
                    }
                }
            }
            return item.Header?.ToString() ?? string.Empty;
        }

        public string GetImageSource(TreeViewItem item)
        {
            if (item.Header is StackPanel stackPanel)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is Image image)
                    {
                        string fullPath = image.Source.ToString();
                        int lastSlashIndex = fullPath.LastIndexOf('/');
                        if (lastSlashIndex != -1)
                        {
                            return "Ico/" + fullPath.Substring(lastSlashIndex + 1);
                        }
                        return fullPath;
                    }
                }
            }
            return "Ico/Reserve.png"; // Запасной путь
        }

        private void ExpandCategory(TreeViewItem categoryItem)
        {
            categoryItem.IsExpanded = true;
            categoryItem.BringIntoView();
        }
    }
}