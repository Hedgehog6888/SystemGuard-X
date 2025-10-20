using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;
using System.Drawing.Printing;

namespace Computer_Status_Viewer
{
    public class RegionalSettingsManager : IDisposable
    {
        private readonly ResourceDictionary resources;
        private ScrollViewer cachedRegionalSettingsPanel;

        public RegionalSettingsManager(ResourceDictionary resources)
        {
            this.resources = resources;
        }

        public UIElement CreateRegionalSettingsPanel()
        {
            if (cachedRegionalSettingsPanel != null)
            {
                return cachedRegionalSettingsPanel;
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

            listView.Items.Add(CreateCategorySection("Временная зона", GetTimeZoneParameters()));
            listView.Items.Add(CreateCategorySection("Язык", GetLanguageParameters()));
            listView.Items.Add(CreateCategorySection("Страна/Регион", GetRegionParameters()));
            listView.Items.Add(CreateCategorySection("Символ денежной единицы", GetCurrencySymbolParameters()));
            listView.Items.Add(CreateCategorySection("Форматирование", GetFormattingParameters()));
            listView.Items.Add(CreateCategorySection("Дни недели", GetDaysOfWeekParameters()));
            listView.Items.Add(CreateCategorySection("Месяцы", GetMonthsParameters()));
            listView.Items.Add(CreateCategorySection("Разное", GetMiscellaneousParameters()));

            cachedRegionalSettingsPanel = scrollViewer;
            return cachedRegionalSettingsPanel;
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

            listBox.ItemContainerStyleSelector = new AlternatingRowStyleSelector();
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

                return style;
            }
        }

        private string GetIconForCategory(string category)
        {
            switch (category)
            {
                case "Временная зона":
                    return "Ico/Watch.png";
                case "Язык":
                    return "Ico/Language.png";
                case "Страна/Регион":
                    return "Ico/Region.png";
                case "Символ денежной единицы":
                    return "Ico/Currency.png";
                case "Форматирование":
                    return "Ico/Font.png";
                case "Дни недели":
                    return "Ico/Dates.png";
                case "Месяцы":
                    return "Ico/Dates.png";
                case "Разное":
                    return "Ico/Config.png";
                default:
                    return "Ico/Reserve.png";
            }
        }

        private string GetIconForParameter(string field)
        {
            switch (field)
            {
                case "Текущая временная зона":
                    return "Ico/Watch.png";
                case "Основная текущая зона":
                    return "Ico/Watch.png";
                case "Перевод на стандартное время":
                    return "Ico/Watch.png";

                case "Язык":
                    return "Ico/Language.png";
                case "Язык (ISO 639)":
                    return "Ico/Language.png";

                case "Страна":
                    return "Ico/Region.png";
                case "Страна (ISO 3166)":
                    return "Ico/Region.png";
                case "Код страны":
                    return "Ico/Region.png";

                case "Денежная единица":
                case "Символ денежной единицы": // Добавляем иконку для нового параметра
                case "Формат денежной единицы":
                    return "Ico/Currency.png";

                case "Формат времени":
                    return "Ico/Font.png";
                case "Формат даты":
                    return "Ico/Font.png";
                case "Формат вывода чисел":
                    return "Ico/Font.png";
                case "Формат списка":
                    return "Ico/Font.png";
                case "Набор цифр":
                    return "Ico/Font.png";

                case "Понедельник":
                case "Вторник":
                case "Среда":
                case "Четверг":
                case "Пятница":
                case "Суббота":
                case "Воскресенье":
                    return "Ico/Dates.png";

                case "Январь":
                case "Февраль":
                case "Март":
                case "Апрель":
                case "Май":
                case "Июнь":
                case "Июль":
                case "Август":
                case "Сентябрь":
                case "Октябрь":
                case "Ноябрь":
                case "Декабрь":
                    return "Ico/Dates.png";

                case "Тип календаря":
                    return "Ico/Time.png";
                case "Размер бумаги по умолчанию":
                    return "Ico/Paper.png";
                case "Система счисления":
                    return "Ico/Counting.png";

                default:
                    return "Ico/Reserve.png";
            }
        }

        public class SystemParameter
        {
            public string Field { get; set; }
            public string Value { get; set; }
        }

        private List<SystemParameter> GetTimeZoneParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();
            TimeZoneInfo timeZone = TimeZoneInfo.Local;

            parameters.Add(new SystemParameter { Field = "Текущая временная зона", Value = timeZone.DisplayName });
            parameters.Add(new SystemParameter { Field = "Основная текущая зона", Value = timeZone.StandardName });
            parameters.Add(new SystemParameter { Field = "Перевод на стандартное время", Value = timeZone.IsDaylightSavingTime(DateTime.Now) ? "Да" : "—" });

            return parameters;
        }

        private List<SystemParameter> GetLanguageParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();
            CultureInfo culture = CultureInfo.CurrentCulture;
            parameters.Add(new SystemParameter { Field = "Язык", Value = culture.DisplayName });
            parameters.Add(new SystemParameter { Field = "Язык (ISO 639)", Value = culture.TwoLetterISOLanguageName });
            return parameters;
        }

        private List<SystemParameter> GetRegionParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();
            CultureInfo culture = CultureInfo.CurrentCulture;
            RegionInfo region;

            try
            {
                region = new RegionInfo(culture.Name);
            }
            catch (ArgumentException)
            {
                region = new RegionInfo(CultureInfo.InvariantCulture.Name);
            }

            parameters.Add(new SystemParameter { Field = "Страна", Value = region.DisplayName });
            parameters.Add(new SystemParameter { Field = "Страна (ISO 3166)", Value = region.TwoLetterISORegionName });
            parameters.Add(new SystemParameter { Field = "Код страны", Value = GetCountryPhoneCode(region.TwoLetterISORegionName) });
            return parameters;
        }

        private List<SystemParameter> GetCurrencySymbolParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();
            CultureInfo culture = CultureInfo.CurrentCulture;
            RegionInfo region;

            try
            {
                region = new RegionInfo(culture.Name);
            }
            catch (ArgumentException)
            {
                region = new RegionInfo(CultureInfo.InvariantCulture.Name);
            }

            decimal sampleNumber = 123456789.00m;
            parameters.Add(new SystemParameter { Field = "Денежная единица", Value = region.ISOCurrencySymbol });
            parameters.Add(new SystemParameter { Field = "Символ денежной единицы", Value = culture.NumberFormat.CurrencySymbol }); // Добавляем символ валюты
            parameters.Add(new SystemParameter { Field = "Формат денежной единицы", Value = sampleNumber.ToString("C", culture) });

            return parameters;
        }

        private List<SystemParameter> GetFormattingParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();
            CultureInfo culture = CultureInfo.CurrentCulture;
            decimal sampleNumber = 123456789.00m;

            parameters.Add(new SystemParameter { Field = "Формат времени", Value = culture.DateTimeFormat.ShortTimePattern });
            parameters.Add(new SystemParameter { Field = "Формат даты", Value = culture.DateTimeFormat.ShortDatePattern });
            parameters.Add(new SystemParameter { Field = "Формат вывода чисел", Value = sampleNumber.ToString("N", culture) });
            parameters.Add(new SystemParameter { Field = "Формат списка", Value = $"first{culture.TextInfo.ListSeparator} second{culture.TextInfo.ListSeparator} third" });
            parameters.Add(new SystemParameter { Field = "Набор цифр", Value = string.Join("", culture.NumberFormat.NativeDigits) });
            return parameters;
        }

        private List<SystemParameter> GetDaysOfWeekParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();
            CultureInfo culture = CultureInfo.CurrentCulture;
            string[] dayNames = culture.DateTimeFormat.DayNames;
            string[] abbrDayNames = culture.DateTimeFormat.AbbreviatedDayNames;

            parameters.Add(new SystemParameter { Field = "Понедельник", Value = $"{dayNames[0]} / {abbrDayNames[0]}" });
            parameters.Add(new SystemParameter { Field = "Вторник", Value = $"{dayNames[1]} / {abbrDayNames[1]}" });
            parameters.Add(new SystemParameter { Field = "Среда", Value = $"{dayNames[2]} / {abbrDayNames[2]}" });
            parameters.Add(new SystemParameter { Field = "Четверг", Value = $"{dayNames[3]} / {abbrDayNames[3]}" });
            parameters.Add(new SystemParameter { Field = "Пятница", Value = $"{dayNames[4]} / {abbrDayNames[4]}" });
            parameters.Add(new SystemParameter { Field = "Суббота", Value = $"{dayNames[5]} / {abbrDayNames[5]}" });
            parameters.Add(new SystemParameter { Field = "Воскресенье", Value = $"{dayNames[6]} / {abbrDayNames[6]}" });
            return parameters;
        }

        private List<SystemParameter> GetMonthsParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();
            CultureInfo culture = CultureInfo.CurrentCulture;
            string[] monthNames = culture.DateTimeFormat.MonthNames;
            string[] abbrMonthNames = culture.DateTimeFormat.AbbreviatedMonthNames;

            parameters.Add(new SystemParameter { Field = "Январь", Value = $"{monthNames[0]} / {abbrMonthNames[0]}" });
            parameters.Add(new SystemParameter { Field = "Февраль", Value = $"{monthNames[1]} / {abbrMonthNames[1]}" });
            parameters.Add(new SystemParameter { Field = "Март", Value = $"{monthNames[2]} / {abbrMonthNames[2]}" });
            parameters.Add(new SystemParameter { Field = "Апрель", Value = $"{monthNames[3]} / {abbrMonthNames[3]}" });
            parameters.Add(new SystemParameter { Field = "Май", Value = $"{monthNames[4]} / {abbrMonthNames[4]}" });
            parameters.Add(new SystemParameter { Field = "Июнь", Value = $"{monthNames[5]} / {abbrMonthNames[5]}" });
            parameters.Add(new SystemParameter { Field = "Июль", Value = $"{monthNames[6]} / {abbrMonthNames[6]}" });
            parameters.Add(new SystemParameter { Field = "Август", Value = $"{monthNames[7]} / {abbrMonthNames[7]}" });
            parameters.Add(new SystemParameter { Field = "Сентябрь", Value = $"{monthNames[8]} / {abbrMonthNames[8]}" });
            parameters.Add(new SystemParameter { Field = "Октябрь", Value = $"{monthNames[9]} / {abbrMonthNames[9]}" });
            parameters.Add(new SystemParameter { Field = "Ноябрь", Value = $"{monthNames[10]} / {abbrMonthNames[10]}" });
            parameters.Add(new SystemParameter { Field = "Декабрь", Value = $"{monthNames[11]} / {abbrMonthNames[11]}" });
            return parameters;
        }

        private List<SystemParameter> GetMiscellaneousParameters()
        {
            List<SystemParameter> parameters = new List<SystemParameter>();
            CultureInfo culture = CultureInfo.CurrentCulture;
            PrinterSettings printerSettings = new PrinterSettings();

            parameters.Add(new SystemParameter { Field = "Тип календаря", Value = culture.Calendar.ToString().Replace("System.Globalization.", "") });
            parameters.Add(new SystemParameter { Field = "Размер бумаги по умолчанию", Value = printerSettings.DefaultPageSettings.PaperSize.PaperName });
            parameters.Add(new SystemParameter { Field = "Система счисления", Value = GetNumberSystemDescription(culture) });
            return parameters;
        }

        private string GetNumberSystemDescription(CultureInfo culture)
        {
            string digits = string.Join("", culture.NumberFormat.NativeDigits);
            if (digits == "0123456789")
            {
                return "Десятичная (арабские цифры)";
            }
            else
            {
                return $"Особая (используемые цифры: {digits})";
            }
        }

        private string GetCountryPhoneCode(string twoLetterISORegionName)
        {
            // Примечание: Это статический словарь, так как .NET не предоставляет прямого доступа к телефонным кодам из системных настроек
            Dictionary<string, string> countryCodes = new Dictionary<string, string>
            {
                { "RU", "7" },
                { "US", "1" },
                { "GB", "44" },
                { "FR", "33" },
                { "DE", "49" },
                { "JP", "81" },
                { "CN", "86" },
                { "IN", "91" }
            };

            return countryCodes.TryGetValue(twoLetterISORegionName, out var code) ? code : "Н/Д";
        }

        public void Dispose()
        {
            if (cachedRegionalSettingsPanel != null)
            {
                cachedRegionalSettingsPanel.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
            }
        }
    }
}