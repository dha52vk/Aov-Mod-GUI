using Aov_Mod_GUI.Models;
using AovClass.Models;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Aov_Mod_GUI
{
    /// <summary>
    /// Interaction logic for ChooseSkinWindow.xaml
    /// </summary>
    public partial class ChooseSkinWindow : Window
    {
        public List<Skin> skins;
        public Skin? Result;

        public ChooseSkinWindow(List<Skin> skins)
        {
            InitializeComponent();

            this.skins = skins;
            ListItem.ItemsSource = skins;
            SkinResultName.Text = "Chưa chọn";
        }

        private void ListItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                var selectedItem = (FrameworkElement)e.OriginalSource;
                var itemDataContext = selectedItem.DataContext;

                if (itemDataContext is Skin skin)
                {
                    SkinResultName.Text = skin.Name + $" ({skin.Id})";
                    Result = skin;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
