using ClosedXML.Excel;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfBookmarkToolModern.Services
{
    /// <summary>
    /// Excel連携機能のロジック
    /// </summary>
    public static class ExcelLogic
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// ブックマークリストをExcelファイルに保存
        /// </summary>
        public static bool SaveBookmarksToExcel(string excelPath, List<BookmarkExcelRow> bookmarks, int maxLevel)
        {
            if (bookmarks == null || !bookmarks.Any())
            {
                Logger.Warn("保存するブックマークデータがありません");
                return false;
            }

            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(Constants.SHEET_NAME_BOOKMARKS);

                // ヘッダー行を作成
                CreateHeaderRow(worksheet, maxLevel);

                // データ行を作成
                CreateDataRows(worksheet, bookmarks, maxLevel);

                // 列幅を自動調整
                worksheet.Columns().AdjustToContents();

                // ファイルを保存
                workbook.SaveAs(excelPath);
                Logger.Info($"Excelファイルに保存しました: {Path.GetFileName(excelPath)}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Excel保存エラー: {excelPath}");
                return false;
            }
        }

        /// <summary>
        /// ヘッダー行を作成
        /// </summary>
        private static void CreateHeaderRow(IXLWorksheet worksheet, int maxLevel)
        {
            int col = 1;

            // レベル列
            worksheet.Cell(1, col++).Value = Constants.COLUMN_LEVEL;

            // 各レベルのタイトル列
            for (int level = 1; level <= maxLevel; level++)
            {
                worksheet.Cell(1, col++).Value = $"{Constants.COLUMN_PREFIX_LEVEL}{level}{Constants.COLUMN_LEVEL_TITLE_SUFFIX}";
            }

            // その他の列
            worksheet.Cell(1, col++).Value = Constants.COLUMN_ACTION_TYPE;
            worksheet.Cell(1, col++).Value = Constants.COLUMN_LINK_FILE;
            worksheet.Cell(1, col++).Value = Constants.COLUMN_LINK_PAGE;
            worksheet.Cell(1, col++).Value = Constants.COLUMN_DISPLAY_OPTION;
            worksheet.Cell(1, col++).Value = Constants.COLUMN_X_COORD;
            worksheet.Cell(1, col++).Value = Constants.COLUMN_Y_COORD;

            // ヘッダー行のスタイル設定
            var headerRange = worksheet.Range(1, 1, 1, col - 1);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        /// <summary>
        /// データ行を作成
        /// </summary>
        private static void CreateDataRows(IXLWorksheet worksheet, List<BookmarkExcelRow> bookmarks, int maxLevel)
        {
            int row = 2;

            foreach (var bookmark in bookmarks)
            {
                int col = 1;

                // レベル
                worksheet.Cell(row, col++).Value = bookmark.Level;

                // 各レベルのタイトル列（該当レベルにのみタイトルを設定）
                for (int level = 1; level <= maxLevel; level++)
                {
                    if (level == bookmark.Level)
                    {
                        worksheet.Cell(row, col).Value = bookmark.Title ?? "";
                    }
                    col++;
                }

                // その他のデータ
                worksheet.Cell(row, col++).Value = bookmark.ActionType ?? "";
                worksheet.Cell(row, col++).Value = bookmark.LinkFile ?? "";
                worksheet.Cell(row, col++).Value = bookmark.LinkPage ?? "";
                worksheet.Cell(row, col++).Value = bookmark.DisplayOption ?? "";
                worksheet.Cell(row, col++).Value = bookmark.XCoord ?? "";
                worksheet.Cell(row, col++).Value = bookmark.YCoord ?? "";

                // レベルに応じた行のスタイル設定
                SetRowStyle(worksheet, row, bookmark.Level, col - 1);

                row++;
            }
        }

        /// <summary>
        /// 行のスタイルを設定
        /// </summary>
        private static void SetRowStyle(IXLWorksheet worksheet, int row, int level, int maxCol)
        {
            var rowRange = worksheet.Range(row, 1, row, maxCol);

            // レベルに応じた色設定
            switch (level)
            {
                case 1:
                    rowRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    rowRange.Style.Font.Bold = true;
                    break;
                case 2:
                    rowRange.Style.Fill.BackgroundColor = XLColor.LightCyan;
                    break;
                case 3:
                    rowRange.Style.Fill.BackgroundColor = XLColor.LightYellow;
                    break;
                default:
                    // レベル4以上は通常のスタイル
                    break;
            }

            // 境界線
            rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        /// <summary>
        /// Excelファイルからブックマークを読み込み
        /// </summary>
        public static List<BookmarkExcelRow> LoadBookmarksFromExcel(string excelPath)
        {
            var bookmarks = new List<BookmarkExcelRow>();

            if (!File.Exists(excelPath))
            {
                Logger.Error($"Excelファイルが見つかりません: {excelPath}");
                return bookmarks;
            }

            try
            {
                using var workbook = new XLWorkbook(excelPath);
                var worksheet = workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    Logger.Warn("Excelファイルにワークシートが見つかりません");
                    return bookmarks;
                }

                // ヘッダー行から列位置を特定
                var columnMapping = GetColumnMapping(worksheet);

                // データ行を読み込み
                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
                
                for (int row = 2; row <= lastRow; row++)
                {
                    var bookmark = CreateBookmarkFromRow(worksheet, row, columnMapping);
                    if (bookmark != null)
                    {
                        bookmarks.Add(bookmark);
                    }
                }

                Logger.Info($"Excelファイルから{bookmarks.Count}個のブックマークを読み込みました: {Path.GetFileName(excelPath)}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Excel読み込みエラー: {excelPath}");
            }

            return bookmarks;
        }

        /// <summary>
        /// ヘッダー行から列マッピングを取得
        /// </summary>
        private static Dictionary<string, int> GetColumnMapping(IXLWorksheet worksheet)
        {
            var mapping = new Dictionary<string, int>();
            var lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 1;

            for (int col = 1; col <= lastCol; col++)
            {
                var headerValue = worksheet.Cell(1, col).GetString().Trim();
                if (!string.IsNullOrEmpty(headerValue))
                {
                    mapping[headerValue] = col;
                }
            }

            return mapping;
        }

        /// <summary>
        /// 行からブックマークオブジェクトを作成
        /// </summary>
        private static BookmarkExcelRow? CreateBookmarkFromRow(IXLWorksheet worksheet, int row, Dictionary<string, int> columnMapping)
        {
            try
            {
                var bookmark = new BookmarkExcelRow();

                // レベル
                if (columnMapping.TryGetValue(Constants.COLUMN_LEVEL, out int levelCol))
                {
                    bookmark.Level = worksheet.Cell(row, levelCol).GetValue<int>();
                }

                // タイトル（各レベル列から検索）
                for (int level = 1; level <= 10; level++) // 最大10レベルまでチェック
                {
                    var levelColName = $"{Constants.COLUMN_PREFIX_LEVEL}{level}{Constants.COLUMN_LEVEL_TITLE_SUFFIX}";
                    if (columnMapping.TryGetValue(levelColName, out int titleCol))
                    {
                        var title = worksheet.Cell(row, titleCol).GetString().Trim();
                        if (!string.IsNullOrEmpty(title))
                        {
                            bookmark.Title = title;
                            break; // 最初に見つかったタイトルを使用
                        }
                    }
                }

                // その他のプロパティ
                if (columnMapping.TryGetValue(Constants.COLUMN_ACTION_TYPE, out int actionCol))
                    bookmark.ActionType = worksheet.Cell(row, actionCol).GetString().Trim();

                if (columnMapping.TryGetValue(Constants.COLUMN_LINK_FILE, out int linkFileCol))
                    bookmark.LinkFile = worksheet.Cell(row, linkFileCol).GetString().Trim();

                if (columnMapping.TryGetValue(Constants.COLUMN_LINK_PAGE, out int linkPageCol))
                    bookmark.LinkPage = worksheet.Cell(row, linkPageCol).GetString().Trim();

                if (columnMapping.TryGetValue(Constants.COLUMN_DISPLAY_OPTION, out int displayCol))
                    bookmark.DisplayOption = worksheet.Cell(row, displayCol).GetString().Trim();

                if (columnMapping.TryGetValue(Constants.COLUMN_X_COORD, out int xCol))
                    bookmark.XCoord = worksheet.Cell(row, xCol).GetString().Trim();

                if (columnMapping.TryGetValue(Constants.COLUMN_Y_COORD, out int yCol))
                    bookmark.YCoord = worksheet.Cell(row, yCol).GetString().Trim();

                // 最低限タイトルまたはレベルが存在する場合のみ有効とする
                if (!string.IsNullOrEmpty(bookmark.Title) || bookmark.Level > 0)
                {
                    return bookmark;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"行{row}の読み込みをスキップしました");
            }

            return null;
        }
    }
} 