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
    /// Interaction logic for FilterField.xaml
    /// </summary>
    public partial class FilterField : UserControl
    {
        public string? FilterLabel { get; set; }
        public string Value { get; set; }
        public string AttributeName { get; set; }
        private bool _IsAttributeField;
        public bool IsAttributeField { get => _IsAttributeField; set
            {
                _IsAttributeField = value;
                if (value)
                    NameColumn.Width =new(2,GridUnitType.Star);
                else
                    NameColumn.Width = new(0);
            }
        }
        public Action? RemoveClick { get; set; }

        public FilterField()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RemoveClick?.Invoke();
        }
    }
}
