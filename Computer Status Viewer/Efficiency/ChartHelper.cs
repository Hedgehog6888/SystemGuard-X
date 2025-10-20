using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace Computer_Status_Viewer
{
    public static class ChartHelper
    {
        public static CartesianChart SetupChart(ChartValues<double> values, SolidColorBrush color, string yAxisTitle, double maxValue = 100)
        {
            var chart = new CartesianChart
            {
                Margin = new Thickness(0, 0, 0, 0),
                LegendLocation = LegendLocation.None,
                DisableAnimations = true,
                Background = Brushes.White
            };

            chart.DataTooltip = null;

            chart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Values = values,
                    Stroke = color,
                    Fill = new SolidColorBrush(Color.FromArgb(50, color.Color.R, color.Color.G, color.Color.B)),
                    PointGeometry = null,
                    StrokeThickness = 2,
                    LineSmoothness = 0
                }
            };

            chart.AxisX.Add(new Axis
            {
                ShowLabels = false,
                Separator = new LiveCharts.Wpf.Separator { Step = 10 },
                MinValue = 0,
                MaxValue = 59
            });

            chart.AxisY.Add(new Axis
            {
                MinValue = 0,
                MaxValue = maxValue,
                LabelFormatter = value => $"{value:F1}",
                Foreground = Brushes.Gray,
                FontSize = 12
            });

            return chart;
        }

        public static CartesianChart SetupNetworkChart(ChartValues<double> sendValues, ChartValues<double> receiveValues, string yAxisTitle)
        {
            var chart = new CartesianChart
            {
                Margin = new Thickness(0, 0, 0, 0),
                LegendLocation = LegendLocation.None,
                DisableAnimations = true,
                Background = Brushes.White
            };

            chart.DataTooltip = null;

            chart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Values = receiveValues,
                    Stroke = Brushes.Purple,
                    Fill = new SolidColorBrush(Color.FromArgb(50, 128, 0, 128)),
                    PointGeometry = null,
                    StrokeThickness = 2,
                    LineSmoothness = 0,
                    Title = "Получение"
                },
                new LineSeries
                {
                    Values = sendValues,
                    Stroke = Brushes.Purple,
                    Fill = Brushes.Transparent,
                    PointGeometry = null,
                    StrokeThickness = 2,
                    LineSmoothness = 0,
                    StrokeDashArray = new DoubleCollection { 2, 2 },
                    Title = "Отправка"
                }
            };

            chart.AxisX.Add(new Axis
            {
                ShowLabels = false,
                Separator = new LiveCharts.Wpf.Separator { Step = 10 },
                MinValue = 0,
                MaxValue = 59
            });

            chart.AxisY.Add(new Axis
            {
                MinValue = 0,
                MaxValue = 100,
                LabelFormatter = value => value < 1000 ? $"{value:F1} Кбит/с" : $"{(value / 1000):F1} Мбит/с",
                Foreground = Brushes.Gray,
                FontSize = 12
            });

            return chart;
        }

        public static CartesianChart SetupDiskChart(ChartValues<double> readValues, ChartValues<double> writeValues, string yAxisTitle)
        {
            var chart = new CartesianChart
            {
                Margin = new Thickness(0, 0, 0, 0),
                LegendLocation = LegendLocation.None,
                DisableAnimations = true,
                Background = Brushes.White
            };

            chart.DataTooltip = null;

            chart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Values = readValues,
                    Stroke = Brushes.Orange,
                    Fill = new SolidColorBrush(Color.FromArgb(50, 255, 165, 0)),
                    PointGeometry = null,
                    StrokeThickness = 2,
                    LineSmoothness = 0,
                    Title = "Чтение"
                },
                new LineSeries
                {
                    Values = writeValues,
                    Stroke = Brushes.Orange,
                    Fill = Brushes.Transparent,
                    PointGeometry = null,
                    StrokeThickness = 2,
                    LineSmoothness = 0,
                    StrokeDashArray = new DoubleCollection { 2, 2 },
                    Title = "Запись"
                }
            };

            chart.AxisX.Add(new Axis
            {
                ShowLabels = false,
                Separator = new LiveCharts.Wpf.Separator { Step = 10 },
                MinValue = 0,
                MaxValue = 59
            });

            chart.AxisY.Add(new Axis
            {
                MinValue = 0,
                MaxValue = 100,
                LabelFormatter = value => value < 1024 ? $"{value:F1} КБ/с" : $"{(value / 1024):F1} МБ/с",
                Foreground = Brushes.Gray,
                FontSize = 12
            });

            return chart;
        }

        public static Grid CreateDataRow(string label, string value, bool boldLabel, int fontSize = 12)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            TextBlock labelBlock = new TextBlock
            {
                Text = label,
                FontWeight = boldLabel ? FontWeights.Bold : FontWeights.Normal,
                Foreground = new SolidColorBrush(Color.FromRgb(105, 105, 105)),
                Margin = new Thickness(0, 0, 10, 2),
                FontSize = fontSize
            };
            Grid.SetColumn(labelBlock, 0);
            grid.Children.Add(labelBlock);

            TextBlock valueBlock = new TextBlock
            {
                Text = value,
                Foreground = Brushes.Black,
                Margin = new Thickness(0, 0, 0, 2),
                FontSize = fontSize
            };
            Grid.SetColumn(valueBlock, 1);
            grid.Children.Add(valueBlock);

            return grid;
        }

        public static string FormatSpeed(double speed)
        {
            if (speed >= 1024)
            {
                return $"{(speed / 1024):F1} ГБ/с";
            }
            return $"{speed:F1} МБ/с";
        }
    }
}