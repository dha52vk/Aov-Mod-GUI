using Aov_Mod_GUI.Models;
using AovClass;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aov_Mod_GUI.CustomModWd
{
    /// <summary>
    /// Interaction logic for CusInfos.xaml
    /// </summary>
    public partial class CusInfos : Window
    {
        ModSources? modSources { get => MainWindow.GetModSources(); }
        readonly InfosPackage infosPackage;
        public static List<InfoItem>? ItemsCopied { get; set; }
        ObservableCollection<InfoItem> itemSources = [];
        bool _IsReadOnly = false;
        string? SavePackagePath = null;
        public bool IsReadOnly
        {
            get => _IsReadOnly;
            set
            {
                PasteBtn.IsEnabled = !value;
                InsertBtn.IsEnabled = !value;
                RemoveBtn.IsEnabled = !value;
                EditBtn.IsEnabled = !value;
                ClearFilterBtn.IsEnabled = !value;
                ShowOtherInfoBtn.IsEnabled = !value;
                if (_IsReadOnly != value)
                {
                    if (value)
                    {
                        SaveInfoBtn.Content = "Close";
                    }
                    else
                    {
                        SaveInfoBtn.Content = "Save";
                    }
                }
                _IsReadOnly = value;
            }
        }

        public CusInfos()
        {
            InitializeComponent();
            OpenFileDialog openFileDialog = new() { CheckFileExists = true, Multiselect = true };
            if (openFileDialog.ShowDialog() == true)
            {
                SavePackagePath = openFileDialog.FileName;
                try
                {
                    if (FileExtension.IsZipValid(SavePackagePath))
                    {
                        infosPackage = new InfosPackage(SavePackagePath);
                    }
                    else
                    {
                        infosPackage = new(Path.GetFileName(SavePackagePath),
                            PackageSerializer.Deserialize(DHAExtensions.GetAovBytesFrom(SavePackagePath)));
                    }
                }
                catch
                {
                    MessageBox.Show("File không phải là aov infos file!!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
                Init();
            }
            else
            {
                Close();
            }
        }

        public CusInfos(InfosPackage infosPackage)
        {
            InitializeComponent();
            this.infosPackage = infosPackage;
            Init();
        }

        public void Init()
        {
            SearchPage.Visibility = Visibility.Collapsed;
            ShowOtherPage.Visibility = Visibility.Collapsed;
            ClearFilterBtn.Visibility = Visibility.Collapsed;

            this.Title = infosPackage.PackageTitle;
            ItemsCopied = [];
            ReloadSources();
        }

        public void ReloadSources()
        {
            itemSources = [];
            foreach (var pair in infosPackage.Elements)
            {
                itemSources.Add(new(pair.Value, null) { Name = pair.Key });
            }
            InfosTreeView.ItemsSource = itemSources;

            if (modSources != null)
            {
                OtherInfosCombobox.ItemsSource = Directory.GetFiles(modSources.InfosParentPath)
                    .Select((filePath) => Path.GetFileName(filePath))
                    .Where((filename) => filename.StartsWith("Actor", StringComparison.CurrentCultureIgnoreCase));
            }
        }

        private void TreeView_KeyDown(object sender, KeyEventArgs e)
        {
            bool CtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool AltDown = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            bool ShiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            if (CtrlDown)
            {
                if (Keyboard.IsKeyDown(Key.C))
                {
                    if (AltDown)
                    {
                        CopyAllChildItem(new(), new());
                    }
                    else
                    {
                        CopyItem(new(), new());
                    }
                }
                else if (Keyboard.IsKeyDown(Key.V))
                {
                    PasteItemAsChild(new(), new());
                }
                else if (Keyboard.IsKeyDown(Key.I))
                {
                    InsertItemBeforeSelected(new(), new());
                }
                else if (Keyboard.IsKeyDown(Key.R))
                {
                    ReplaceBtn_Click(new(), new());
                }
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.Delete))
                {
                    RemoveItem(new(), new());
                }
                if (Keyboard.IsKeyDown(Key.Enter))
                {
                    EditBtn_Click(new(), new());
                }
            }
        }

        private void CopyItem(object sender, RoutedEventArgs e)
        {
            List<InfoItem>? selectedItems = InfosTreeView.SelectedItems.Cast<InfoItem>().ToList();
            if (InfosTreeView.SelectedItem == null || !(InfosTreeView.SelectedItem is InfoItem item))
                return;
            ItemsCopied = selectedItems ?? [];
        }

        private void CopyAllChildItem(object sender, RoutedEventArgs e)
        {
            if (InfosTreeView.SelectedItem == null)
                return;
            ItemsCopied = (InfosTreeView.SelectedItem as InfoItem)?.Children.ToList();
        }

        private void PasteItemAsChild(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly)
                return;
            InfoItem? selectedItem = InfosTreeView.SelectedItem as InfoItem;
            if (selectedItem == null || ItemsCopied == null)
                return;
            foreach (var item in ItemsCopied)
            {
                selectedItem.AddChild(new(item.infoElement.Clone(), selectedItem));
            }
        }

        private void InsertItemBeforeSelected(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly)
                return;
            InfoItem? selectedItem = InfosTreeView.SelectedItem as InfoItem;
            if (selectedItem == null || ItemsCopied == null || selectedItem.Parent == null)
                return;
            foreach (var item in ItemsCopied)
            {
                selectedItem.Parent.InsertChildBefore(new(item.infoElement, selectedItem.Parent), selectedItem);
            }
        }

        private void ReplaceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly)
                return;
            InfoItem? selectedItem = InfosTreeView.SelectedItem as InfoItem;
            if (selectedItem == null || ItemsCopied == null || selectedItem.Parent == null)
                return;
            foreach (var item in ItemsCopied)
            {
                selectedItem.Parent.InsertChildBefore(new(item.infoElement, selectedItem.Parent), selectedItem);
            }
            selectedItem.Parent.RemoveChild(selectedItem);
        }

        private void RemoveItem(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly)
                return;
            InfoItem[] selectedItems = InfosTreeView.SelectedItems.Cast<InfoItem>().ToArray();
            InfoItem? selectedItem = InfosTreeView.SelectedItem as InfoItem;
            if (selectedItem == null || selectedItem.Parent == null)
                return;
            foreach (InfoItem item in selectedItems)
            {
                selectedItem.Parent.RemoveChild(item);
            }
        }

        private void SaveInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly)
            {
                Close();
                return;
            }

            if (string.IsNullOrEmpty(SavePackagePath))
            {
                Close();
            }
            else
            {
                infosPackage.SaveTo(SavePackagePath);
            }
            MessageBox.Show("Saved");
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            InfoItem? selectedItem = InfosTreeView.SelectedItem as InfoItem;
            if (selectedItem == null)
                return;
            EditPackageWindow editWindow = new(selectedItem.infoElement, selectedItem.Parent?.infoElement) { Owner = this };
            editWindow.ShowDialog();
            selectedItem.infoElement = editWindow.Result;
            selectedItem.NotifyPropertyChanged("Name");
            selectedItem.NotifyPropertyChanged("infoElement");
            selectedItem.NotifyPropertyChanged("JtType");
            selectedItem.NotifyPropertyChanged("Value");
            //ReloadSources();
        }

        private void OpenSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchPage.Visibility = Visibility.Visible;
        }

        private void BackgroundSearch_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SearchPage.Visibility = Visibility.Collapsed;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchField.Text))
            {
                ClearFilterBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                ClearFilterBtn.Visibility = Visibility.Visible;
            }
            Func<InfoItem, bool> filterName = (infoItem) => infoItem.Name.Contains(SearchField.Text, StringComparison.CurrentCultureIgnoreCase);
            Func<InfoItem, bool> filterType = (infoItem) => infoItem.infoElement._Type.Contains(SearchField.Text, StringComparison.CurrentCultureIgnoreCase);
            Func<InfoItem, bool> filterValue = (infoItem) => infoItem.infoElement._Value.Contains(SearchField.Text, StringComparison.CurrentCultureIgnoreCase);
            Func<InfoItem, bool> filter = (infoItem) => (SearchInNameCheckbox.IsChecked ?? false ? filterName(infoItem) : false)
                                                        || (SearchInTypeCheckbox.IsChecked ?? false ? filterType(infoItem) : false)
                                                        || (SearchInValueCheckbox.IsChecked ?? false ? filterValue(infoItem) : false);
            foreach (var item in itemSources)
            {
                Search(item, filter);
            }

            InfosTreeView.ItemsSource = itemSources;
            SearchPage.Visibility = Visibility.Collapsed;
        }

        public bool Search(InfoItem item, Func<InfoItem, bool> filter)
        {
            bool match = filter(item);
            foreach (var child in item.Children)
            {
                if (Search(child, filter))
                {
                    match = true;
                }
            }
            item.MatchFilter = match;
            item.NotifyPropertyChanged("MatchFilter");
            return match;
        }

        private void ClearFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in itemSources)
            {
                Search(item, (info) => true);
            }
            ClearFilterBtn.Visibility = Visibility.Collapsed;
        }

        private void ShowOtherInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowOtherPage.Visibility = Visibility.Visible;
        }

        private void ShowOtherBackground_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ShowOtherPage.Visibility = Visibility.Collapsed;
        }

        private void ShowOtherBtn_Click(object sender, RoutedEventArgs e)
        {
            string? value = OtherInfosCombobox.SelectedValue.ToString();
            if (modSources == null || value == null)
                return;
            InfosPackage pkg = new InfosPackage(Path.Combine(modSources.InfosParentPath, value));
            CusInfos cusInfos = new(pkg);
            cusInfos.IsReadOnly = true;
            cusInfos.Show();
        }
    }

    public class InfoItem : INotifyPropertyChanged
    {
        public readonly InfoItem? Parent;
        public PackageElement infoElement;
        private string? _Name = null;
        //public Func<InfoItem, bool>? Filter = null;
        public bool MatchFilter { get; set; } = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get => string.IsNullOrEmpty(_Name) ? infoElement._Name : _Name; set => _Name = value; }
        public string Value { get => infoElement._Value; }
        public string JtType { get => infoElement._JtType; }
        public int ChildCount { get => infoElement.Children?.Count ?? 0; }
        public ObservableCollection<InfoItem> Children { get; private set; }

        public InfoItem(PackageElement element, InfoItem? parent)
        {
            infoElement = element;
            Children = [];
            Parent = parent;
            if (element.Children != null)
            {
                for (int i = 0; i < element.Children.Count; i++)
                {
                    InfoItem info = new(element.Children[i], this);
                    if (element._JtType.Equals("JTArr", StringComparison.CurrentCultureIgnoreCase))
                    {
                        info.Name = "Element " + i;
                    }
                    Children.Add(info);
                }
            }
        }

        public void RemoveChild(InfoItem info)
        {
            Children.Remove(info);
            infoElement.Children?.Remove(info.infoElement);
        }

        public void InsertChildBefore(InfoItem newChild, InfoItem refChild)
        {
            InfoItem? item = Children.Where((child) => child.infoElement._Name.
                        Equals(newChild.infoElement._Name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            bool isChildOfJtArr = ((item?.infoElement._Name?.Equals("Element") ?? false) && infoElement._JtType == "JTArr");
            if (item != null && !isChildOfJtArr)
            {
                RemoveChild(item);
            }
            int index = Children.ToList().FindIndex((child) => child == refChild);
            if (index < 0)
            {
                return;
            }
            if (isChildOfJtArr)
                newChild.Name = refChild.Name;
            Children.Insert(index, newChild);
            infoElement.InsertChild(index, newChild.infoElement);
        }

        public void AddChild(InfoItem newChild)
        {
            InfoItem? item = Children.Where((child) => child.infoElement._Name.
                        Equals(newChild.infoElement._Name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (item != null || ((item?._Name?.Equals("Element") ?? false) && infoElement._JtType == "JTArr"))
            {
                RemoveChild(item);
            }
            Children.Add(newChild);
            infoElement.AddChild(newChild.infoElement);
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
