using Aov_Mod_GUI.MainWindowControls;
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
using System.Xml;

namespace Aov_Mod_GUI.CustomModWd
{
    /// <summary>
    /// Interaction logic for EditXmlNodeWindow.xaml
    /// </summary>
    public partial class EditXmlNodeWindow : Window
    {
        XmlNode node;

        public EditXmlNodeWindow(XmlNode xmlNode)
        {
            InitializeComponent();
            node = xmlNode;

            KeyDown += EditXmlNodeWindow_KeyDown;
            Loaded += EditXmlNodeWindow_Loaded;
        }

        private void EditXmlNodeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NodeLabel.Content = node.Name;
            if (node.Attributes == null)
            {
                return;
            }
            foreach (XmlAttribute attr in node.Attributes)
            {
                PathTextedit textedit = new();
                textedit.PathLabel = attr.Name;
                textedit.Text = attr.Value;
                textedit.ToolTip = $"{textedit.PathLabel}: {textedit.Text}";
                AttributeFieldContainer.Children.Add(textedit);
            }
        }

        private void EditXmlNodeWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CancelButton_Click(new(), new());
            }
            else if (e.Key == Key.Enter)
            {
                ApplyButton_Click(new(), new());
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (node != null)
            {
                foreach (PathTextedit textedit in AttributeFieldContainer.Children)
                {
                    if (textedit.PathLabel == null || node.Attributes == null || node.GetAttribute(textedit.PathLabel) == null)
                    {
                        continue;
                    }
                    node.SetAttribute(textedit.PathLabel, textedit.Text);
                }
            }
            Close();
        }
    }
}
