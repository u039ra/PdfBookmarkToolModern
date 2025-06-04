using System;
using System.Collections.Generic;

namespace PdfBookmarkToolModern
{
    /// <summary>
    /// ブックマークエントリを表すクラス
    /// </summary>
    public class BookmarkEntry
    {
        /// <summary>
        /// ブックマークのレベル（階層の深さ）
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// ブックマークのタイトル
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// リンク先のページ番号
        /// </summary>
        public string? LinkPage { get; set; }

        /// <summary>
        /// アクションタイプ (GoTo, GoToR など)
        /// </summary>
        public string? ActionType { get; set; }

        /// <summary>
        /// 表示オプション (XYZ など)
        /// </summary>
        public string? DisplayOption { get; set; }

        /// <summary>
        /// X座標
        /// </summary>
        public string? XCoord { get; set; }

        /// <summary>
        /// Y座標
        /// </summary>
        public string? YCoord { get; set; }

        /// <summary>
        /// リンク先ファイル（外部ファイルへのリンクの場合）
        /// </summary>
        public string? LinkFile { get; set; }

        /// <summary>
        /// 子ブックマークのリスト
        /// </summary>
        public List<BookmarkEntry> Children { get; set; } = new List<BookmarkEntry>();

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public BookmarkEntry()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public BookmarkEntry(int level, string title)
        {
            Level = level;
            Title = title;
        }

        /// <summary>
        /// デバッグ用の文字列表現
        /// </summary>
        public override string ToString()
        {
            return $"Level:{Level}, Title:{Title}, Page:{LinkPage}, Action:{ActionType}";
        }
    }

    /// <summary>
    /// Excelから読み込んだブックマーク行データ
    /// </summary>
    public class BookmarkExcelRow
    {
        public int Level { get; set; }
        public string? Title { get; set; }
        public string? LinkPage { get; set; }
        public string? ActionType { get; set; }
        public string? DisplayOption { get; set; }
        public string? XCoord { get; set; }
        public string? YCoord { get; set; }
        public string? LinkFile { get; set; }

        public BookmarkExcelRow()
        {
        }

        /// <summary>
        /// BookmarkEntryに変換
        /// </summary>
        public BookmarkEntry ToBookmarkEntry()
        {
            return new BookmarkEntry
            {
                Level = this.Level,
                Title = this.Title,
                LinkPage = this.LinkPage,
                ActionType = this.ActionType,
                DisplayOption = this.DisplayOption,
                XCoord = this.XCoord,
                YCoord = this.YCoord,
                LinkFile = this.LinkFile
            };
        }
    }
} 