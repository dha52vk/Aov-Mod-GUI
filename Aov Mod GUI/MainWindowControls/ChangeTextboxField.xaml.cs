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
    /// Interaction logic for ChangeTextboxField.xaml
    /// </summary>
    public partial class ChangeTextboxField : UserControl
    {
        public string OldText { get => OldTextBox.Text; set => OldTextBox.Text = value; }
        public string NewText { get => NewTextBox.Text; set => NewTextBox.Text = value; }
        public string FieldLabel { set => TextLabel.Content = value; }

        public ChangeTextboxField()
        {
            InitializeComponent();
        }
    }
}
