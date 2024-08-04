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

namespace Aov_Mod_GUI
{
    /// <summary>
    /// Interaction logic for MenuRadioButton.xaml
    /// </summary>
    public partial class MenuRadioButton : RadioButton
    {
        public ImageSource? MenuIconSource { get; set; }
        public string? MenuLabel { get; set; }

        public MenuRadioButton()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
