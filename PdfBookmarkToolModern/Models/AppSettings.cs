using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PdfBookmarkToolModern.Models
{
    /// <summary>
    /// アプリケーション設定
    /// </summary>
    public class AppSettings : INotifyPropertyChanged
    {
        private Theme _currentTheme = Theme.Light;
        private string _language = "ja-JP";
        private bool _expandBookmarksOnSave = true;
        private bool _showConfirmationDialogs = true;
        private int _recentFileCount = 10;
        private string _defaultOutputDirectory = "output";
        private bool _autoBackup = true;
        private int _autoSaveInterval = 300; // 秒

        /// <summary>
        /// 現在のテーマ
        /// </summary>
        public Theme CurrentTheme
        {
            get => _currentTheme;
            set => SetProperty(ref _currentTheme, value);
        }

        /// <summary>
        /// 言語設定
        /// </summary>
        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        /// <summary>
        /// PDF保存時にブックマークを展開するかどうか
        /// </summary>
        public bool ExpandBookmarksOnSave
        {
            get => _expandBookmarksOnSave;
            set => SetProperty(ref _expandBookmarksOnSave, value);
        }

        /// <summary>
        /// 確認ダイアログを表示するかどうか
        /// </summary>
        public bool ShowConfirmationDialogs
        {
            get => _showConfirmationDialogs;
            set => SetProperty(ref _showConfirmationDialogs, value);
        }

        /// <summary>
        /// 最近使用したファイルの保持数
        /// </summary>
        public int RecentFileCount
        {
            get => _recentFileCount;
            set => SetProperty(ref _recentFileCount, value);
        }

        /// <summary>
        /// デフォルト出力ディレクトリ
        /// </summary>
        public string DefaultOutputDirectory
        {
            get => _defaultOutputDirectory;
            set => SetProperty(ref _defaultOutputDirectory, value);
        }

        /// <summary>
        /// 自動バックアップの有効/無効
        /// </summary>
        public bool AutoBackup
        {
            get => _autoBackup;
            set => SetProperty(ref _autoBackup, value);
        }

        /// <summary>
        /// 自動保存間隔（秒）
        /// </summary>
        public int AutoSaveInterval
        {
            get => _autoSaveInterval;
            set => SetProperty(ref _autoSaveInterval, value);
        }

        /// <summary>
        /// 利用可能なテーマ一覧
        /// </summary>
        public Theme[] AvailableThemes => new[] { Theme.Light, Theme.Dark, Theme.Auto };

        /// <summary>
        /// 利用可能な言語一覧
        /// </summary>
        public string[] AvailableLanguages => new[] { "ja-JP", "en-US" };

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

    /// <summary>
    /// テーマ列挙型
    /// </summary>
    public enum Theme
    {
        Light,
        Dark,
        Auto
    }
} 