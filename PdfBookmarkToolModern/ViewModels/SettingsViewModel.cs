using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PdfBookmarkToolModern.Commands;
using PdfBookmarkToolModern.Models;
using PdfBookmarkToolModern.Services;

namespace PdfBookmarkToolModern.ViewModels
{
    /// <summary>
    /// 設定画面のViewModel
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private AppSettings _settings;
        private AppSettings _originalSettings;

        public SettingsViewModel(AppSettings settings)
        {
            _originalSettings = settings;
            // 設定のコピーを作成（キャンセル時に元に戻すため）
            _settings = new AppSettings
            {
                CurrentTheme = settings.CurrentTheme,
                Language = settings.Language,
                ExpandBookmarksOnSave = settings.ExpandBookmarksOnSave,
                ShowConfirmationDialogs = settings.ShowConfirmationDialogs,
                RecentFileCount = settings.RecentFileCount,
                DefaultOutputDirectory = settings.DefaultOutputDirectory,
                AutoBackup = settings.AutoBackup,
                AutoSaveInterval = settings.AutoSaveInterval
            };

            InitializeCommands();
        }

        #region Properties

        /// <summary>
        /// 設定オブジェクト
        /// </summary>
        public AppSettings Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        /// <summary>
        /// 設定が変更されているかどうか
        /// </summary>
        public bool HasChanges => !AreSettingsEqual(_settings, _originalSettings);

        #endregion

        #region Commands

        public ICommand SaveCommand { get; private set; } = null!;
        public ICommand CancelCommand { get; private set; } = null!;
        public ICommand ResetToDefaultCommand { get; private set; } = null!;
        public ICommand SelectOutputDirectoryCommand { get; private set; } = null!;

        #endregion

        #region Command Implementations

        private void InitializeCommands()
        {
            SaveCommand = new RelayCommand(Save, () => HasChanges);
            CancelCommand = new RelayCommand(Cancel);
            ResetToDefaultCommand = new RelayCommand(ResetToDefault);
            SelectOutputDirectoryCommand = new RelayCommand(SelectOutputDirectory);
        }

        private void Save()
        {
            // 元の設定に変更を適用
            _originalSettings.CurrentTheme = _settings.CurrentTheme;
            _originalSettings.Language = _settings.Language;
            _originalSettings.ExpandBookmarksOnSave = _settings.ExpandBookmarksOnSave;
            _originalSettings.ShowConfirmationDialogs = _settings.ShowConfirmationDialogs;
            _originalSettings.RecentFileCount = _settings.RecentFileCount;
            _originalSettings.DefaultOutputDirectory = _settings.DefaultOutputDirectory;
            _originalSettings.AutoBackup = _settings.AutoBackup;
            _originalSettings.AutoSaveInterval = _settings.AutoSaveInterval;

            // 設定を保存
            SettingsService.SaveSettings(_originalSettings);

            OnPropertyChanged(nameof(HasChanges));
            RefreshCommands();
        }

        private void Cancel()
        {
            // 設定を元に戻す
            _settings.CurrentTheme = _originalSettings.CurrentTheme;
            _settings.Language = _originalSettings.Language;
            _settings.ExpandBookmarksOnSave = _originalSettings.ExpandBookmarksOnSave;
            _settings.ShowConfirmationDialogs = _originalSettings.ShowConfirmationDialogs;
            _settings.RecentFileCount = _originalSettings.RecentFileCount;
            _settings.DefaultOutputDirectory = _originalSettings.DefaultOutputDirectory;
            _settings.AutoBackup = _originalSettings.AutoBackup;
            _settings.AutoSaveInterval = _originalSettings.AutoSaveInterval;

            OnPropertyChanged(nameof(HasChanges));
            RefreshCommands();
        }

        private void ResetToDefault()
        {
            var result = System.Windows.MessageBox.Show(
                "すべての設定をデフォルト値にリセットしますか？",
                "設定リセット",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _settings.CurrentTheme = Theme.Light;
                _settings.Language = "ja-JP";
                _settings.ExpandBookmarksOnSave = true;
                _settings.ShowConfirmationDialogs = true;
                _settings.RecentFileCount = 10;
                _settings.DefaultOutputDirectory = "output";
                _settings.AutoBackup = true;
                _settings.AutoSaveInterval = 300;

                OnPropertyChanged(nameof(Settings));
                OnPropertyChanged(nameof(HasChanges));
                RefreshCommands();
            }
        }

        private void SelectOutputDirectory()
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "デフォルト出力ディレクトリを選択",
                SelectedPath = _settings.DefaultOutputDirectory
            };

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _settings.DefaultOutputDirectory = folderDialog.SelectedPath;
                OnPropertyChanged(nameof(Settings));
                OnPropertyChanged(nameof(HasChanges));
                RefreshCommands();
            }
        }

        #endregion

        #region Helper Methods

        private bool AreSettingsEqual(AppSettings settings1, AppSettings settings2)
        {
            return settings1.CurrentTheme == settings2.CurrentTheme &&
                   settings1.Language == settings2.Language &&
                   settings1.ExpandBookmarksOnSave == settings2.ExpandBookmarksOnSave &&
                   settings1.ShowConfirmationDialogs == settings2.ShowConfirmationDialogs &&
                   settings1.RecentFileCount == settings2.RecentFileCount &&
                   settings1.DefaultOutputDirectory == settings2.DefaultOutputDirectory &&
                   settings1.AutoBackup == settings2.AutoBackup &&
                   settings1.AutoSaveInterval == settings2.AutoSaveInterval;
        }

        private void RefreshCommands()
        {
            if (SaveCommand is RelayCommand saveCmd) saveCmd.RaiseCanExecuteChanged();
        }

        #endregion

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
} 