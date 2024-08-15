using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aov_Mod_GUI
{
    /// <summary>
    /// Interaction logic for PathTextedit.xaml
    /// </summary>
    public partial class PathTextedit : UserControl
    {
        public string? PathLabel { get; set; }
        public bool IsFileSelect { get; set; }
        public bool IsFolderSelect { get; set; }
        public bool IsNumberField { get; set; }
        public string Text
        {
            get => PathTextbox.Text;
            set
            {
                PathTextbox.Text = value;
            }
        }
        public TextChangedEventHandler? TextChanged { get; set; }

        public PathTextedit()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += PathTextedit_Loaded;
            PathTextbox.PreviewTextInput += PathTextbox_PreviewTextInput;
        }

        private void PathTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (IsNumberField)
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
        }

        private void PathTextedit_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsFileSelect || IsFolderSelect)
            {
                ParentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new(50) });
                Button btn = new()
                {
                    Content = "...",
                    Margin = new(10)
                };
                btn.Click += Button_Click;
                ParentGrid.Children.Add(btn);
                Grid.SetColumn(btn, 2);
            }
        }

        public string GetText()
        {
            return PathTextbox.Text;
        }

        public void SetReadOnly(bool isReadOnly)
        {
            PathTextbox.IsReadOnly = isReadOnly;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsFileSelect)
            {
                OpenFileDialog fileDialog = new()
                {
                    Multiselect = false,
                    CheckFileExists = true
                };
                if (fileDialog.ShowDialog() == true)
                {
                    PathTextbox.Text = fileDialog.FileName;
                }
            }else if (IsFolderSelect)
            {
                OpenFolderDialog folderDialog = new()
                {
                    Multiselect = false
                };
                if (folderDialog.ShowDialog() == true)
                {
                    PathTextbox.Text = folderDialog.FolderName;
                }
            }
        }

        private void PathTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged?.Invoke(sender, e);
        }
    }
}
