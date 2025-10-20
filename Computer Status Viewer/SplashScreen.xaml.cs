using System;
using System.Windows;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Computer_Status_Viewer
{
    public partial class SplashScreen : Window
    {
        private TaskCompletionSource<bool> _renderCompletionSource = new TaskCompletionSource<bool>();
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public SplashScreen()
        {
            try
            {
                InitializeComponent();
                ProgressBar.IsIndeterminate = true;
                Loaded += SplashScreen_Loaded;
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] SplashScreen создан");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ошибка инициализации: {ex.Message}");
                throw;
            }
        }

        private void SplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] SplashScreen загружен за {_stopwatch.ElapsedMilliseconds}мс");
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            _renderCompletionSource.SetResult(true);
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] SplashScreen отрендерен за {_stopwatch.ElapsedMilliseconds}мс");
        }

        public Task WaitForRenderAsync() => _renderCompletionSource.Task;
    }
}