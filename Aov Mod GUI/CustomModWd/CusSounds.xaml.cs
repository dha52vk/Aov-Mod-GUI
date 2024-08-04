using Aov_Mod_GUI.MainWindowControls;
using AovClass;
using AovClass.Models;
using System;
using System.Buffers.Text;
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

namespace Aov_Mod_GUI.CustomModWd
{
    /// <summary>
    /// Interaction logic for CusSounds.xaml
    /// </summary>
    public partial class CusSounds : Window
    {
        Dictionary<string, SoundWrapper> soundWps;
        List<ChangeTextboxField> changeTextboxFields = [];
        ModSources? modSources { get => MainWindow.GetModSources(); }
        List<Hero>? heroes { get => MainWindow.GetHeroList(); }

        public CusSounds(Dictionary<string, SoundWrapper> soundWrappers)
        {
            InitializeComponent();
            soundWps = soundWrappers;
            Loaded += CusSounds_Loaded;
            CancelBtn.Click += CancelBtn_Click;
            AcceptBtn.Click += AcceptBtn_Click;
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CusSounds_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var pair in soundWps)
            {
                ChangeTextboxField ChangeTextbox = new();
                ChangeTextbox.FieldLabel = pair.Key;
                ChangeTextbox.Margin = new Thickness(5, 0, 15, 0);
                FieldContainer.Children.Add(ChangeTextbox);
                changeTextboxFields.Add(ChangeTextbox);
            }
        }
    }
}
