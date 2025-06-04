using System;
using System.Windows;
using PdfBookmarkToolModern.Models;
using PdfBookmarkToolModern.ViewModels;

namespace PdfBookmarkToolModern.Views
{
    /// <summary>
    /// SettingsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private SettingsViewModel _viewModel;

        public SettingsWindow(AppSettings settings)
        {
            InitializeComponent();
            
            _viewModel = new SettingsViewModel(settings);
            DataContext = _viewModel;
            
            // ViewModelのイベントハンドリング
            _viewModel.SaveCommand.CanExecuteChanged += (s, e) => CheckCanClose();
            _viewModel.CancelCommand.CanExecuteChanged += (s, e) => CheckCanClose();
        }

        /// <summary>
        /// 設定が保存されたかどうか
        /// </summary>
        public bool SettingsSaved { get; private set; }

        private void CheckCanClose()
        {
            // 保存ボタンが実行された場合のハンドリング
            if (_viewModel.SaveCommand is Commands.RelayCommand saveCmd)
            {
                // 実際の保存は ViewModel で行われるため、ここでは結果のみ確認
                // 保存が成功した場合にウィンドウを閉じる
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // ViewModelの設定プロパティの変更通知を購読
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsViewModel.HasChanges))
            {
                // タイトルに変更状態を表示
                Title = _viewModel.HasChanges ? "⚙️ 設定 *" : "⚙️ 設定";
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // 変更がある場合は確認
            if (_viewModel.HasChanges)
            {
                var result = System.Windows.MessageBox.Show(
                    "設定に変更があります。保存せずに閉じますか？",
                    "設定の変更",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // 保存せずに閉じる
                        if (_viewModel.CancelCommand.CanExecute(null))
                            _viewModel.CancelCommand.Execute(null);
                        break;
                    case MessageBoxResult.No:
                        // 保存して閉じる
                        if (_viewModel.SaveCommand.CanExecute(null))
                            _viewModel.SaveCommand.Execute(null);
                        SettingsSaved = true;
                        break;
                    case MessageBoxResult.Cancel:
                        // キャンセル
                        e.Cancel = true;
                        return;
                }
            }

            base.OnClosing(e);
        }

        /// <summary>
        /// 設定画面を表示する静的メソッド
        /// </summary>
        public static bool ShowSettingsDialog(Window owner, AppSettings settings)
        {
            var settingsWindow = new SettingsWindow(settings)
            {
                Owner = owner
            };

            settingsWindow.ShowDialog();
            return settingsWindow.SettingsSaved;
        }
    }
} 