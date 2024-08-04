using System.Reflection;
using System.Windows;

namespace Aov_Mod_GUI
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {

        public SplashWindow()
        {
            InitializeComponent();
            Activated += (s, e) => LoadMainWindow();
            AppName.Content = Assembly.GetEntryAssembly()?.GetName().Name;
        }

        public async void LoadMainWindow()
        {
            MainWindow mainWindow = new();
            mainWindow.ShowInTaskbar = false;
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Minimized;
            await Task.Run(() =>
            {
                while (!mainWindow.IsListLoaded)
                {
                    Thread.Sleep(500);
                }
            });
            mainWindow.ShowInTaskbar = true;
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
            Close();
        }
    }
}
