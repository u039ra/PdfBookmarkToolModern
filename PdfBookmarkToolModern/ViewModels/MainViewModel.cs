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
            LoadPdfCommand = new RelayCommand(async () => await LoadPdfAsync());
            CheckBookmarksCommand = new RelayCommand(async () => await CheckBookmarksAsync());
            AddBookmarkCommand = new RelayCommand(AddBookmark);
            DeleteBookmarkCommand = new RelayCommand(DeleteBookmark, () => HasSelectedBookmark);
            ApplyChangesCommand = new RelayCommand(ApplyChanges, () => HasSelectedBookmark);
            SaveToExcelCommand = new RelayCommand(async () => await SaveToExcelAsync(), () => HasBookmarks);
            LoadFromExcelCommand = new RelayCommand(async () => await LoadFromExcelAsync());
            WriteToPdfCommand = new RelayCommand(async () => await WriteToPdfAsync(), () => HasBookmarks && !string.IsNullOrEmpty(_currentPdfPath));
            WriteToBatchPdfCommand = new RelayCommand(async () => await WriteToBatchPdfAsync(), () => HasBookmarks);
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
            if (SelectedBookmark == null) return;

            var result = System.Windows.MessageBox.Show(
                $"ブックマーク '{SelectedBookmark.Title}' を削除しますか？\n子ブックマークも削除されます。",
                "ブックマーク削除",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question
            );

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                RemoveBookmarkFromCollection(SelectedBookmark, Bookmarks);
                SelectedBookmark = null;
                OnPropertyChanged(nameof(HasBookmarks));
                OnPropertyChanged(nameof(HasSelectedBookmark));
                RefreshCommands();
                StatusMessage = "ブックマークを削除しました";
            }
        }

        private void RemoveBookmarkFromCollection(BookmarkViewModel bookmarkToRemove, ObservableCollection<BookmarkViewModel> collection)
        {
            if (collection.Remove(bookmarkToRemove))
                return;

            foreach (var bookmark in collection)
            {
                RemoveBookmarkFromCollection(bookmarkToRemove, bookmark.Children);
            }
        }

        private void ApplyChanges()
        {
            if (SelectedBookmark == null) return;
            
            SelectedBookmark.OnPropertyChanged(nameof(BookmarkViewModel.DisplayText));
            StatusMessage = "変更を適用しました";
        }

        private async Task LoadPdfAsync()
        {
            var openFileDialog = new WpfOpenFileDialog
            {
                Title = "PDFファイルを選択",
                Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await LoadPdfFileAsync(openFileDialog.FileName);
            }
        }

        private async Task LoadPdfFileAsync(string pdfPath)
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "PDFファイルを読み込み中...";
                
                await Task.Run(() =>
                {
                    var (bookmarksData, maxLevel) = PdfLogic.ExtractBookmarks(pdfPath);
                    var bookmarkTree = PdfLogic.BuildBookmarkTree(bookmarksData);
                    
                    WpfApplication.Current.Dispatcher.Invoke(() =>
                    {
                        Bookmarks.Clear();
                        foreach (var bookmark in bookmarkTree)
                        {
                            Bookmarks.Add(new BookmarkViewModel(bookmark));
                        }
                        
                        _currentPdfPath = pdfPath;
                        WindowTitle = $"{Constants.APP_NAME} - {Path.GetFileName(pdfPath)}";
                        StatusMessage = $"{Bookmarks.Count}個のブックマークを読み込みました";
                        
                        OnPropertyChanged(nameof(HasBookmarks));
                        RefreshCommands();
                    });
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"PDFファイル読み込みエラー: {pdfPath}");
                WpfMessageBox.Show($"PDFファイルの読み込みに失敗しました。\n{ex.Message}", 
                    Constants.MSG_TITLE_PDF_ERROR, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                StatusMessage = "PDFファイルの読み込みに失敗しました";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task CheckBookmarksAsync()
        {
            var openFileDialog = new WpfOpenFileDialog
            {
                Title = "ブックマークをチェックするPDFファイルを選択",
                Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsProcessing = true;
                    StatusMessage = "ブックマークをチェック中...";
                    
                    bool hasBookmarks = await Task.Run(() => PdfLogic.HasBookmarks(openFileDialog.FileName));
                    
                    WpfMessageBox.Show(
                        hasBookmarks ? "ブックマークが存在します。" : "ブックマークは存在しません。",
                        "チェック結果",
                        System.Windows.MessageBoxButton.OK,
                        hasBookmarks ? System.Windows.MessageBoxImage.Information : System.Windows.MessageBoxImage.Warning
                    );
                    
                    StatusMessage = hasBookmarks ? "ブックマークが存在します" : "ブックマークは存在しません";
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"ブックマークチェックエラー: {openFileDialog.FileName}");
                    WpfMessageBox.Show($"ブックマークのチェックに失敗しました。\n{ex.Message}", 
                        Constants.MSG_TITLE_PDF_ERROR, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    StatusMessage = "ブックマークのチェックに失敗しました";
                }
                finally
                {
                    IsProcessing = false;
                }
            }
        }

        private async Task SaveToExcelAsync()
        {
            if (!HasBookmarks) return;

            var saveFileDialog = new WpfSaveFileDialog
            {
                Title = "Excelファイルに保存",
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                FileName = "bookmarks.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsProcessing = true;
                    StatusMessage = "Excelファイルに保存中...";

                    await Task.Run(() =>
                    {
                        var flatList = BookmarkViewModel.ToFlatList(Bookmarks);
                        var maxLevel = flatList.Max(b => b.Level);
                        bool success = ExcelLogic.SaveBookmarksToExcel(saveFileDialog.FileName, flatList, maxLevel);
                        
                        if (!success)
                        {
                            throw new Exception("Excelファイルの保存に失敗しました");
                        }
                    });

                    WpfMessageBox.Show("Excelファイルに保存しました。", Constants.MSG_TITLE_SUCCESS, 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    StatusMessage = "Excelファイルに保存しました";
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Excel保存エラー: {saveFileDialog.FileName}");
                    WpfMessageBox.Show($"Excelファイルの保存に失敗しました。\n{ex.Message}", 
                        Constants.MSG_TITLE_EXCEL_ERROR, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    StatusMessage = "Excelファイルの保存に失敗しました";
                }
                finally
                {
                    IsProcessing = false;
                }
            }
        }

        private async Task LoadFromExcelAsync()
        {
            var openFileDialog = new WpfOpenFileDialog
            {
                Title = "Excelファイルを選択",
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsProcessing = true;
                    StatusMessage = "Excelファイルを読み込み中...";

                    await Task.Run(() =>
                    {
                        var bookmarksData = ExcelLogic.LoadBookmarksFromExcel(openFileDialog.FileName);
                        var bookmarkTree = PdfLogic.BuildBookmarkTree(bookmarksData);
                        
                        WpfApplication.Current.Dispatcher.Invoke(() =>
                        {
                            Bookmarks.Clear();
                            foreach (var bookmark in bookmarkTree)
                            {
                                Bookmarks.Add(new BookmarkViewModel(bookmark));
                            }
                            
                            WindowTitle = $"{Constants.APP_NAME} - {Path.GetFileName(openFileDialog.FileName)}";
                            StatusMessage = $"{Bookmarks.Count}個のブックマークを読み込みました";
                            
                            OnPropertyChanged(nameof(HasBookmarks));
                            RefreshCommands();
                        });
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Excel読み込みエラー: {openFileDialog.FileName}");
                    WpfMessageBox.Show($"Excelファイルの読み込みに失敗しました。\n{ex.Message}", 
                        Constants.MSG_TITLE_EXCEL_ERROR, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    StatusMessage = "Excelファイルの読み込みに失敗しました";
                }
                finally
                {
                    IsProcessing = false;
                }
            }
        }

        private async Task WriteToPdfAsync()
        {
            if (!HasBookmarks || string.IsNullOrEmpty(_currentPdfPath)) return;

            var saveFileDialog = new WpfSaveFileDialog
            {
                Title = "PDFファイルに保存",
                Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                FileName = Path.GetFileNameWithoutExtension(_currentPdfPath) + "_bookmarks.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsProcessing = true;
                    StatusMessage = "PDFファイルに書き込み中...";

                    await Task.Run(() =>
                    {
                        var bookmarkEntries = Bookmarks.Select(b => b.ToBookmarkEntry()).ToList();
                        bool success = PdfLogic.ImportBookmarksToPdf(_currentPdfPath, bookmarkEntries, saveFileDialog.FileName, true);
                        
                        if (!success)
                        {
                            throw new Exception("PDFファイルへの書き込みに失敗しました");
                        }
                    });

                    WpfMessageBox.Show("PDFファイルに書き込みました。", Constants.MSG_TITLE_SUCCESS, 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    StatusMessage = "PDFファイルに書き込みました";
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"PDF書き込みエラー: {saveFileDialog.FileName}");
                    WpfMessageBox.Show($"PDFファイルへの書き込みに失敗しました。\n{ex.Message}", 
                        Constants.MSG_TITLE_PDF_ERROR, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    StatusMessage = "PDFファイルへの書き込みに失敗しました";
                }
                finally
                {
                    IsProcessing = false;
                }
            }
        }

        private async Task WriteToBatchPdfAsync()
        {
            if (!HasBookmarks) return;

            System.Windows.Forms.FolderBrowserDialog folderDialog = new()
            {
                Description = "PDFファイルがあるフォルダを選択"
            };

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    IsProcessing = true;
                    StatusMessage = "一括書き込み中...";

                    await Task.Run(() =>
                    {
                        string targetDir = folderDialog.SelectedPath;
                        string[] pdfFiles = Directory.GetFiles(targetDir, "*.pdf");
                        
                        if (pdfFiles.Length == 0)
                        {
                            WpfApplication.Current.Dispatcher.Invoke(() =>
                            {
                                WpfMessageBox.Show("選択したフォルダにPDFファイルがありません。", 
                                    Constants.MSG_TITLE_INFO, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                            });
                            return;
                        }

                        string outputDir = Path.Combine(targetDir, Constants.DEFAULT_OUTPUT_DIR_NAME);
                        if (!Directory.Exists(outputDir))
                            Directory.CreateDirectory(outputDir);

                        var bookmarkEntries = WpfApplication.Current.Dispatcher.Invoke(() => 
                            Bookmarks.Select(b => b.ToBookmarkEntry()).ToList());
                        
                        int success = 0, fail = 0;
                        
                        foreach (var pdfFile in pdfFiles)
                        {
                            try
                            {
                                string outputPath = Path.Combine(outputDir, Path.GetFileName(pdfFile));
                                bool result = PdfLogic.ImportBookmarksToPdf(pdfFile, bookmarkEntries, outputPath, true);
                                if (result) success++; else fail++;
                            }
                            catch
                            {
                                fail++;
                            }
                        }

                        WpfApplication.Current.Dispatcher.Invoke(() =>
                        {
                            string message = $"{success}個のPDFにブックマークを適用しました。\n出力先: '{Constants.DEFAULT_OUTPUT_DIR_NAME}' フォルダ";
                            if (fail > 0) message += $"\n{fail}個のファイルで失敗しました。";
                            
                            WpfMessageBox.Show(message, 
                                fail == 0 ? Constants.MSG_TITLE_SUCCESS : Constants.MSG_TITLE_WARNING, 
                                System.Windows.MessageBoxButton.OK, 
                                fail == 0 ? System.Windows.MessageBoxImage.Information : System.Windows.MessageBoxImage.Warning);
                        });
                    });

                    StatusMessage = "一括書き込みが完了しました";
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "一括書き込みエラー");
                    WpfMessageBox.Show($"一括書き込みに失敗しました。\n{ex.Message}", 
                        Constants.MSG_TITLE_PDF_ERROR, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    StatusMessage = "一括書き込みに失敗しました";
                }
                finally
                {
                    IsProcessing = false;
                }
            }
        }

        private void RefreshCommands()
        {
            OnPropertyChanged(nameof(HasBookmarks));
            OnPropertyChanged(nameof(HasSelectedBookmark));
            
            if (DeleteBookmarkCommand is RelayCommand deleteCmd) deleteCmd.RaiseCanExecuteChanged();
            if (ApplyChangesCommand is RelayCommand applyCmd) applyCmd.RaiseCanExecuteChanged();
            if (SaveToExcelCommand is RelayCommand saveCmd) saveCmd.RaiseCanExecuteChanged();
            if (WriteToPdfCommand is RelayCommand writeCmd) writeCmd.RaiseCanExecuteChanged();
            if (WriteToBatchPdfCommand is RelayCommand batchCmd) batchCmd.RaiseCanExecuteChanged();
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