using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PdfBookmarkToolModern.ViewModels;
using System.Linq;

namespace PdfBookmarkToolModern
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;
        private System.Windows.Point _dragStartPoint;
        private BookmarkViewModel? _draggedItem;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        #region Event Handlers

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BookmarkTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is BookmarkViewModel bookmark && ViewModel.SelectedBookmark != bookmark)
            {
                ViewModel.SelectedBookmark = bookmark;
            }
        }

        private void TreeView_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _dragStartPoint = e.GetPosition(null);
                _draggedItem = GetBookmarkFromPoint(e.GetPosition(BookmarkTreeView));
            }
        }

        private void TreeView_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedItem != null)
            {
                System.Windows.Point mousePos = e.GetPosition(null);
                Vector diff = _dragStartPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    // ドラッグ開始
                    var dragData = new System.Windows.DataObject("BookmarkViewModel", _draggedItem);
                    DragDrop.DoDragDrop(BookmarkTreeView, dragData, System.Windows.DragDropEffects.Move);
                }
            }
        }

        private void TreeView_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("BookmarkViewModel"))
            {
                e.Effects = System.Windows.DragDropEffects.None;
                return;
            }

            var targetItem = GetBookmarkFromPoint(e.GetPosition(BookmarkTreeView));
            var draggedItem = e.Data.GetData("BookmarkViewModel") as BookmarkViewModel;

            if (targetItem == null || draggedItem == null || targetItem == draggedItem ||
                IsAncestor(draggedItem, targetItem))
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.Move;
                
                // 既存インジケーター削除
                var indicator = FindDropIndicator();
                if (indicator != null)
                    DropIndicatorCanvas.Children.Remove(indicator);

                // 既存ハイライト解除
                var allItems = GetAllTreeViewItems(BookmarkTreeView);
                foreach (var item in allItems)
                {
                    item.ClearValue(TreeViewItem.BackgroundProperty);
                }

                // ドロップ位置のインジケーターまたはハイライトを表示
                var dropPosition = GetDropPosition(e.GetPosition(BookmarkTreeView));
                if (dropPosition != null)
                {
                    var targetItemContainer = GetTreeViewItem(targetItem);
                    if (targetItemContainer != null)
                    {
                        if (dropPosition == DropPosition.Before || dropPosition == DropPosition.After)
                        {
                            indicator = new System.Windows.Shapes.Rectangle
                            {
                                Height = 1,
                                Fill = System.Windows.Media.Brushes.Blue,
                                Width = targetItemContainer.ActualWidth,
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
                            };
                            System.Windows.Controls.Panel.SetZIndex(indicator, 100);
                            DropIndicatorCanvas.Children.Add(indicator);
                            var transform = targetItemContainer.TransformToAncestor(BookmarkTreeView);
                            var position = transform.Transform(new System.Windows.Point(0, 0));
                            System.Windows.Controls.Canvas.SetLeft(indicator, position.X);
                            System.Windows.Controls.Canvas.SetTop(indicator, position.Y + (dropPosition == DropPosition.Before ? 0 : targetItemContainer.ActualHeight));
                        }
                        else if (dropPosition == DropPosition.Into)
                        {
                            targetItemContainer.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224)); // 選択時と同じ色
                        }
                    }
                }
            }

            e.Handled = true;
        }

        private void TreeView_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("BookmarkViewModel"))
                return;

            var draggedItem = e.Data.GetData("BookmarkViewModel") as BookmarkViewModel;
            var targetItem = GetBookmarkFromPoint(e.GetPosition(BookmarkTreeView));

            if (draggedItem == null || targetItem == null || draggedItem == targetItem)
                return;

            // 循環参照チェック
            if (IsAncestor(draggedItem, targetItem))
                return;

            try
            {
                // ドロップ位置を取得
                var dropPosition = GetDropPosition(e.GetPosition(BookmarkTreeView));
                if (dropPosition == null) return;

                // 元の位置から削除
                RemoveBookmarkFromParent(draggedItem);

                if (dropPosition == DropPosition.Into)
                {
                    // 子として追加
                    targetItem.Children.Add(draggedItem);
                    draggedItem.Level = targetItem.Level + 1;
                    targetItem.IsExpanded = true;
                }
                else
                {
                    // 新しい位置に追加（同じ階層）
                    var parent = GetParentBookmark(targetItem);
                    if (parent != null)
                    {
                        var index = parent.Children.IndexOf(targetItem);
                        parent.Children.Insert(dropPosition == DropPosition.After ? index + 1 : index, draggedItem);
                        draggedItem.Level = targetItem.Level;
                    }
                    else
                    {
                        var index = ViewModel.Bookmarks.IndexOf(targetItem);
                        ViewModel.Bookmarks.Insert(dropPosition == DropPosition.After ? index + 1 : index, draggedItem);
                        draggedItem.Level = 1;
                    }
                }
                // 子要素のレベルを再帰的に更新
                UpdateChildLevels(draggedItem);

                // 選択状態を更新
                ViewModel.SelectedBookmark = draggedItem;
                ViewModel.StatusMessage = $"ブックマーク '{draggedItem.Title}' を移動しました";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"ドラッグ&ドロップエラー: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // ドロップインジケーターを削除
                var indicator = FindDropIndicator();
                if (indicator != null)
                {
                    DropIndicatorCanvas.Children.Remove(indicator);
                }
                // ハイライト解除
                var allItems = GetAllTreeViewItems(BookmarkTreeView);
                foreach (var item in allItems)
                {
                    item.ClearValue(TreeViewItem.BackgroundProperty);
                }
            }

            e.Handled = true;
        }

        private void TreeView_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            // インジケーター削除
            var indicator = FindDropIndicator();
            if (indicator != null)
                DropIndicatorCanvas.Children.Remove(indicator);
            // ハイライト解除
            var allItems = GetAllTreeViewItems(BookmarkTreeView);
            foreach (var item in allItems)
            {
                item.ClearValue(TreeViewItem.BackgroundProperty);
            }
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.IsSelected = true;
                item.Focus();
                e.Handled = true;
            }
        }

        #endregion

        #region Helper Methods

        private BookmarkViewModel? GetBookmarkFromPoint(System.Windows.Point point)
        {
            var hitTestResult = VisualTreeHelper.HitTest(BookmarkTreeView, point);
            
            if (hitTestResult?.VisualHit is DependencyObject visualHit)
            {
                var treeViewItem = FindAncestor<TreeViewItem>(visualHit);
                return treeViewItem?.DataContext as BookmarkViewModel;
            }
            
            return null;
        }

        private T? FindAncestor<T>(DependencyObject current) where T : class
        {
            do
            {
                if (current is T result)
                    return result;
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            
            return null;
        }

        private bool IsAncestor(BookmarkViewModel ancestor, BookmarkViewModel descendant)
        {
            var parent = GetParentBookmark(descendant);
            while (parent != null)
            {
                if (parent == ancestor)
                    return true;
                parent = GetParentBookmark(parent);
            }
            return false;
        }

        private BookmarkViewModel? GetParentBookmark(BookmarkViewModel bookmark)
        {
            // ルートレベルから検索
            foreach (var rootBookmark in ViewModel.Bookmarks)
            {
                var parent = FindParentRecursive(rootBookmark, bookmark);
                if (parent != null)
                    return parent;
            }
            return null;
        }

        private BookmarkViewModel? FindParentRecursive(BookmarkViewModel parent, BookmarkViewModel target)
        {
            foreach (var child in parent.Children)
            {
                if (child == target)
                    return parent;
                
                var found = FindParentRecursive(child, target);
                if (found != null)
                    return found;
            }
            return null;
        }

        private void RemoveBookmarkFromParent(BookmarkViewModel bookmark)
        {
            var parent = GetParentBookmark(bookmark);
            if (parent != null)
            {
                parent.Children.Remove(bookmark);
            }
            else
            {
                ViewModel.Bookmarks.Remove(bookmark);
            }
        }

        private void UpdateChildLevels(BookmarkViewModel parent)
        {
            foreach (var child in parent.Children)
            {
                child.Level = parent.Level + 1;
                UpdateChildLevels(child);
            }
        }

        private enum DropPosition
        {
            Before,
            After,
            Into
        }

        private DropPosition? GetDropPosition(System.Windows.Point point)
        {
            var targetItem = GetBookmarkFromPoint(point);
            if (targetItem == null) return null;

            var targetItemContainer = GetTreeViewItem(targetItem);
            if (targetItemContainer == null) return null;

            var relativePoint = point - targetItemContainer.TransformToAncestor(BookmarkTreeView).Transform(new System.Windows.Point(0, 0));
            double height = targetItemContainer.ActualHeight;
            if (relativePoint.Y < height * 0.25)
                return DropPosition.Before;
            else if (relativePoint.Y > height * 0.75)
                return DropPosition.After;
            else
                return DropPosition.Into;
        }

        private System.Windows.Shapes.Rectangle? FindDropIndicator()
        {
            return DropIndicatorCanvas.Children.OfType<System.Windows.Shapes.Rectangle>().FirstOrDefault();
        }

        private TreeViewItem? GetTreeViewItem(BookmarkViewModel bookmark)
        {
            return FindTreeViewItem(BookmarkTreeView, bookmark);
        }

        private TreeViewItem? FindTreeViewItem(ItemsControl parent, BookmarkViewModel bookmark)
        {
            foreach (var item in parent.Items)
            {
                if (item is BookmarkViewModel vm && vm == bookmark)
                {
                    return parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                }

                if (item is BookmarkViewModel parentVm)
                {
                    var container = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                    if (container != null)
                    {
                        var result = FindTreeViewItem(container, bookmark);
                        if (result != null) return result;
                    }
                }
            }
            return null;
        }

        // すべてのTreeViewItemを再帰的に取得
        private System.Collections.Generic.List<TreeViewItem> GetAllTreeViewItems(ItemsControl parent)
        {
            var result = new System.Collections.Generic.List<TreeViewItem>();
            foreach (var item in parent.Items)
            {
                var container = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (container != null)
                {
                    result.Add(container);
                    result.AddRange(GetAllTreeViewItems(container));
                }
            }
            return result;
        }

        #endregion
    }
} 