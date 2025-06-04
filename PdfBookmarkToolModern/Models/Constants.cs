using System;

namespace PdfBookmarkToolModern
{
    /// <summary>
    /// アプリケーション全体で使用する定数定義
    /// </summary>
    public static class Constants
    {
        // PDF関連の定数
        public const string PDF_KEY_ACTION_TYPE_S = "/S";
        public const string PDF_KEY_DESTINATION_S = "/D";
        public const string PDF_KEY_FILE_SPEC_ENTRY_S = "/F";
        public const string PDF_KEY_UNICODE_FILENAME_S = "/UF";
        
        // PDFアクションタイプ
        public const string PDF_ACTION_GOTO = "GoTo";
        public const string PDF_ACTION_GOTOR = "GoToR";
        public const string PDF_ACTION_XYZ = "XYZ";
        
        // デフォルト値
        public const string DEFAULT_BOOKMARK_TITLE = "(タイトルなし)";
        public const string DEFAULT_OUTPUT_DIR_NAME = "PDFしおり追加済み";
        public const string NAMED_DESTINATION_PLACEHOLDER = "[名前付き先]";
        
        // メッセージタイトル
        public const string MSG_TITLE_FILE_ERROR = "ファイルエラー";
        public const string MSG_TITLE_PDF_ERROR = "PDF処理エラー";
        public const string MSG_TITLE_UNEXPECTED_ERROR = "予期せぬエラー";
        public const string MSG_TITLE_EXCEL_ERROR = "Excel処理エラー";
        public const string MSG_TITLE_SUCCESS = "完了";
        public const string MSG_TITLE_WARNING = "警告";
        public const string MSG_TITLE_INFO = "情報";
        public const string MSG_TITLE_NO_DATA_EXTRACT = "データなし";
        public const string MSG_TITLE_EXCEL_SAVE_ERROR = "Excel保存エラー";
        
        // Excel関連の定数
        public const string SHEET_NAME_BOOKMARKS = "ブックマーク";
        public const string COLUMN_LEVEL = "レベル";
        public const string COLUMN_PREFIX_LEVEL = "レベル";
        public const string COLUMN_LEVEL_TITLE_SUFFIX = "タイトル";
        public const string COLUMN_ACTION_TYPE = "アクションタイプ";
        public const string COLUMN_LINK_FILE = "リンク先ファイル";
        public const string COLUMN_LINK_PAGE = "リンク先ページ";
        public const string COLUMN_DISPLAY_OPTION = "表示オプション";
        public const string COLUMN_X_COORD = "X座標";
        public const string COLUMN_Y_COORD = "Y座標";
        
        // アクションタイプの選択肢
        public static readonly string[] ACTION_TYPES = { "", "GoTo", "GoToR" };
        
        // 表示オプションの選択肢
        public static readonly string[] DISPLAY_OPTIONS = { "", "XYZ", "Fit", "FitH", "FitV", "FitR", "FitB", "FitBH", "FitBV" };
        
        // フォーマット
        public static readonly string[] SUPPORTED_PDF_EXTENSIONS = { ".pdf", ".PDF" };
        public static readonly string[] SUPPORTED_EXCEL_EXTENSIONS = { ".xlsx", ".xls" };
        
        // アプリケーション情報
        public const string APP_NAME = "PDFしおり簡易編集ツール";
        public const string APP_VERSION = "2.0.0";
        public const string APP_DESCRIPTION = ".NET 9.0 + PDFsharp 6.2.0版";
    }
    
    /// <summary>
    /// Excelのカラム定義
    /// </summary>
    public static class ExcelColumns
    {
        public const int LEVEL = 1;
        public const int TITLE = 2;
        public const int PAGE = 3;
        public const int ACTION_TYPE = 4;
        public const int DISPLAY_OPTION = 5;
        public const int X_COORD = 6;
        public const int Y_COORD = 7;
        public const int LINK_FILE = 8;
        
        public static readonly string[] HEADER_NAMES = 
        {
            "",
            "レベル",
            "タイトル", 
            "ページ",
            "アクションタイプ",
            "表示オプション",
            "X座標",
            "Y座標",
            "リンクファイル"
        };
    }
} 