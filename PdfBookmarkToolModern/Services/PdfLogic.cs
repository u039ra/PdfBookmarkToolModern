using NLog;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Advanced;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfBookmarkToolModern.Services
{
    /// <summary>
    /// PDFしおり処理のメインロジック（PDFsharp 6.2.0対応）
    /// </summary>
    public static class PdfLogic
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// PDFファイルにしおりが存在するかチェック
        /// </summary>
        public static bool HasBookmarks(string pdfPath)
        {
            if (!File.Exists(pdfPath))
            {
                Logger.Error($"ファイル '{pdfPath}' が見つかりません。");
                return false;
            }

            try
            {
                using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
                var hasBookmarks = document.Outlines.Count > 0;
                Logger.Info($"'{Path.GetFileName(pdfPath)}': しおり {(hasBookmarks ? "あり" : "なし")}");
                return hasBookmarks;
            }
            catch (PdfReaderException ex)
            {
                Logger.Error(ex, $"PDF読み込みエラー: {pdfPath}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"予期せぬエラー: {pdfPath}");
                return false;
            }
        }

        /// <summary>
        /// PDFからしおりを抽出してフラットなリストとして返す
        /// </summary>
        public static (List<BookmarkExcelRow> Rows, int MaxLevel) ExtractBookmarks(string pdfPath)
        {
            var extractedRows = new List<BookmarkExcelRow>();
            int maxLevel = 0;

            if (!File.Exists(pdfPath))
            {
                Logger.Error($"ファイル '{pdfPath}' が見つかりません。");
                return (extractedRows, maxLevel);
            }

            try
            {
                using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
                
                if (document.Outlines.Count == 0)
                {
                    Logger.Info($"'{Path.GetFileName(pdfPath)}': しおりがありません。");
                    return (extractedRows, maxLevel);
                }

                ExtractOutlinesRecursive(document.Outlines, 1, document.Pages, extractedRows, ref maxLevel);
                Logger.Info($"'{Path.GetFileName(pdfPath)}': {extractedRows.Count}個のしおりを抽出、最大レベル:{maxLevel}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"しおり抽出エラー: {pdfPath}");
            }

            return (extractedRows, maxLevel);
        }

        /// <summary>
        /// アウトラインを再帰的に抽出
        /// </summary>
        private static void ExtractOutlinesRecursive(
            PdfOutlineCollection outlines, 
            int currentLevel, 
            PdfPages pages,
            List<BookmarkExcelRow> extractedRows, 
            ref int maxLevel)
        {
            if (currentLevel > maxLevel) 
                maxLevel = currentLevel;

            foreach (PdfOutline outline in outlines)
            {
                var row = new BookmarkExcelRow
                {
                    Level = currentLevel,
                    Title = !string.IsNullOrEmpty(outline.Title) ? outline.Title : Constants.DEFAULT_BOOKMARK_TITLE
                };

                // アクション詳細を解析
                ParseOutlineAction(outline, row, pages);
                extractedRows.Add(row);

                // 子要素を再帰処理
                if (outline.Outlines.Count > 0)
                {
                    ExtractOutlinesRecursive(outline.Outlines, currentLevel + 1, pages, extractedRows, ref maxLevel);
                }
            }
        }

        /// <summary>
        /// アウトラインのアクション情報を解析
        /// </summary>
        private static void ParseOutlineAction(PdfOutline outline, BookmarkExcelRow row, PdfPages pages)
        {
            try
            {
                // /A エントリ（アクション）をチェック
                var actionItem = outline.Elements.GetValue("/A");
                if (actionItem != null)
                {
                    var actionDict = GetDictionary(actionItem);
                    if (actionDict != null)
                    {
                        ParseActionDictionary(actionDict, row, pages);
                        return;
                    }
                }

                // /Dest エントリ（直接デスティネーション）をチェック
                var destItem = outline.Elements.GetValue("/Dest");
                if (destItem != null)
                {
                    row.ActionType = Constants.PDF_ACTION_GOTO;
                    ParseDestination(destItem, row, pages);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"アクション解析エラー: {outline.Title}");
            }
        }

        /// <summary>
        /// アクション辞書を解析
        /// </summary>
        private static void ParseActionDictionary(PdfDictionary actionDict, BookmarkExcelRow row, PdfPages pages)
        {
            var actionTypeItem = actionDict.Elements.GetValue("/S");
            if (actionTypeItem is PdfName actionTypeName)
            {
                row.ActionType = actionTypeName.Value.TrimStart('/');

                if (row.ActionType == Constants.PDF_ACTION_GOTO)
                {
                    var destItem = actionDict.Elements.GetValue("/D");
                    if (destItem != null)
                    {
                        ParseDestination(destItem, row, pages);
                    }
                }
                else if (row.ActionType == Constants.PDF_ACTION_GOTOR)
                {
                    ParseGoToRAction(actionDict, row, pages);
                }
            }
        }

        /// <summary>
        /// デスティネーション情報を解析
        /// </summary>
        private static void ParseDestination(PdfItem destItem, BookmarkExcelRow row, PdfPages pages)
        {
            if (destItem is PdfArray destArray && destArray.Elements.Count > 0)
            {
                // ページ参照を取得
                var pageRefItem = destArray.Elements[0];
                if (pageRefItem is PdfReference pageRef && pageRef.Value is PdfPage targetPage)
                {
                    // ページ番号を検索
                    for (int i = 0; i < pages.Count; i++)
                    {
                        if (pages[i].Equals(targetPage))
                        {
                            row.LinkPage = (i + 1).ToString();
                            break;
                        }
                    }
                }
                else if (pageRefItem is PdfInteger pageIndex)
                {
                    row.LinkPage = (pageIndex.Value + 1).ToString();
                }

                // 表示オプションを解析
                if (destArray.Elements.Count > 1 && destArray.Elements[1] is PdfName displayOption)
                {
                    row.DisplayOption = displayOption.Value.TrimStart('/');
                    
                    // XYZ の場合は座標も取得
                    if (row.DisplayOption == Constants.PDF_ACTION_XYZ && destArray.Elements.Count > 3)
                    {
                        row.XCoord = destArray.Elements[2]?.ToString() ?? "";
                        row.YCoord = destArray.Elements[3]?.ToString() ?? "";
                    }
                }
            }
            else if (destItem is PdfName namedDest)
            {
                row.LinkPage = Constants.NAMED_DESTINATION_PLACEHOLDER;
                row.DisplayOption = namedDest.Value.TrimStart('/');
            }
            else if (destItem is PdfString namedDestStr)
            {
                row.LinkPage = Constants.NAMED_DESTINATION_PLACEHOLDER;
                row.DisplayOption = namedDestStr.Value;
            }
        }

        /// <summary>
        /// GoToR アクションを解析
        /// </summary>
        private static void ParseGoToRAction(PdfDictionary actionDict, BookmarkExcelRow row, PdfPages pages)
        {
            // ファイル指定を取得
            var fileSpecItem = actionDict.Elements.GetValue("/F");
            if (fileSpecItem != null)
            {
                if (fileSpecItem is PdfString fileString)
                {
                    row.LinkFile = fileString.Value;
                }
                else
                {
                    var fileSpecDict = GetDictionary(fileSpecItem);
                    if (fileSpecDict != null)
                    {
                        var unicodeFile = fileSpecDict.Elements.GetValue("/UF");
                        var regularFile = fileSpecDict.Elements.GetValue("/F");
                        
                        if (unicodeFile is PdfString uniStr)
                            row.LinkFile = uniStr.Value;
                        else if (regularFile is PdfString regStr)
                            row.LinkFile = regStr.Value;
                    }
                }
            }

            // デスティネーションを解析
            var destItem = actionDict.Elements.GetValue("/D");
            if (destItem != null)
            {
                ParseDestination(destItem, row, pages);
            }
        }

        /// <summary>
        /// PdfItemからPdfDictionaryを安全に取得
        /// </summary>
        private static PdfDictionary? GetDictionary(PdfItem item)
        {
            return item switch
            {
                PdfDictionary dict => dict,
                PdfReference reference when reference.Value is PdfDictionary refDict => refDict,
                _ => null
            };
        }

        /// <summary>
        /// フラットなブックマークリストから階層構造を構築
        /// </summary>
        public static List<BookmarkEntry> BuildBookmarkTree(List<BookmarkExcelRow> flatBookmarks)
        {
            var rootEntries = new List<BookmarkEntry>();
            var stack = new Stack<BookmarkEntry>();

            foreach (var row in flatBookmarks)
            {
                var entry = row.ToBookmarkEntry();

                // スタックから適切な親を見つける
                while (stack.Count > 0 && stack.Peek().Level >= entry.Level)
                {
                    stack.Pop();
                }

                if (stack.Count == 0)
                {
                    // ルートレベル
                    rootEntries.Add(entry);
                }
                else
                {
                    // 親の子として追加
                    stack.Peek().Children.Add(entry);
                }

                stack.Push(entry);
            }

            return rootEntries;
        }

        /// <summary>
        /// PDFファイルにしおりをインポート
        /// </summary>
        public static bool ImportBookmarksToPdf(string inputPath, List<BookmarkEntry> bookmarks, string outputPath, bool expandBookmarks = false)
        {
            if (!File.Exists(inputPath))
            {
                Logger.Error($"入力ファイルが見つかりません: {inputPath}");
                return false;
            }

            try
            {
                using var document = PdfReader.Open(inputPath, PdfDocumentOpenMode.Modify);
                
                // 既存のしおりをクリア
                document.Outlines.Clear();
                
                // 新しいしおりを追加
                AddBookmarksRecursive(document.Outlines, bookmarks, document, expandBookmarks);
                
                // ブックマーク展開設定
                if (expandBookmarks && bookmarks.Any())
                {
                    document.Internals.Catalog.Elements["/PageMode"] = new PdfName("/UseOutlines");
                }
                
                document.Save(outputPath);
                Logger.Info($"しおりを適用しました: '{Path.GetFileName(outputPath)}'");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"しおりインポートエラー: {inputPath}");
                return false;
            }
        }

        /// <summary>
        /// ブックマークを再帰的に追加
        /// </summary>
        private static void AddBookmarksRecursive(
            PdfOutlineCollection parentOutlines, 
            List<BookmarkEntry> bookmarks, 
            PdfDocument document, 
            bool expandBookmarks)
        {
            foreach (var bookmark in bookmarks)
            {
                var outline = new PdfOutline
                {
                    Title = bookmark.Title ?? Constants.DEFAULT_BOOKMARK_TITLE
                };

                // アクションを作成
                var actionDict = CreateActionDictionary(bookmark, document);
                if (actionDict != null)
                {
                    outline.Elements["/A"] = actionDict;
                }

                parentOutlines.Add(outline);

                // 子ブックマークを追加
                if (bookmark.Children.Any())
                {
                    AddBookmarksRecursive(outline.Outlines, bookmark.Children, document, expandBookmarks);
                    
                    if (expandBookmarks)
                    {
                        outline.Elements["/Count"] = new PdfInteger(bookmark.Children.Count);
                    }
                }
            }
        }

        /// <summary>
        /// ブックマークエントリからアクション辞書を作成
        /// </summary>
        private static PdfDictionary? CreateActionDictionary(BookmarkEntry bookmark, PdfDocument document)
        {
            if (string.IsNullOrEmpty(bookmark.ActionType))
                return null;

            var actionDict = new PdfDictionary(document);
            actionDict.Elements["/S"] = new PdfName($"/{bookmark.ActionType}");

            try
            {
                if (bookmark.ActionType == Constants.PDF_ACTION_GOTO)
                {
                    var destItem = CreateDestinationItem(bookmark, document, false);
                    if (destItem != null)
                    {
                        actionDict.Elements["/D"] = destItem;
                        return actionDict;
                    }
                }
                else if (bookmark.ActionType == Constants.PDF_ACTION_GOTOR)
                {
                    if (!string.IsNullOrWhiteSpace(bookmark.LinkFile))
                    {
                        actionDict.Elements["/F"] = new PdfString(bookmark.LinkFile.Replace("\\", "/"));
                        
                        var destItem = CreateDestinationItem(bookmark, document, true);
                        if (destItem != null)
                        {
                            actionDict.Elements["/D"] = destItem;
                        }
                        
                        return actionDict;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"アクション辞書作成エラー: {bookmark.Title}");
            }

            return null;
        }

        /// <summary>
        /// デスティネーション項目を作成
        /// </summary>
        private static PdfItem? CreateDestinationItem(BookmarkEntry bookmark, PdfDocument document, bool isRemote)
        {
            var destElements = new List<PdfItem>();

            // ページ指定を処理
            if (!string.IsNullOrWhiteSpace(bookmark.LinkPage))
            {
                if (bookmark.LinkPage == Constants.NAMED_DESTINATION_PLACEHOLDER && 
                    !string.IsNullOrWhiteSpace(bookmark.DisplayOption))
                {
                    if (!isRemote)
                        return new PdfName($"/{bookmark.DisplayOption}");
                    
                    destElements.Add(new PdfString(bookmark.DisplayOption));
                }
                else if (int.TryParse(bookmark.LinkPage, out int pageNum))
                {
                    int pageIndex = pageNum - 1;
                    if (pageIndex >= 0 && pageIndex < document.PageCount)
                    {
                        destElements.Add(document.Pages[pageIndex].Reference!);
                    }
                    else if (isRemote)
                    {
                        destElements.Add(new PdfString(bookmark.LinkPage));
                    }
                    else
                    {
                        Logger.Warn($"ページ番号範囲外: {bookmark.LinkPage} ({bookmark.Title})");
                        return null;
                    }
                }
                else
                {
                    destElements.Add(new PdfString(bookmark.LinkPage));
                }
            }
            else if (!isRemote)
            {
                Logger.Warn($"ページ指定なし: {bookmark.Title}");
                return null;
            }

            // 表示オプションを追加
            if (!string.IsNullOrWhiteSpace(bookmark.DisplayOption) && 
                bookmark.DisplayOption != Constants.NAMED_DESTINATION_PLACEHOLDER)
            {
                destElements.Add(new PdfName($"/{bookmark.DisplayOption}"));
                
                if (bookmark.DisplayOption == Constants.PDF_ACTION_XYZ)
                {
                    destElements.Add(double.TryParse(bookmark.XCoord, out double x) ? new PdfReal(x) : PdfNull.Value);
                    destElements.Add(double.TryParse(bookmark.YCoord, out double y) ? new PdfReal(y) : PdfNull.Value);
                    destElements.Add(PdfNull.Value); // zoom
                }
            }

            return destElements.Any() ? new PdfArray(document, destElements.ToArray()) : null;
        }
    }
} 