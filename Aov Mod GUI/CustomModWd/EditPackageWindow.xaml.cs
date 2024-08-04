using AovClass;
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

namespace Aov_Mod_GUI.CustomModWd
{
    /// <summary>
    /// Interaction logic for EditPackageWindow.xaml
    /// </summary>
    public partial class EditPackageWindow : Window
    {
        PackageElement packageElement;
        bool ValueChanged = false;
        public PackageElement Result { get => packageElement; }

        public EditPackageWindow(PackageElement element, PackageElement? parent)
        {
            InitializeComponent();
            packageElement = element;
            Loaded += EditPackageWindow_Loaded;
            if (parent != null && parent._JtType.Equals("JTArr", StringComparison.CurrentCultureIgnoreCase))
            {
                NameField.SetReadOnly(true);
            }
        }

        private void EditPackageWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (packageElement.Value == null)
            {
                ValueField.SetReadOnly(true);
                ValueField.Opacity = 0.5;
            }
            else
            {
                ValueField.Text = packageElement._Value;
            }
            NameField.Text = packageElement._Name;
            JtTypeField.Text = packageElement._JtType;
            TypeField.Text = packageElement._Type;
            ValueField.TextChanged += (sender, e) => ValueChanged = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            packageElement._Name = NameField.Text;
            packageElement._JtType = JtTypeField.Text;
            packageElement._Type = TypeField.Text;
            if (ValueChanged && packageElement.Value != null)
            {
                packageElement._Value = ValueField.Text;
            }
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CancelButton_Click(new(), new());
            }
            else if (e.Key == Key.Enter)
            {
                AcceptButton_Click(new(), new());
            }
        }
    }
}
