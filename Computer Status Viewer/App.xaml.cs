using System;
using System.Threading;
using System.Windows;
using System.Threading.Tasks;
using System.Diagnostics;
using Computer_Status_Viewer.Properties;
using System.IO;
using WinForms = System.Windows.Forms;
using System.IO.Pipes;

namespace Computer_Status_Viewer
{
    public partial class App : Application
    {
        private SplashScreen splashScreen;
        private Thread splashThread;
        private TaskCompletionSource<bool> splashScreenReady = new TaskCompletionSource<bool>();
        private WidgetManager widgetManager;
        private WinForms.NotifyIcon trayIcon;
        private MainWindow mainWindow;
        private static readonly string LogPath = @"C:\Temp\SystemGuardX.log";
        private static readonly string MutexName = "SystemGuardXMutex";
        private static readonly string PipeName = "SystemGuardXPipe";
        private Mutex mutex;
        private CancellationTokenSource pipeCts;

        protected override async void OnStartup(StartupEventArgs e)
        {
            Log("Начало OnStartup");

            try
            {
                ShowSplashScreen();
                Log("SplashScreen показан");

                bool isNewInstance;
                mutex = new Mutex(true, MutexName, out isNewInstance);

                if (!isNewInstance)
                {
                    Log("Другая копия уже запущена, отправляем сообщение для показа окна");
                    SendShowWindowMessage();
                    Shutdown();
                    return;
                }

                base.OnStartup(e);

                Directory.CreateDirectory(@"C:\Temp");
                Log("Папка для логов создана");

                Log($"Запуск приложения, аргументы: {string.Join(",", e.Args)}");

                widgetManager = new WidgetManager();
                Log("WidgetManager создан");

                bool isAutoStart = e.Args.Length > 0 && e.Args[0] == "/autostart";
                Log($"Автозапуск: {isAutoStart}");

                if (!Settings.Default.SettingsInitialized)
                {
                    Log("Первый запуск приложения, инициализация настроек");
                    Settings.Default.IsCpuWidgetActive = false;
                    Settings.Default.IsRamWidgetActive = false;
                    Settings.Default.IsDiskWidgetActive = false;
                    Settings.Default.SettingsInitialized = true;
                    Settings.Default.Save();
                    Log("Настройки сохранены");
                }

                bool isCpuWidgetActive = Settings.Default.IsCpuWidgetActive;
                bool isRamWidgetActive = Settings.Default.IsRamWidgetActive;
                bool isDiskWidgetActive = Settings.Default.IsDiskWidgetActive;
                Log($"Виджеты - CPU: {isCpuWidgetActive}, RAM: {isRamWidgetActive}, Disk: {isDiskWidgetActive}");

                InitializeTrayIcon();
                Log("Иконка в трее инициализирована");

                pipeCts = new CancellationTokenSource();
                var pipeTask = Task.Run(() => ListenForShowWindowMessage(pipeCts.Token));
                Log("Запущена задача обработки сообщений через пайп");

                mainWindow = new MainWindow(widgetManager);
                Log("MainWindow создан");

                try
                {
                    await mainWindow.InitializeAsync();
                    Log("InitializeAsync выполнен");
                }
                catch (Exception ex)
                {
                    Log($"Ошибка в InitializeAsync: {ex.Message}\nStackTrace: {ex.StackTrace}");
                }

                try
                {
                    await mainWindow.LoadContentAsync();
                    Log("LoadContentAsync выполнен");
                }
                catch (Exception ex)
                {
                    Log($"Ошибка в LoadContentAsync: {ex.Message}\nStackTrace: {ex.StackTrace}");
                }

                if (isAutoStart)
                {
                    Log("Автозапуск: скрываем главное окно");
                    mainWindow.Hide();
                    if (isCpuWidgetActive) widgetManager.CreateWidget1();
                    if (isRamWidgetActive) widgetManager.CreateWidget2();
                    if (isDiskWidgetActive) widgetManager.CreateWidget3();
                    await CloseSplashScreenAsync(); // Исправлено на асинхронный метод
                }
                else
                {
                    Log("Обычный запуск: ждем сплеш-скрин");
                    await splashScreenReady.Task;
                    Log("Сплеш-скрин готов, ждем 2 секунды");
                    await Task.Delay(2000);
                    Log("Сплеш-скрин закрывается");
                    await CloseSplashScreenAsync();
                    mainWindow.Show();
                    MainWindow = mainWindow;
                    mainWindow.Topmost = true;
                    mainWindow.Topmost = false;
                    mainWindow.Activate();
                    Log("Главное окно показано");

                    if (isCpuWidgetActive) widgetManager.CreateWidget1();
                    if (isRamWidgetActive) widgetManager.CreateWidget2();
                    if (isDiskWidgetActive) widgetManager.CreateWidget3();
                }
            }
            catch (Exception ex)
            {
                Log($"Критическая ошибка в OnStartup: {ex.Message}\nStackTrace: {ex.StackTrace}");
                MessageBox.Show($"Критическая ошибка при запуске: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                await CloseSplashScreenAsync(); // Исправлено на асинхронный метод
                // Не вызываем Shutdown(), чтобы программа осталась в трее
            }
        }

        private void Log(string message)
        {
            try
            {
                File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss.fff}: {message}\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка логирования: {ex.Message}");
            }
        }

        private void ShowSplashScreen()
        {
            splashThread = new Thread(() =>
            {
                splashScreen = new SplashScreen();
                splashScreen.Closed += (s, e) =>
                {
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
                };
                splashScreen.Show();
                splashScreenReady.SetResult(true);
                Log("SplashScreen показан в потоке");
                System.Windows.Threading.Dispatcher.Run();
            });
            splashThread.SetApartmentState(ApartmentState.STA);
            splashThread.IsBackground = true;
            splashThread.Start();
            Log("SplashScreen запущен в отдельном потоке");
        }

        private async Task CloseSplashScreenAsync()
        {
            if (splashScreen != null && splashThread != null && splashThread.IsAlive)
            {
                await splashScreen.Dispatcher.InvokeAsync(() =>
                {
                    splashScreen.Close();
                    Log("SplashScreen закрыт");
                });
                await Task.Run(() => splashThread.Join(1000)); // Ждем завершения потока
                if (splashThread.IsAlive)
                {
                    Log("Принудительное завершение потока SplashScreen");
                    splashThread.Abort(); // Используем как последнее средство
                }
                splashScreen = null;
            }
        }

        private void InitializeTrayIcon()
        {
            if (trayIcon != null)
            {
                Log("Иконка в трее уже существует, удаляем старую");
                trayIcon.Dispose();
            }

            trayIcon = new WinForms.NotifyIcon
            {
                Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/Ico/Logo.ico")).Stream),
                Visible = true,
                Text = "SystemGuard X"
            };
            Log("Новая иконка в трее создана");

            trayIcon.DoubleClick += (s, e) => ShowMainWindow();

            var contextMenu = new WinForms.ContextMenuStrip();
            contextMenu.Items.Add("Открыть", null, (s, e) => ShowMainWindow());
            contextMenu.Items.Add("Выход", null, (s, e) =>
            {
                Log("Запрос выхода из трея");
                trayIcon.Visible = false;
                Shutdown();
            });
            trayIcon.ContextMenuStrip = contextMenu;
        }

        private void ShowMainWindow()
        {
            Log("Вызов ShowMainWindow");
            if (mainWindow == null)
            {
                Log("Главное окно null, создаем новое");
                mainWindow = new MainWindow(widgetManager); // Исправлено MainMainWindow на MainWindow
                mainWindow.Show();
            }
            else
            {
                Log("Главное окно существует, показываем его");
                if (!mainWindow.IsVisible)
                {
                    mainWindow.Show();
                }
                if (mainWindow.WindowState == WindowState.Minimized)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }
                mainWindow.Topmost = true;
                mainWindow.Topmost = false;
                mainWindow.Activate();
            }
        }

        private void SendShowWindowMessage()
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    client.Connect(1000);
                    using (var writer = new StreamWriter(client))
                    {
                        writer.WriteLine("SHOW_WINDOW");
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Не удалось отправить сообщение показа окна: {ex.Message}");
            }
        }

        private async Task ListenForShowWindowMessage(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In))
                    {
                        await Task.Run(() => server.WaitForConnection(), cancellationToken);
                        if (server.IsConnected)
                        {
                            using (var reader = new StreamReader(server))
                            {
                                string message = await reader.ReadLineAsync();
                                if (message == "SHOW_WINDOW")
                                {
                                    Log("Получено сообщение SHOW_WINDOW");
                                    Dispatcher.Invoke(() => ShowMainWindow());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"Ошибка пайпа: {ex.Message}");
                }
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            Log("Выход из приложения");
            pipeCts?.Cancel();
            trayIcon?.Dispose();
            widgetManager?.Dispose();
            await CloseSplashScreenAsync(); // Исправлено на асинхронный метод
            mutex?.ReleaseMutex();
            mutex?.Dispose();
            base.OnExit(e);
        }
    }
}