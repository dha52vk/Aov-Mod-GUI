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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aov_Mod_GUI.MainWindowControls
{
    /// <summary>
    /// Interaction logic for SkinPackField.xaml
    /// </summary>
    public partial class SkinPackField : UserControl
    {
        public string FieldLabel { get => TitleLabel.Content.ToString()??""; set => TitleLabel.Content = value; }
        public Brush TextColor { get; set; }
        public double FieldFontSize { get; set; }
        public RoutedEventHandler? RemoveButtonClick;

        private List<int>? _SkinList;
        public List<int>? SkinList
        {
            get { return _SkinList; }
            set
            {
                _SkinList = value;
                SkinListTxt.Text = string.Join(", ", _SkinList ?? []);
            }
        }

        public SkinPackField()
        {
            InitializeComponent();
            DataContext = this;
            FieldFontSize = 18;
            TextColor = Brushes.White;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (RemoveButtonClick != null)
            {
                RemoveButtonClick(sender, e);
            }
        }
    }
}
