using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for InputFieldWindow.xaml
    /// </summary>
    public partial class InputFieldWindow : Window
    {
        public string InputResult { get => InputTextbox.Text; }
        public string? Label { get => InputLabel.Content.ToString(); set => InputLabel.Content = value; }
        public bool IsNumberField;

        public InputFieldWindow()
        {
            InitializeComponent();

            CancelButton.Click += CancelButton_Click;
            CommitButton.Click += CommitButton_Click;
            InputTextbox.PreviewTextInput += InputTextbox_PreviewTextInput;
        }

        private void InputTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (IsNumberField)
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
        }

        private void CommitButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
