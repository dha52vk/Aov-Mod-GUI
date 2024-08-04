using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aov_Mod_GUI
{
    /// <summary>
    /// Interaction logic for TitleBar.xaml
    /// </summary>
    public partial class TitleBar : UserControl
    {
        public string? Title { get; set; }
        public Brush? BackgroundColor { get; set; }
        public ImageSource? MenuIconSource { get; set; }
        public MenuIconAnimation? MenuAnim { get; set; }

        public event EventHandler? MenuButtonClicked;

        public TitleBar()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.Close();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            if (w.WindowState == WindowState.Maximized)
            {
                w.WindowState = WindowState.Normal;
            }
            else
            {
                w.WindowState = WindowState.Maximized;
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowState = WindowState.Minimized;
        }

        private void DragZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.DragMove();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            switch (MenuAnim)
            {
                case MenuIconAnimation.Spin:
                    Storyboard? s = FindResource("SpinMenuIcon") as Storyboard;
                    if (s != null)
                        s.Begin();
                    break;
                default:
                    break;
            }
            if (MenuButtonClicked != null)
            {
                MenuButtonClicked(this, new EventArgs());
            }
        }
    }

    public enum MenuIconAnimation { None, Spin }
}
