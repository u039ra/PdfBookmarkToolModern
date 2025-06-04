using ClosedXML.Excel;
using System.Collections.Generic;
using System.Linq;

namespace PdfBookmarkToolModern.Services
{
    /// <summary>
    /// Excel操作用のサービスクラス
    /// </summary>
    public static class ExcelService
    {
        /// <summary>
        /// Excelファイルからブックマークを読み込む
        /// </summary>
        public static List<BookmarkExcelRow> LoadBookmarksFromExcel(string excelPath)
        {
            var rows = new List<BookmarkExcelRow>();
            
            using var workbook = new XLWorkbook(excelPath);
            var worksheet = workbook.Worksheets.First();
            var headers = worksheet.Row(1).Cells().Select(c => c.GetString()).ToList();
            
            int maxLevel = headers.Count(h => h.StartsWith("レベル") && h.EndsWith("タイトル"));
            
            for (int i = 2; i <= worksheet.LastRowUsed().RowNumber(); i++)
            {
                var row = new BookmarkExcelRow();
                
                // レベルを取得
                if (headers.Contains("レベル"))
                {
                    row.Level = worksheet.Cell(i, headers.IndexOf("レベル") + 1).GetValue<int>();
                }
                
                // タイトルを取得（各レベルから最初に見つかったものを使用）
                for (int level = 1; level <= maxLevel; level++)
                {
                    string colName = $"レベル{level}タイトル";
                    if (headers.Contains(colName))
                    {
                        string val = worksheet.Cell(i, headers.IndexOf(colName) + 1).GetString();
                        if (!string.IsNullOrEmpty(val))
                        {
                            row.Title = val;
                            break;
                        }
                    }
                }
                
                // その他のプロパティを取得
                if (headers.Contains("アクションタイプ"))
                    row.ActionType = worksheet.Cell(i, headers.IndexOf("アクションタイプ") + 1).GetString();
                
                if (headers.Contains("リンク先ファイル"))
                    row.LinkFile = worksheet.Cell(i, headers.IndexOf("リンク先ファイル") + 1).GetString();
                
                if (headers.Contains("リンク先ページ"))
                    row.LinkPage = worksheet.Cell(i, headers.IndexOf("リンク先ページ") + 1).GetString();
                
                if (headers.Contains("表示オプション"))
                    row.DisplayOption = worksheet.Cell(i, headers.IndexOf("表示オプション") + 1).GetString();
                
                if (headers.Contains("X座標"))
                    row.XCoord = worksheet.Cell(i, headers.IndexOf("X座標") + 1).GetString();
                
                if (headers.Contains("Y座標"))
                    row.YCoord = worksheet.Cell(i, headers.IndexOf("Y座標") + 1).GetString();
                
                rows.Add(row);
            }
            
            return rows;
        }
    }
} 