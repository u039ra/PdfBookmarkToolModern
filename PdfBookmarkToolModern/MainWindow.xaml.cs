using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PdfBookmarkToolModern.ViewModels;

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
            if (e.NewValue is BookmarkViewModel bookmark)
            {
                ViewModel.SelectedBookmark = bookmark;
            }
        }

        #endregion

        #region Drag and Drop Implementation

        private void TreeView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _draggedItem = GetBookmarkFromPoint(e.GetPosition(BookmarkTreeView));
        }

        private void TreeView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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
                // 元の位置から削除
                RemoveBookmarkFromParent(draggedItem);

                // 新しい位置に追加
                targetItem.Children.Add(draggedItem);
                draggedItem.Level = targetItem.Level + 1;
                
                // 子要素のレベルを再帰的に更新
                UpdateChildLevels(draggedItem);

                // ターゲットを展開
                targetItem.IsExpanded = true;

                // 選択状態を更新
                ViewModel.SelectedBookmark = draggedItem;
                
                ViewModel.StatusMessage = $"ブックマーク '{draggedItem.Title}' を '{targetItem.Title}' の子に移動しました";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"ドラッグ&ドロップエラー: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            e.Handled = true;
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

        #endregion
    }
} 