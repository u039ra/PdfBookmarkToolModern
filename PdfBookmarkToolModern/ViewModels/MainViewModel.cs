using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NLog;
using PdfBookmarkToolModern.Commands;
using PdfBookmarkToolModern.Services;
using ClosedXML.Excel;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;
using WpfSaveFileDialog = Microsoft.Win32.SaveFileDialog;
using WpfMessageBox = System.Windows.MessageBox;
using WpfApplication = System.Windows.Application;

namespace PdfBookmarkToolModern.ViewModels
{
    /// <summary>
    /// メインウィンドウのViewModel（シンプル版）
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private ObservableCollection<BookmarkViewModel> _bookmarks;
        private BookmarkViewModel? _selectedBookmark;
        private string _windowTitle = Constants.APP_NAME;
        private bool _isProcessing;
        private string _statusMessage = "準備完了";
        private string? _currentPdfPath;

        public MainViewModel()
        {
            _bookmarks = new ObservableCollection<BookmarkViewModel>();
            InitializeCommands();
        }

        #region Properties

        /// <summary>
        /// ブックマークのコレクション
        /// </summary>
        public ObservableCollection<BookmarkViewModel> Bookmarks
        {
            get => _bookmarks;
            set => SetProperty(ref _bookmarks, value);
        }

        /// <summary>
        /// 選択されたブックマーク
        /// </summary>
        public BookmarkViewModel? SelectedBookmark
        {
            get => _selectedBookmark;
            set => SetProperty(ref _selectedBookmark, value);
        }

        /// <summary>
        /// ウィンドウタイトル
        /// </summary>
        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }

        /// <summary>
        /// 処理中フラグ
        /// </summary>
        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        /// <summary>
        /// ステータスメッセージ
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// アクションタイプの選択肢
        /// </summary>
        public string[] ActionTypes => Constants.ACTION_TYPES;

        /// <summary>
        /// 表示オプションの選択肢
        /// </summary>
        public string[] DisplayOptions => Constants.DISPLAY_OPTIONS;

        /// <summary>
        /// ブックマークが存在するかどうか
        /// </summary>
        public bool HasBookmarks => Bookmarks.Any();

        /// <summary>
        /// ブックマークが選択されているかどうか
        /// </summary>
        public bool HasSelectedBookmark => SelectedBookmark != null;

        #endregion

        #region Commands

        public ICommand LoadPdfCommand { get; private set; } = null!;
        public ICommand CheckBookmarksCommand { get; private set; } = null!;
        public ICommand AddBookmarkCommand { get; private set; } = null!;
        public ICommand DeleteBookmarkCommand { get; private set; } = null!;
        public ICommand ApplyChangesCommand { get; private set; } = null!;
        public ICommand SaveToExcelCommand { get; private set; } = null!;
        public ICommand LoadFromExcelCommand { get; private set; } = null!;
        public ICommand WriteToPdfCommand { get; private set; } = null!;
        public ICommand WriteToBatchPdfCommand { get; private set; } = null!;

        #endregion

        #region Command Implementations

        private void InitializeCommands()
        {
            LoadPdfCommand = new RelayCommand(() => StatusMessage = "PDF読み込み機能（未実装）");
            CheckBookmarksCommand = new RelayCommand(() => StatusMessage = "ブックマークチェック機能（未実装）");
            AddBookmarkCommand = new RelayCommand(AddBookmark);
            DeleteBookmarkCommand = new RelayCommand(DeleteBookmark);
            ApplyChangesCommand = new RelayCommand(() => StatusMessage = "変更適用機能（未実装）");
            SaveToExcelCommand = new RelayCommand(() => StatusMessage = "Excel保存機能（未実装）");
            LoadFromExcelCommand = new RelayCommand(() => StatusMessage = "Excel読み込み機能（未実装）");
            WriteToPdfCommand = new RelayCommand(() => StatusMessage = "PDF書き込み機能（未実装）");
            WriteToBatchPdfCommand = new RelayCommand(() => StatusMessage = "一括PDF書き込み機能（未実装）");
        }

        private void AddBookmark()
        {
            var newBookmark = new BookmarkViewModel
            {
                Title = "新しいブックマーク",
                Level = 1,
                ActionType = Constants.PDF_ACTION_GOTO,
                LinkPage = "1",
                DisplayOption = Constants.PDF_ACTION_XYZ
            };

            Bookmarks.Add(newBookmark);
            SelectedBookmark = newBookmark;
            StatusMessage = "新しいブックマークを追加しました";
        }

        private void DeleteBookmark()
        {
            if (SelectedBookmark != null)
            {
                Bookmarks.Remove(SelectedBookmark);
                SelectedBookmark = null;
                StatusMessage = "ブックマークを削除しました";
            }
        }

        #endregion

        #region INotifyPropertyChanged

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

        #endregion
    }
}