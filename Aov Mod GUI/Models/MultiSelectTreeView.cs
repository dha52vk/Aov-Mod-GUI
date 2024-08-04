using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Diagnostics;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace Aov_Mod_GUI.Models
{
    public sealed class MultiSelectTreeView : TreeView
    {
        #region Fields

        // Used in shift selections
        private TreeViewItem _lastItemSelected;

        #endregion Fields
        #region Dependency Properties

        public static readonly DependencyProperty IsItemSelectedProperty =
            DependencyProperty.RegisterAttached("IsItemSelected", typeof(bool), typeof(MultiSelectTreeView));

        public static void SetIsItemSelected(UIElement element, bool value)
        {
            element.SetValue(IsItemSelectedProperty, value);
        }
        public static bool GetIsItemSelected(UIElement element)
        {
            return (bool)element.GetValue(IsItemSelectedProperty);
        }

        #endregion Dependency Properties
        #region Properties

        private static bool IsCtrlPressed
        {
            get { return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl); }
        }
        private static bool IsShiftPressed
        {
            get { return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift); }
        }

        public IList SelectedItems
        {
            get
            {
                var selectedTreeViewItems = GetTreeViewItems(this, true).Where(GetIsItemSelected);
                var selectedModelItems = selectedTreeViewItems.Select(treeViewItem => treeViewItem.Header);

                return selectedModelItems.ToList();
            }
        }

        #endregion Properties
        #region Event Handlers

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            // If clicking on a tree branch expander...
            //if (e.OriginalSource is Shape || e.OriginalSource is Grid || e.OriginalSource is Border)
            //    return;

            var item = GetTreeViewItemClicked((FrameworkElement)e.OriginalSource);
            if (item != null) SelectedItemChangedInternal(item);
        }

        #endregion Event Handlers
        #region Utility Methods

        private void SelectedItemChangedInternal(TreeViewItem tvItem)
        {
            // Clear all previous selected item states if ctrl is NOT being held down
            if (!IsCtrlPressed)
            {
                var items = GetTreeViewItems(this, true);
                foreach (var treeViewItem in items)
                    SetIsItemSelected(treeViewItem, false);
            }

            // Is this an item range selection?
            if (IsShiftPressed && _lastItemSelected != null)
            {
                var items = GetTreeViewItemRange(_lastItemSelected, tvItem);
                if (items.Count > 0)
                {
                    foreach (var treeViewItem in items)
                        SetIsItemSelected(treeViewItem, true);

                    _lastItemSelected = items.Last();
                }
            }
            // Otherwise, individual selection
            else
            {
                SetIsItemSelected(tvItem, true);
                _lastItemSelected = tvItem;
            }
        }
        private static TreeViewItem? GetTreeViewItemClicked(DependencyObject sender)
        {
            while (sender != null && !(sender is TreeViewItem))
                sender = VisualTreeHelper.GetParent(sender);
            return sender as TreeViewItem;
        }
        private static List<TreeViewItem> GetTreeViewItems(ItemsControl parentItem, bool includeCollapsedItems, List<TreeViewItem> itemList = null)
        {
            if (itemList == null)
                itemList = new List<TreeViewItem>();

            for (var index = 0; index < parentItem.Items.Count; index++)
            {
                var tvItem = parentItem.ItemContainerGenerator.ContainerFromIndex(index) as TreeViewItem;
                if (tvItem == null) continue;

                itemList.Add(tvItem);
                if (includeCollapsedItems || tvItem.IsExpanded)
                    GetTreeViewItems(tvItem, includeCollapsedItems, itemList);
            }
            return itemList;
        }
        private List<TreeViewItem> GetTreeViewItemRange(TreeViewItem start, TreeViewItem end)
        {
            var items = GetTreeViewItems(this, false);

            var startIndex = items.IndexOf(start);
            var endIndex = items.IndexOf(end);
            var rangeStart = startIndex > endIndex || startIndex == -1 ? endIndex : startIndex;
            var rangeCount = startIndex > endIndex ? startIndex - endIndex + 1 : endIndex - startIndex + 1;

            if (startIndex == -1 && endIndex == -1)
                rangeCount = 0;

            else if (startIndex == -1 || endIndex == -1)
                rangeCount = 1;

            return rangeCount > 0 ? items.GetRange(rangeStart, rangeCount) : new List<TreeViewItem>();
        }

        #endregion Utility Methods
    }
    //public class MultiSelectTreeView : TreeView
    //{
    //    public MultiSelectTreeView()
    //    {
    //        GotFocus += OnTreeViewItemGotFocus;
    //        PreviewMouseLeftButtonDown += OnTreeViewItemPreviewMouseDown;
    //        PreviewMouseLeftButtonUp += OnTreeViewItemPreviewMouseUp;
    //    }

    //    private static TreeViewItem _selectTreeViewItemOnMouseUp;


    //    public static readonly DependencyProperty IsItemSelectedProperty = DependencyProperty.RegisterAttached("IsItemSelected", typeof(Boolean), typeof(MultiSelectTreeView), new PropertyMetadata(false, OnIsItemSelectedPropertyChanged));

    //    public static bool GetIsItemSelected(TreeViewItem element)
    //    {
    //        return (bool)element.GetValue(IsItemSelectedProperty);
    //    }

    //    public static void SetIsItemSelected(TreeViewItem element, Boolean value)
    //    {
    //        if (element == null) return;

    //        element.SetValue(IsItemSelectedProperty, value);
    //    }

    //    private static void OnIsItemSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        var treeViewItem = d as TreeViewItem;
    //        var treeView = FindTreeView(treeViewItem);
    //        if (treeViewItem != null && treeView != null)
    //        {
    //            var selectedItems = GetSelectedItems(treeView);
    //            if (selectedItems != null)
    //            {
    //                if (GetIsItemSelected(treeViewItem))
    //                {
    //                    selectedItems.Add(treeViewItem.Header);
    //                }
    //                else
    //                {
    //                    selectedItems.Remove(treeViewItem.Header);
    //                }
    //            }
    //        }
    //    }

    //    public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.RegisterAttached("SelectedItems", typeof(IList), typeof(MultiSelectTreeView));

    //    public static IList GetSelectedItems(TreeView element)
    //    {
    //        return (IList)element.GetValue(SelectedItemsProperty);
    //    }

    //    public static void SetSelectedItems(TreeView element, IList value)
    //    {
    //        element.SetValue(SelectedItemsProperty, value);
    //    }

    //    private static readonly DependencyProperty StartItemProperty = DependencyProperty.RegisterAttached("StartItem", typeof(TreeViewItem), typeof(MultiSelectTreeView));


    //    private static TreeViewItem GetStartItem(TreeView element)
    //    {
    //        return (TreeViewItem)element.GetValue(StartItemProperty);
    //    }

    //    private static void SetStartItem(TreeView element, TreeViewItem value)
    //    {
    //        element.SetValue(StartItemProperty, value);
    //    }


    //    private static void OnTreeViewItemGotFocus(object sender, RoutedEventArgs e)
    //    {
    //        _selectTreeViewItemOnMouseUp = null;

    //        if (e.OriginalSource is TreeView) return;

    //        var treeViewItem = FindTreeViewItem(e.OriginalSource as DependencyObject);
    //        if (Mouse.LeftButton == MouseButtonState.Pressed && GetIsItemSelected(treeViewItem) && Keyboard.Modifiers != ModifierKeys.Control)
    //        {
    //            _selectTreeViewItemOnMouseUp = treeViewItem;
    //            return;
    //        }

    //        SelectItems(treeViewItem, sender as TreeView);
    //    }

    //    private static void SelectItems(TreeViewItem treeViewItem, TreeView treeView)
    //    {
    //        if (treeViewItem != null && treeView != null)
    //        {
    //            if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
    //            {
    //                SelectMultipleItemsContinuously(treeView, treeViewItem, true);
    //            }
    //            else if (Keyboard.Modifiers == ModifierKeys.Control)
    //            {
    //                SelectMultipleItemsRandomly(treeView, treeViewItem);
    //            }
    //            else if (Keyboard.Modifiers == ModifierKeys.Shift)
    //            {
    //                SelectMultipleItemsContinuously(treeView, treeViewItem);
    //            }
    //            else
    //            {
    //                SelectSingleItem(treeView, treeViewItem);
    //            }
    //        }
    //    }

    //    private static void OnTreeViewItemPreviewMouseDown(object sender, MouseEventArgs e)
    //    {
    //        var treeViewItem = FindTreeViewItem(e.OriginalSource as DependencyObject);

    //        if (treeViewItem != null && treeViewItem.IsFocused)
    //            OnTreeViewItemGotFocus(sender, e);
    //    }

    //    private static void OnTreeViewItemPreviewMouseUp(object sender, MouseButtonEventArgs e)
    //    {
    //        var treeViewItem = FindTreeViewItem(e.OriginalSource as DependencyObject);

    //        if (treeViewItem == _selectTreeViewItemOnMouseUp)
    //        {
    //            SelectItems(treeViewItem, sender as TreeView);
    //        }
    //    }

    //    private static TreeViewItem? FindTreeViewItem(DependencyObject dependencyObject)
    //    {
    //        if (!(dependencyObject is Visual || dependencyObject is Visual3D))
    //            return null;

    //        var treeViewItem = dependencyObject as TreeViewItem;
    //        if (treeViewItem != null)
    //        {
    //            return treeViewItem;
    //        }

    //        return FindTreeViewItem(VisualTreeHelper.GetParent(dependencyObject));
    //    }

    //    private static void SelectSingleItem(TreeView treeView, TreeViewItem treeViewItem)
    //    {
    //        // first deselect all items
    //        DeSelectAllItems(treeView, null);
    //        SetIsItemSelected(treeViewItem, true);
    //        SetStartItem(treeView, treeViewItem);
    //    }

    //    private static void DeSelectAllItems(TreeView treeView, TreeViewItem treeViewItem)
    //    {
    //        if (treeView != null)
    //        {
    //            for (int i = 0; i < treeView.Items.Count; i++)
    //            {
    //                var item = treeView.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
    //                if (item != null)
    //                {
    //                    SetIsItemSelected(item, false);
    //                    DeSelectAllItems(null, item);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            for (int i = 0; i < treeViewItem.Items.Count; i++)
    //            {
    //                var item = treeViewItem.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
    //                if (item != null)
    //                {
    //                    SetIsItemSelected(item, false);
    //                    DeSelectAllItems(null, item);
    //                }
    //            }
    //        }
    //    }

    //    private static TreeView? FindTreeView(DependencyObject dependencyObject)
    //    {
    //        if (dependencyObject == null)
    //        {
    //            return null;
    //        }

    //        var treeView = dependencyObject as TreeView;

    //        return treeView ?? FindTreeView(VisualTreeHelper.GetParent(dependencyObject));
    //    }

    //    private static void SelectMultipleItemsRandomly(TreeView treeView, TreeViewItem treeViewItem)
    //    {
    //        SetIsItemSelected(treeViewItem, !GetIsItemSelected(treeViewItem));
    //        if (GetStartItem(treeView) == null || Keyboard.Modifiers == ModifierKeys.Control)
    //        {
    //            if (GetIsItemSelected(treeViewItem))
    //            {
    //                SetStartItem(treeView, treeViewItem);
    //            }
    //        }
    //        else
    //        {
    //            if (GetSelectedItems(treeView).Count == 0)
    //            {
    //                SetStartItem(treeView, null);
    //            }
    //        }
    //    }

    //    private static void SelectMultipleItemsContinuously(TreeView treeView, TreeViewItem treeViewItem, bool shiftControl = false)
    //    {
    //        TreeViewItem startItem = GetStartItem(treeView);
    //        if (startItem != null)
    //        {
    //            if (startItem == treeViewItem)
    //            {
    //                SelectSingleItem(treeView, treeViewItem);
    //                return;
    //            }

    //            ICollection<TreeViewItem> allItems = new List<TreeViewItem>();
    //            GetAllItems(treeView, null, allItems);
    //            //DeSelectAllItems(treeView, null);
    //            bool isBetween = false;
    //            foreach (var item in allItems)
    //            {
    //                if (item == treeViewItem || item == startItem)
    //                {
    //                    // toggle to true if first element is found and
    //                    // back to false if last element is found
    //                    isBetween = !isBetween;

    //                    // set boundary element
    //                    SetIsItemSelected(item, true);
    //                    continue;
    //                }

    //                if (isBetween)
    //                {
    //                    SetIsItemSelected(item, true);
    //                    continue;
    //                }

    //                if (!shiftControl)
    //                    SetIsItemSelected(item, false);
    //            }
    //        }
    //    }

    //    private static void GetAllItems(TreeView treeView, TreeViewItem treeViewItem, ICollection<TreeViewItem> allItems)
    //    {
    //        if (treeView != null)
    //        {
    //            for (int i = 0; i < treeView.Items.Count; i++)
    //            {
    //                var item = treeView.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
    //                if (item != null)
    //                {
    //                    allItems.Add(item);
    //                    GetAllItems(null, item, allItems);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            for (int i = 0; i < treeViewItem.Items.Count; i++)
    //            {
    //                var item = treeViewItem.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
    //                if (item != null)
    //                {
    //                    allItems.Add(item);
    //                    GetAllItems(null, item, allItems);
    //                }
    //            }
    //        }
    //    }
    //}
}

#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.