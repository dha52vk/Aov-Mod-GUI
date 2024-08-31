using Aov_Mod_GUI.MainWindowControls;
using Aov_Mod_GUI.Models;
using AovClass;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
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
using System.Windows.Threading;
using System.Xml;

namespace Aov_Mod_GUI.CustomModWd
{
    /// <summary>
    /// Interaction logic for CusProjectXml.xaml
    /// </summary>
    public partial class CusProjectXml : Window
    {
        ModSources? modSources { get => MainWindow.GetModSources(); }
        ProjectPackage? projectPackage;
        public static List<ProjectItem>? ItemsCopied { get; set; }
        ObservableCollection<ProjectItem> itemSources = [];
        bool _IsReadOnly = false;
        readonly string? SavePackagePath = null;
        public bool IsReadOnly
        {
            get => _IsReadOnly;
            set
            {
                PasteBtn.IsEnabled = !value;
                InsertBtn.IsEnabled = !value;
                RemoveBtn.IsEnabled = !value;
                EditBtn.IsEnabled = !value;
                ReplaceBtn.IsEnabled = !value;
                ShowOtherInfoBtn.IsEnabled = !value;
                if (_IsReadOnly != value)
                {
                    if (value)
                    {
                        SaveActionBtn.Content = "Close";
                    }
                    else
                    {
                        SaveActionBtn.Content = "Save";
                    }
                }
                _IsReadOnly = value;
            }
        }

        TextChangedEventHandler? OtherCustomPathChanged;
        SelectionChangedEventHandler? OtherActionsComboboxChanged;

        public CusProjectXml(string? pkgPath = null)
        {
            InitializeComponent();
            if (pkgPath == null || !File.Exists(pkgPath))
            {
                OpenFileDialog openFileDialog = new() { CheckFileExists = true, Multiselect = true };
                if (openFileDialog.ShowDialog() == true)
                {
                    SavePackagePath = openFileDialog.FileName;
                }
                else
                {
                    Close();
                }
            }
            else
            {
                SavePackagePath = pkgPath;
            }
            Init();
        }

        public CusProjectXml(ProjectPackage package)
        {
            InitializeComponent();
            projectPackage = package;
            Init();
        }

        private void Init()
        {
            ContentRendered += CusProjectXml_ContentRendered;
            OtherCustomPathChanged = (sender, e) =>
            {
                OtherActionsCombobox.SelectionChanged -= OtherActionsComboboxChanged;
                OtherActionsCombobox.SelectedIndex = -1;
                OtherActionsCombobox.SelectionChanged += OtherActionsComboboxChanged;
            };
            OtherActionsComboboxChanged = (sender, e) =>
            {
                OtherCustomPath.TextChanged = null;
                OtherCustomPath.Text = string.Empty;
                OtherCustomPath.TextChanged = OtherCustomPathChanged;
            };

            OtherCustomPath.TextChanged = OtherCustomPathChanged;
            OtherActionsCombobox.SelectionChanged += OtherActionsComboboxChanged;

            SearchPage.Visibility = Visibility.Collapsed;
            ShowOtherPage.Visibility = Visibility.Collapsed;

            SaveActionBtn.Click += SaveActionBtn_Click;
            AddFilterFieldButton.Click += AddFilterFieldButton_Click;
            AddTagNameFilterButton.Click += AddTagNameFilterButton_Click;
            AddAttributeFilterButton.Click += AddAttributeFilterButton_Click;
            ClearFilterFieldButton.Click += ClearFilterFieldButton_Click;
            InsertTemplateBtn.Click += InsertTemplateBtn_Click;
            AddFilterFieldPopup.MouseLeave += (sender, e) =>
            {
                AddFilterFieldPopup.IsOpen = false;
            };
        }

        private void InsertTemplateBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearFilterFieldButton_Click(object sender, RoutedEventArgs e)
        {
            FilterFieldContainer.Children.Clear();
        }

        private void AddAttributeFilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterField filterField = new() { FilterLabel = "Attribute: ", IsAttributeField = true };
            filterField.RemoveClick = () => { FilterFieldContainer.Children.Remove(filterField); };
            FilterFieldContainer.Children.Add(filterField);
        }

        private void AddTagNameFilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterField filterField = new() { FilterLabel = "Tag Name: " };
            filterField.RemoveClick = () => { FilterFieldContainer.Children.Remove(filterField); };
            FilterFieldContainer.Children.Add(filterField);
        }

        private void AddFilterFieldButton_Click(object sender, RoutedEventArgs e)
        {
            AddFilterFieldPopup.IsOpen = !AddFilterFieldPopup.IsOpen;
        }

        private void CusProjectXml_ContentRendered(object? sender, EventArgs e)
        {
            ProgressWindow progressWd = new() { Owner = this, IsIndeterminate = true };
            progressWd.Execute(() =>
            {
                ReloadSources();
            });
            progressWd.ShowDialog();
        }

        public void ReloadSources()
        {
            if (!string.IsNullOrEmpty(SavePackagePath))
            {
                try
                {
                    projectPackage = new ProjectPackage(SavePackagePath);
                }
                catch
                {
                    MessageBox.Show("File không phải là aov infos file!!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            if (projectPackage == null)
            {
                return;
            }
            itemSources = [];
            foreach (var pair in projectPackage.Projects)
            {
                itemSources.Add(new(pair.Value, pair.Key));
            }
            Dispatcher.Invoke(() =>
            {
                this.Title = projectPackage.PackageTitle;
                ProjectTreeView.ItemsSource = itemSources;

                if (modSources != null)
                {
                    OtherActionsCombobox.ItemsSource = Directory.GetFiles(modSources.ActionsParentPath)
                        .Select((filePath) => Path.GetFileName(filePath))
                        .Where((filename) => filename.StartsWith("Actor", StringComparison.CurrentCultureIgnoreCase));
                }
            });
        }

        private void SaveActionBtn_Click(object sender, RoutedEventArgs e)
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
                ProgressWindow progressWd = new() { Owner = this };
                progressWd.Execute(() => projectPackage?.SaveTo(SavePackagePath));
                progressWd.ShowDialog();
            }
            MessageBox.Show("Saved");
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
                else if (Keyboard.IsKeyDown(Key.F))
                {
                    OpenSearchBtn_Click(new(), new());
                }
                else if (Keyboard.IsKeyDown(Key.R))
                {
                    ReplaceBtn_Click(new(), new());
                }
                else if (Keyboard.IsKeyDown(Key.S))
                {
                    SaveActionBtn_Click(new(), new());
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
            List<ProjectItem>? selectedItems = ProjectTreeView.SelectedItems.Cast<ProjectItem>().ToList();
            if (ProjectTreeView.SelectedItem == null || ProjectTreeView.SelectedItem is not ProjectItem)
                return;
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            ItemsCopied = selectedItems.Select(item => item.Clone()).Where(item => item != null).ToList();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

        private void CopyAllChildItem(object sender, RoutedEventArgs e)
        {
            List<ProjectItem>? selectedItems = ProjectTreeView.SelectedItems.Cast<ProjectItem>().ToList();
            if (ProjectTreeView.SelectedItem == null || selectedItems == null || selectedItems.Count > 1)
                return;
            ItemsCopied = (ProjectTreeView.SelectedItem as ProjectItem)?.Children.ToList();
        }

        private void PasteItemAsChild(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly)
                return;
            List<ProjectItem>? selectedItems = ProjectTreeView.SelectedItems.Cast<ProjectItem>().ToList();
            if (ProjectTreeView.SelectedItem is not ProjectItem selectedItem || ItemsCopied == null || selectedItems.Count > 1)
                return;
            foreach (var item in ItemsCopied)
            {
                if (item.node == null)
                {
                    continue;
                }
                selectedItem.AddChild(new(item.node.CloneNode(true), selectedItem));
            }
        }

        private void InsertItemBeforeSelected(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly)
                return;
            List<ProjectItem>? selectedItems = ProjectTreeView.SelectedItems.Cast<ProjectItem>().ToList();
            if (ProjectTreeView.SelectedItem is not ProjectItem selectedItem || ItemsCopied == null || selectedItem.Parent == null || selectedItems.Count > 1)
                return;
            foreach (var item in ItemsCopied)
            {
                if (item.node == null)
                {
                    continue;
                }
                selectedItem.Parent.InsertChildBefore(new(item.node, selectedItem.Parent), selectedItem);
            }
        }

        private void ReplaceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly)
                return;
            List<ProjectItem>? selectedItems = ProjectTreeView.SelectedItems.Cast<ProjectItem>().ToList();
            if (selectedItems == null || selectedItems.Count != 1 || ItemsCopied == null || ItemsCopied.Count < 1)
                return;
            var selectedItem = selectedItems[0];
            foreach (ProjectItem item in ItemsCopied)
            {
                ProjectItem? clone = item.Clone();
                if (clone != null)
                {
                    selectedItem.Parent?.InsertChildBefore(clone, selectedItem);
                }
            }
            selectedItem.Parent?.RemoveChild(selectedItem);
        }

        private void RemoveItem(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly)
                return;
            List<ProjectItem>? selectedItems = ProjectTreeView.SelectedItems.Cast<ProjectItem>().ToList();
            if (ProjectTreeView.SelectedItem is not ProjectItem || selectedItems.Count <= 0)
                return;
            foreach (ProjectItem selectedItem in selectedItems)
                selectedItem?.Parent?.RemoveChild(selectedItem);
        }

        private void SaveInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            List<ProjectItem>? selectedItems = ProjectTreeView.SelectedItems.Cast<ProjectItem>().ToList();
            if (ProjectTreeView.SelectedItem is not ProjectItem selectedItem || selectedItems.Count > 1 || selectedItem.node == null || selectedItem.IsRoot)
                return;

            EditXmlNodeWindow xmlNodeWindow = new(selectedItem.node) { Owner = this };
            xmlNodeWindow.ShowDialog();
            selectedItem.NotifyPropertyChanged("Name");
            selectedItem.NotifyPropertyChanged("Info1");
            selectedItem.NotifyPropertyChanged("Info2");
            selectedItem.NotifyPropertyChanged("Info3");
            selectedItem.NotifyPropertyChanged("ToolTip");
        }

        private void OpenSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchPage.Visibility = Visibility.Visible;
            ShowOtherPage.Visibility = Visibility.Collapsed;
        }

        private void ShowOtherInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowOtherPage.Visibility = Visibility.Visible;
            SearchPage.Visibility = Visibility.Collapsed;
        }

        private void BackgroundSearch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchPage.Visibility = Visibility.Collapsed;
        }

        private void ShowOtherBackground_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowOtherPage.Visibility = Visibility.Collapsed;
        }

        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            List<FilterField> filterFields = FilterFieldContainer.Children.Cast<FilterField>().ToList();
            List<Func<ProjectItem, bool>> Filters = [];
            foreach (var filterField in filterFields)
            {
                Func<ProjectItem, bool> filter;
                if (filterField.IsAttributeField)
                    filter = (item) => item.node?.GetAttribute(filterField.AttributeName)?.Contains(filterField.Value, StringComparison.CurrentCultureIgnoreCase) ?? false;
                else
                    filter = (item) => item.node?.Name.Equals(filterField.Value, StringComparison.CurrentCultureIgnoreCase) ?? false;
                Filters.Add(filter);
            }
            foreach (var item in itemSources)
            {
                ApplyFilter(item, (item) => Filters.Count == 0 || Filters.FindIndex((filter) => filter(item)) > -1);
            }
            SearchPage.Visibility = Visibility.Collapsed;
        }

        public static bool ApplyFilter(ProjectItem item, Func<ProjectItem, bool> filter)
        {
            bool match = filter(item);
            foreach (var child in item.Children)
            {
                if (ApplyFilter(child, filter))
                {
                    match = true;
                }
            }
            item.MatchFilter = match;
            item.NotifyPropertyChanged("MatchFilter");
            return match;
        }

        private void ShowOtherBtn_Click(object sender, RoutedEventArgs e)
        {
            string? value = OtherActionsCombobox.SelectedValue?.ToString();
            string? value2 = OtherCustomPath.Text;
            if (modSources == null || (value == null && string.IsNullOrEmpty(value2)))
                return;
            //ProjectPackage pkg = new(value != null ? Path.Combine(modSources.ActionsParentPath, value) : value2);
            CusProjectXml CusActions = new(value != null ? Path.Combine(modSources.ActionsParentPath, value) : value2)
            {
                IsReadOnly = true
            };
            CusActions.Show();
        }
    }

    public class ProjectItem : INotifyPropertyChanged
    {
        public ProjectItem? Parent { get; private set; }
        public ActionsXml? Root;
        public XmlNode? node;
        private string? _Name = null;
        public bool HasToolTip { get => ToolTip != ""; }
        public bool IsRootOrTrack { get => IsRoot || node?.Name == "Track"; }
        public bool MatchFilter { get; set; } = true;
        public bool IsRoot { get; private set; }
        public bool HasChild { get => Children != null && Children.Count > 0; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get
            {
                if (node != null && string.IsNullOrEmpty(_Name))
                {
                    if (node.Name == "Track")
                    {
                        return node.GetAttribute("trackName") ?? "";
                    }
                    else if (node.Name == "Event")
                    {
                        return node.GetAttribute("eventName") ?? "";
                    }
                    else if (node.Name == "Condition")
                    {
                        return "Condition";
                    }
                    else
                    {
                        return node.GetAttribute("name") ?? node.Name ?? "";
                    }
                }
                else
                {
                    return _Name ?? "";
                }
            }
            //string.IsNullOrEmpty(_Name) ? node?.GetAttribute("trackName")
            //    ?? node?.GetAttribute("eventName")
            //    ?? node?.GetAttribute("name")
            //    ??"" : _Name; 
            set => _Name = value;
        }
        public string Info1
        {
            get
            {
                if (node?.Name == "Track")
                {
                    return node.GetAttribute("guid") ?? "";
                }
                else if (node?.Name == "Event")
                {
                    return node.GetAttribute("time") ?? "";
                }
                else if (node?.Name == "Condition")
                {
                    return node.GetAttribute("guid") ?? "";
                }
                else if (node?.Name == "TemplateObject")
                {
                    return node.GetAttribute("objectName") ?? "";
                }
                else if (node?.Name == "Vector3i" || node?.Name == "Vector3" || node?.Name == "EulerAngle")
                {
                    return $"x: {node.GetAttribute("x")}, y: {node.GetAttribute("y")}, z: {node.GetAttribute("z")}";
                }
                else if (node?.Name == "TrackObject")
                {
                    return node.GetAttribute("guid") ?? "";
                }
                else
                {
                    return node?.GetAttribute("value") ?? "";
                }
            }
            //get => node?.GetAttribute("eventType") 
            //    ?? node?.GetAttribute("value")
            //    ?? "value"; 
        }
        public string Info2
        {
            get
            {
                if (node?.Name == "Track")
                {
                    return node.GetAttribute("enabled") == "false" ? "disable" : "";
                }
                else if (node?.Name == "Event")
                {
                    return node.GetAttribute("isDuration") == "true" ? "Is Duration" : "";
                }
                else
                {
                    return node?.GetAttribute("useRefParam") == "true" ? "UseRefParam" : "";
                }
            }
            //get => node?.GetAttribute("enabled") 
            //    ?? ""; 
        }
        public string Info3
        {
            get
            {
                if (node?.Name == "Track")
                {
                    return node.GetAttribute("none") == "false" ? "disable" : "";
                }
                else if (node?.Name == "Event")
                {
                    return node?.GetChildrenByName("Condition")?.Count.ToString() ?? "";
                }
                else
                {
                    return node?.Name ?? node?.GetAttribute("refParamName") ?? "";
                }
            }
            //get => node?.Name == "Event" ? node?.GetChildrenByName("Condition")?.Count.ToString() ?? "" :
            //    (node?.GetAttribute("useRefParam") ?? "");
        }
        public string ToolTip
        {
            get
            {
                if (node?.Name == "Condition" || node?.Name == "TrackObject")
                {
                    XmlNode? Track = Root?.GetActionNodes()?.Find((track) => track.GetAttribute("guid") == node.GetAttribute("guid"));
                    XmlNodeList? list = Track?.GetChildrenByName("Event")?[0].ChildNodes;
                    string parameters = "";
                    if (list != null)
                    {
                        //foreach (XmlNode param in list)
                        //{
                        //    parameters += (param.GetAttribute("name") ?? "") + ": " + (param.GetAttribute("value") ?? "") + ", ";
                        //}
                        parameters = string.Join(", ", list.Cast<XmlNode>().Select(param => (param.GetAttribute("name") ?? "") + ": " + (param.GetAttribute("value") ?? "")));
                    }
                    return (node?.Name == "TrackObject" ? "" : node?.GetAttribute("status") + ": ") + (Track?.GetAttribute("eventType") + " - " ?? "") + parameters;
                }
                else
                    return "";
            }
        }
        public ObservableCollection<ProjectItem> Children { get; private set; }

        public ProjectItem(ActionsXml actions, string name)
        {
            Root = actions;
            _Name = name;
            IsRoot = true;
            node = null;
            Children = [];
            List<XmlNode> child = actions.GetActionNodes() ?? [];
            foreach (XmlNode node in child)
            {
                Children.Add(new(node, this));
            }
        }

        public ProjectItem(XmlNode _node, ProjectItem? parent)
        {
            node = _node;
            IsRoot = false;
            Root = parent?.Root;
            Children = [];
            Parent = parent;
            if (_node != null && _node.ChildNodes != null)
            {
                for (int i = 0; i < _node.ChildNodes.Count; i++)
                {
                    XmlNode? node = _node.ChildNodes[i];
                    if (node != null)
                    {
                        ProjectItem action = new(node, this);
                        Children.Add(action);
                    }
                }
            }
        }

        public void InsertChildBefore(ProjectItem newChild, ProjectItem refChild)
        {
            if (newChild.node == null || refChild.node == null)
                return;
            int index = Children.ToList().FindIndex((child) => child == refChild);
            if (index < 0)
                return;
            newChild.Parent = this;
            XmlNode? newNode = node?.OwnerDocument?.ImportNode(newChild.node, true) ?? Root?.document.ImportNode(newChild.node, true);
            if (newNode == null) return;
            newChild.node = newNode;
            Children.Insert(index, newChild);
            if (IsRoot)
            {
                if (Root == null)
                    return;
                Root.InsertActionNode(index, newNode);
            }
            else
            {
                if (node == null)
                    return;
                node.InsertBefore(newNode, refChild.node);
            }
        }

        public void AddChild(ProjectItem newChild)
        {
            if (newChild.node == null)
                return;
            newChild.Parent = this;
            XmlNode? newNode = node?.OwnerDocument?.ImportNode(newChild.node, true) ?? Root?.document.ImportNode(newChild.node, true);
            if (newNode == null) return;
            newChild.node = newNode;
            Children.Add(newChild);
            if (IsRoot)
            {
                if (Root == null)
                    return;
                Root.AppendActionNode(newChild.node);
            }
            else
            {
                if (node == null)
                    return;
                node.AppendChild(newChild.node);
            }
        }

        public void RemoveChild(ProjectItem child)
        {
            if (child.node == null)
                return;
            Children.Remove(child);
            if (IsRoot)
            {
                child.node.ParentNode?.RemoveChild(child.node);
            }
            else
            {
                node?.RemoveChild(child.node);
            }
        }

        public ProjectItem? Clone()
        {
            if (!IsRoot && node != null)
                return new ProjectItem(node.Clone(), null);
            else
                return null;
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
