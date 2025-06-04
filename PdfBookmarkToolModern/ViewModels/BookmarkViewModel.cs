using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PdfBookmarkToolModern.ViewModels
{
    /// <summary>
    /// WPFデータバインディング用のブックマークビューモデル
    /// </summary>
    public class BookmarkViewModel : INotifyPropertyChanged
    {
        private int _level;
        private string? _title;
        private string? _linkPage;
        private string? _actionType;
        private string? _displayOption;
        private string? _xCoord;
        private string? _yCoord;
        private string? _linkFile;
        private bool _isSelected;
        private bool _isExpanded = true;

        public BookmarkViewModel()
        {
            Children = new ObservableCollection<BookmarkViewModel>();
            Level = 1;
            Title = "新しいブックマーク";
            LinkPage = "1";
            ActionType = "GoTo";
            DisplayOption = "XYZ";
        }

        public BookmarkViewModel(BookmarkEntry entry) : this()
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            Level = entry.Level;
            Title = entry.Title;
            LinkPage = entry.LinkPage;
            ActionType = entry.ActionType;
            DisplayOption = entry.DisplayOption;
            XCoord = entry.XCoord;
            YCoord = entry.YCoord;
            LinkFile = entry.LinkFile;

            foreach (var child in entry.Children)
            {
                Children.Add(new BookmarkViewModel(child));
            }
        }

        /// <summary>
        /// ブックマークのレベル（階層の深さ）
        /// </summary>
        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        /// <summary>
        /// ブックマークのタイトル
        /// </summary>
        public string? Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// リンク先のページ番号
        /// </summary>
        public string? LinkPage
        {
            get => _linkPage;
            set => SetProperty(ref _linkPage, value);
        }

        /// <summary>
        /// アクションタイプ (GoTo, GoToR など)
        /// </summary>
        public string? ActionType
        {
            get => _actionType;
            set => SetProperty(ref _actionType, value);
        }

        /// <summary>
        /// 表示オプション (XYZ など)
        /// </summary>
        public string? DisplayOption
        {
            get => _displayOption;
            set => SetProperty(ref _displayOption, value);
        }

        /// <summary>
        /// X座標
        /// </summary>
        public string? XCoord
        {
            get => _xCoord;
            set => SetProperty(ref _xCoord, value);
        }

        /// <summary>
        /// Y座標
        /// </summary>
        public string? YCoord
        {
            get => _yCoord;
            set => SetProperty(ref _yCoord, value);
        }

        /// <summary>
        /// リンク先ファイル（外部ファイルへのリンクの場合）
        /// </summary>
        public string? LinkFile
        {
            get => _linkFile;
            set => SetProperty(ref _linkFile, value);
        }

        /// <summary>
        /// 子ブックマークのコレクション
        /// </summary>
        public ObservableCollection<BookmarkViewModel> Children { get; }

        /// <summary>
        /// TreeViewでの選択状態
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// TreeViewでの展開状態
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        /// <summary>
        /// 表示用のツールチップテキスト
        /// </summary>
        public string DisplayText
        {
            get
            {
                var details = new List<string>();
                if (!string.IsNullOrEmpty(Title))
                    details.Add($"タイトル: {Title}");
                if (!string.IsNullOrEmpty(ActionType))
                    details.Add($"アクション: {ActionType}");
                if (!string.IsNullOrEmpty(LinkFile))
                    details.Add($"リンクファイル: {LinkFile}");
                if (!string.IsNullOrEmpty(LinkPage))
                    details.Add($"リンクページ: {LinkPage}");
                if (!string.IsNullOrEmpty(DisplayOption))
                    details.Add($"表示オプション: {DisplayOption}");
                if (!string.IsNullOrEmpty(XCoord) || !string.IsNullOrEmpty(YCoord))
                    details.Add($"座標: X={XCoord ?? "未設定"}, Y={YCoord ?? "未設定"}");

                return string.Join(Environment.NewLine, details);
            }
        }

        /// <summary>
        /// BookmarkEntryに変換
        /// </summary>
        public BookmarkEntry ToBookmarkEntry()
        {
            var entry = new BookmarkEntry
            {
                Level = Level,
                Title = Title,
                LinkPage = LinkPage,
                ActionType = ActionType,
                DisplayOption = DisplayOption,
                XCoord = XCoord,
                YCoord = YCoord,
                LinkFile = LinkFile
            };

            foreach (var child in Children)
            {
                entry.Children.Add(child.ToBookmarkEntry());
            }

            return entry;
        }

        /// <summary>
        /// 階層構造をフラットなリストに変換
        /// </summary>
        public static List<BookmarkExcelRow> ToFlatList(IEnumerable<BookmarkViewModel> bookmarks)
        {
            var result = new List<BookmarkExcelRow>();
            ToFlatListRecursive(bookmarks, result);
            return result;
        }

        private static void ToFlatListRecursive(IEnumerable<BookmarkViewModel> bookmarks, List<BookmarkExcelRow> result)
        {
            foreach (var bookmark in bookmarks)
            {
                result.Add(new BookmarkExcelRow
                {
                    Level = bookmark.Level,
                    Title = bookmark.Title,
                    LinkPage = bookmark.LinkPage,
                    ActionType = bookmark.ActionType,
                    DisplayOption = bookmark.DisplayOption,
                    XCoord = bookmark.XCoord,
                    YCoord = bookmark.YCoord,
                    LinkFile = bookmark.LinkFile
                });

                if (bookmark.Children.Any())
                {
                    ToFlatListRecursive(bookmark.Children, result);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
} 