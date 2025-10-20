using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Shell32;
using Newtonsoft.Json;
using System.IO;

namespace Computer_Status_Viewer
{
    public class WidgetSettings
    {
        public bool IsDarkTheme { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
    }

    public partial class WidgetWindow : Window
    {
        private bool isDragging = false;
        private Point startPoint;
        private IntPtr hwnd;
        private int initialX;
        private bool isDarkTheme = true;
        private string widgetId;
        private readonly int fixedWidth;
        private static readonly string SettingsFilePath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ComputerStatusViewer",
            "WidgetSettings.json");

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("shell32.dll")]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint dwFlags);

        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SHERB_NOCONFIRMATION = 0x00000001;
        private const uint SHERB_NOPROGRESSUI = 0x00000002;
        private const uint SHERB_NOSOUND = 0x00000004;

        public WidgetWindow(string title, int width, int height, int nextX)
        {
            InitializeComponent();
            fixedWidth = width;
            Width = fixedWidth;
            MinWidth = fixedWidth;
            MaxWidth = fixedWidth;
            Height = height;
            MinHeight = height;
            MaxHeight = height;
            initialX = nextX;

            widgetId = title;

            var settingsDict = LoadSettings();
            if (settingsDict.ContainsKey(widgetId))
            {
                isDarkTheme = settingsDict[widgetId].IsDarkTheme;
                Left = settingsDict[widgetId].PositionX;
                Top = settingsDict[widgetId].PositionY;
            }
            else
            {
                isDarkTheme = true;
                Left = initialX;
                Top = SystemParameters.WorkArea.Height - height;
                settingsDict[widgetId] = new WidgetSettings
                {
                    IsDarkTheme = isDarkTheme,
                    PositionX = Left,
                    PositionY = Top
                };
                SaveSettings(settingsDict);
            }
            ApplyTheme(false);

            Loaded += (s, e) =>
            {
                if (title == "Widget Basic" || title == "Widget Advanced")
                {
                    TitleLabel.Text = title;
                    System1Content.Visibility = Visibility.Visible;
                    RecycleBinContainer.Visibility = Visibility.Collapsed;
                    Height = (title == "Widget Basic") ? 250 : 420;
                }
                else if (title == "SYSTEM 3")
                {
                    System1Content.Visibility = Visibility.Collapsed;
                    RecycleBinContainer.Visibility = Visibility.Visible;
                }

                WindowInteropHelper helper = new WindowInteropHelper(this);
                hwnd = helper.Handle;

                IntPtr progman = FindWindow("Progman", null);
                IntPtr shellDll = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
                IntPtr listView = FindWindowEx(shellDll, IntPtr.Zero, "SysListView32", null);

                if (listView != IntPtr.Zero)
                {
                    SetParent(hwnd, listView);
                    AdjustPosition((int)Left);
                }
                else
                {
                    IntPtr workerW = IntPtr.Zero;
                    while ((workerW = FindWindowEx(IntPtr.Zero, workerW, "WorkerW", null)) != IntPtr.Zero)
                    {
                        IntPtr temp = FindWindowEx(workerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                        if (temp != IntPtr.Zero)
                        {
                            listView = FindWindowEx(temp, IntPtr.Zero, "SysListView32", null);
                            if (listView != IntPtr.Zero)
                            {
                                SetParent(hwnd, listView);
                                AdjustPosition((int)Left);
                                break;
                            }
                        }
                    }
                }

                if (listView == IntPtr.Zero)
                {
                    MessageBox.Show(
                        "Не удалось привязать виджет к рабочему столу.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Left = nextX;
                    Top = SystemParameters.WorkArea.Height - ActualHeight;
                    SavePosition();
                }
            };

            MouseDown += WidgetWindow_MouseDown;
            MouseMove += WidgetWindow_MouseMove;
            MouseUp += WidgetWindow_MouseUp;

            UpdateThemeIcon();
        }

        private Dictionary<string, WidgetSettings> LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    return JsonConvert.DeserializeObject<Dictionary<string, WidgetSettings>>(json) ?? new Dictionary<string, WidgetSettings>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке настроек: {ex.Message}");
            }
            return new Dictionary<string, WidgetSettings>();
        }

        private void SaveSettings(Dictionary<string, WidgetSettings> settingsDict)
        {
            try
            {
                string directory = System.IO.Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                string json = JsonConvert.SerializeObject(settingsDict, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении настроек: {ex.Message}");
            }
        }

        private void SavePosition()
        {
            var settingsDict = LoadSettings();
            if (!settingsDict.ContainsKey(widgetId))
            {
                settingsDict[widgetId] = new WidgetSettings();
            }
            settingsDict[widgetId].PositionX = Left;
            settingsDict[widgetId].PositionY = Top;
            settingsDict[widgetId].IsDarkTheme = isDarkTheme;
            SaveSettings(settingsDict);
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            isDarkTheme = !isDarkTheme;
            ApplyTheme(true);
            UpdateThemeIcon();

            var settingsDict = LoadSettings();
            if (!settingsDict.ContainsKey(widgetId))
            {
                settingsDict[widgetId] = new WidgetSettings();
            }
            settingsDict[widgetId].IsDarkTheme = isDarkTheme;
            settingsDict[widgetId].PositionX = Left;
            settingsDict[widgetId].PositionY = Top;
            SaveSettings(settingsDict);
        }

        private void ApplyTheme(bool animate)
        {
            if (isDarkTheme)
            {
                AnimateColor(GradientStop1, Color.FromRgb(26, 31, 42), animate);
                AnimateColor(GradientStop2, Color.FromRgb(45, 52, 71), animate);
                AnimateBrush(TitleBorder, Color.FromRgb(62, 68, 91), animate);
                AnimateBrush(TitleLabel, Color.FromRgb(212, 215, 224), animate);
                AnimateBrush(InfoLabel, Color.FromRgb(212, 215, 224), animate);

                AnimateBrush(CpuBorder, Color.FromArgb(32, 255, 255, 255), animate);
                AnimateBrush(MemoryBorder, Color.FromArgb(32, 255, 255, 255), animate);
                AnimateBrush(DiskContainer, Color.FromArgb(32, 255, 255, 255), animate);
                AnimateBrush(GpuContainer, Color.FromArgb(32, 255, 255, 255), animate);
                AnimateBrush(ProcessesContainer, Color.FromArgb(32, 255, 255, 255), animate);
                AnimateBrush(UptimeBorder, Color.FromArgb(32, 255, 255, 255), animate);
                AnimateBrush(RecycleBinContainer, Color.FromArgb(32, 255, 255, 255), animate);

                AnimateBrush(CpuLabel, Color.FromRgb(135, 206, 235), animate);
                AnimateBrush(CpuSpeedLabel, Color.FromRgb(176, 196, 222), animate);
                AnimateBrush(MemoryLabel, Color.FromRgb(135, 206, 235), animate);
                AnimateBrush(MemoryUsageLabel, Color.FromRgb(176, 196, 222), animate);
                AnimateBrush(DiskLabel, Color.FromRgb(135, 206, 235), animate);
                AnimateBrush(DiskSpaceLabel, Color.FromRgb(176, 196, 222), animate);
                AnimateBrush(GpuLabel, Color.FromRgb(135, 206, 235), animate);
                AnimateBrush(GpuTempLabel, Color.FromRgb(176, 196, 222), animate);
                AnimateBrush(ProcessesLabel, Color.FromRgb(176, 196, 222), animate);
                AnimateBrush(StreamsLabel, Color.FromRgb(176, 196, 222), animate);
                AnimateBrush(UptimeLabel, Color.FromRgb(135, 206, 235), animate);
                AnimateBrush(UptimeValueLabel, Color.FromRgb(176, 196, 222), animate);

                AnimateBrush(CpuBarBackground, Color.FromRgb(64, 69, 82), animate);
                AnimateBrush(MemoryBarBackground, Color.FromRgb(64, 69, 82), animate);
                AnimateBrush(DiskBarBackground, Color.FromRgb(64, 69, 82), animate);
                AnimateBrush(GpuBarBackground, Color.FromRgb(64, 69, 82), animate);
            }
            else
            {
                AnimateColor(GradientStop1, Color.FromRgb(240, 240, 245), animate);
                AnimateColor(GradientStop2, Color.FromRgb(220, 225, 235), animate);
                AnimateBrush(TitleBorder, Color.FromRgb(200, 205, 215), animate);
                AnimateBrush(TitleLabel, Color.FromRgb(40, 45, 60), animate);
                AnimateBrush(InfoLabel, Color.FromRgb(40, 45, 60), animate);

                AnimateBrush(CpuBorder, Color.FromArgb(32, 0, 0, 0), animate);
                AnimateBrush(MemoryBorder, Color.FromArgb(32, 0, 0, 0), animate);
                AnimateBrush(DiskContainer, Color.FromArgb(32, 0, 0, 0), animate);
                AnimateBrush(GpuContainer, Color.FromArgb(32, 0, 0, 0), animate);
                AnimateBrush(ProcessesContainer, Color.FromArgb(32, 0, 0, 0), animate);
                AnimateBrush(UptimeBorder, Color.FromArgb(32, 0, 0, 0), animate);
                AnimateBrush(RecycleBinContainer, Color.FromArgb(32, 0, 0, 0), animate);

                AnimateBrush(CpuLabel, Color.FromRgb(33, 150, 243), animate);
                AnimateBrush(CpuSpeedLabel, Color.FromRgb(80, 100, 140), animate);
                AnimateBrush(MemoryLabel, Color.FromRgb(33, 150, 243), animate);
                AnimateBrush(MemoryUsageLabel, Color.FromRgb(80, 100, 140), animate);
                AnimateBrush(DiskLabel, Color.FromRgb(33, 150, 243), animate);
                AnimateBrush(DiskSpaceLabel, Color.FromRgb(80, 100, 140), animate);
                AnimateBrush(GpuLabel, Color.FromRgb(33, 150, 243), animate);
                AnimateBrush(GpuTempLabel, Color.FromRgb(80, 100, 140), animate);
                AnimateBrush(ProcessesLabel, Color.FromRgb(80, 100, 140), animate);
                AnimateBrush(StreamsLabel, Color.FromRgb(80, 100, 140), animate);
                AnimateBrush(UptimeLabel, Color.FromRgb(33, 150, 243), animate);
                AnimateBrush(UptimeValueLabel, Color.FromRgb(80, 100, 140), animate);

                AnimateBrush(CpuBarBackground, Color.FromRgb(200, 205, 215), animate);
                AnimateBrush(MemoryBarBackground, Color.FromRgb(200, 205, 215), animate);
                AnimateBrush(DiskBarBackground, Color.FromRgb(200, 205, 215), animate);
                AnimateBrush(GpuBarBackground, Color.FromRgb(200, 205, 215), animate);
            }
        }

        private void AnimateColor(GradientStop gradientStop, Color toColor, bool animate)
        {
            if (animate)
            {
                ColorAnimation animation = new ColorAnimation
                {
                    To = toColor,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };
                gradientStop.BeginAnimation(GradientStop.ColorProperty, animation);
            }
            else
            {
                gradientStop.Color = toColor;
            }
        }

        private void AnimateBrush(Control control, Color toColor, bool animate)
        {
            if (control.Background is SolidColorBrush brush)
            {
                if (animate)
                {
                    ColorAnimation animation = new ColorAnimation
                    {
                        To = toColor,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                    };
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                }
                else
                {
                    brush.Color = toColor;
                }
            }
        }

        private void AnimateBrush(Border border, Color toColor, bool animate)
        {
            if (border.Background is SolidColorBrush brush)
            {
                if (animate)
                {
                    ColorAnimation animation = new ColorAnimation
                    {
                        To = toColor,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                    };
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                }
                else
                {
                    brush.Color = toColor;
                }
            }
        }

        private void AnimateBrush(TextBlock textBlock, Color toColor, bool animate)
        {
            if (textBlock.Foreground is SolidColorBrush brush)
            {
                if (animate)
                {
                    ColorAnimation animation = new ColorAnimation
                    {
                        To = toColor,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                    };
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                }
                else
                {
                    brush.Color = toColor;
                }
            }
        }

        private void AnimateBrush(Rectangle rectangle, Color toColor, bool animate)
        {
            if (rectangle?.Fill is SolidColorBrush brush)
            {
                if (animate)
                {
                    ColorAnimation animation = new ColorAnimation
                    {
                        To = toColor,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                    };
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                }
                else
                {
                    brush.Color = toColor;
                }
            }
        }

        private void UpdateThemeIcon()
        {
            ThemeIcon.Source = new BitmapImage(new Uri(
                isDarkTheme ? "Ico\\Moon.png" : "Ico\\Sun.png",
                UriKind.Relative));
        }

        private void RecycleBinButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var shell = new Shell32.Shell();
                Shell32.Folder recycleBin = shell.NameSpace(Shell32.ShellSpecialFolderConstants.ssfBITBUCKET);
                int itemCount = recycleBin.Items().Count;

                if (itemCount > 0)
                {
                    SHEmptyRecycleBin(IntPtr.Zero, null, 0);
                    UpdateContent("EMPTY\nITEMS: 0  SIZE: 0 MB", new List<(int value, int max)>());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при очистке корзины: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void UpdateContent(string text, List<(int value, int max)> bars)
        {
            if (TitleLabel.Text == "Widget Basic" || TitleLabel.Text == "Widget Advanced")
            {
                string[] lines = text.Split('\n');
                if (TitleLabel.Text == "Widget Basic" && lines.Length >= 5)
                {
                    CpuLabel.Text = lines[0];
                    CpuSpeedLabel.Text = lines[1];
                    MemoryLabel.Text = lines[2];
                    MemoryUsageLabel.Text = lines[3];
                    UptimeValueLabel.Text = lines[4];
                    UptimeLabel.Text = "UPTIME";

                    if (bars.Count > 0) UpdateProgressBarWithAnimation(CpuBarBackground, CpuBarFill, bars[0].value, bars[0].max);
                    if (bars.Count > 1) UpdateProgressBarWithAnimation(MemoryBarBackground, MemoryBarFill, bars[1].value, bars[1].max);

                    DiskContainer.Visibility = Visibility.Collapsed;
                    GpuContainer.Visibility = Visibility.Collapsed;
                    ProcessesContainer.Visibility = Visibility.Collapsed;
                }
                else if (TitleLabel.Text == "Widget Advanced" && lines.Length >= 11)
                {
                    CpuLabel.Text = lines[0];
                    CpuSpeedLabel.Text = lines[1];
                    MemoryLabel.Text = lines[2];
                    MemoryUsageLabel.Text = lines[3];
                    DiskLabel.Text = lines[4];
                    DiskSpaceLabel.Text = lines[5];
                    GpuLabel.Text = lines[6];
                    GpuTempLabel.Text = lines[7];
                    ProcessesLabel.Text = lines[8];
                    StreamsLabel.Text = lines[9];
                    UptimeValueLabel.Text = lines[10];
                    UptimeLabel.Text = "UPTIME";

                    if (bars.Count > 0) UpdateProgressBarWithAnimation(CpuBarBackground, CpuBarFill, bars[0].value, bars[0].max);
                    if (bars.Count > 1) UpdateProgressBarWithAnimation(MemoryBarBackground, MemoryBarFill, bars[1].value, bars[1].max);
                    if (bars.Count > 2) UpdateProgressBarWithAnimation(DiskBarBackground, DiskBarFill, bars[2].value, bars[2].max);
                    if (bars.Count > 3) UpdateProgressBarWithAnimation(GpuBarBackground, GpuBarFill, bars[3].value, bars[3].max);

                    DiskContainer.Visibility = Visibility.Visible;
                    GpuContainer.Visibility = Visibility.Visible;
                    ProcessesContainer.Visibility = Visibility.Visible;
                }
            }
            else // SYSTEM 3
            {
                string[] lines = text.Split('\n');
                TitleLabel.Text = lines[0];
                InfoLabel.Text = lines[1];

                if (RecycleBinButton != null)
                {
                    var image = RecycleBinButton.Content as Image;
                    if (image != null)
                    {
                        image.Source = new BitmapImage(new Uri(
                            TitleLabel.Text == "EMPTY" ? "Ico\\Basket1.png" : "Ico\\Basket.png",
                            UriKind.Relative));
                    }
                    RecycleBinButton.IsEnabled = TitleLabel.Text != "EMPTY";
                }
            }
            AdjustPosition((int)Left);
        }

        private void AdjustPosition(int xPos)
        {
            UpdateLayout();
            int newTop = (int)Top;
            if (Top == 0 && Left == 0)
            {
                newTop = (int)(SystemParameters.WorkArea.Height - ActualHeight);
            }
            if (newTop < 0) newTop = 0;
            SetWindowPos(hwnd, IntPtr.Zero, xPos, newTop, fixedWidth, (int)ActualHeight, SWP_NOACTIVATE | SWP_SHOWWINDOW);
            Left = xPos;
            Top = newTop;
            SavePosition();
        }

        private void UpdateProgressBarWithAnimation(Rectangle background, Rectangle fill, int value, int max)
        {
            if (background == null || fill == null)
            {
                Console.WriteLine("Не удалось найти элементы прогресс-бара.");
                return;
            }

            double barWidth = fixedWidth - 60;
            if (barWidth < 0) barWidth = 100;
            double percentage = (max > 0 && value >= 0) ? Math.Min(value / (double)max, 1.0) : 0;
            double fillWidth = percentage * barWidth;

            background.Width = barWidth;

            double currentWidth = fill.Width;
            if (double.IsNaN(currentWidth) || currentWidth < 0) currentWidth = 0;

            DoubleAnimation animation = new DoubleAnimation
            {
                From = currentWidth,
                To = fillWidth,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            fill.BeginAnimation(Rectangle.WidthProperty, animation);
        }

        private void WidgetWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isDragging = true;
                startPoint = e.GetPosition(this);
                CaptureMouse();
            }
        }

        private void WidgetWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                UpdateLayout();
                Point currentPosition = e.GetPosition(this);
                double deltaX = currentPosition.X - startPoint.X;
                double deltaY = currentPosition.Y - startPoint.Y;

                int newLeft = (int)(Left + deltaX);
                int newTop = (int)(Top + deltaY);

                newLeft = Math.Max(0, Math.Min(newLeft, (int)SystemParameters.WorkArea.Width - fixedWidth));
                newTop = Math.Max(0, Math.Min(newTop, (int)SystemParameters.WorkArea.Height - (int)ActualHeight));

                SetWindowPos(hwnd, IntPtr.Zero, newLeft, newTop, fixedWidth, (int)ActualHeight, SWP_NOACTIVATE | SWP_SHOWWINDOW);
                Left = newLeft;
                Top = newTop;
            }
        }

        private void WidgetWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                isDragging = false;
                ReleaseMouseCapture();
                SavePosition();
            }
        }
    }
}