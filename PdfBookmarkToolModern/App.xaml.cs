using System.Windows;

namespace PdfBookmarkToolModern
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // ログ設定やグローバル例外ハンドラーなどを設定可能
            
            // メインウィンドウを表示
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
} 