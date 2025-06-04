using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using PdfBookmarkToolModern.Models;
using PdfBookmarkToolModern.Services;

namespace PdfBookmarkToolModern.Tests.Services
{
    /// <summary>
    /// PdfLogicのテストクラス
    /// </summary>
    public class PdfLogicTests
    {
        #region ExtractBookmarksテスト

        [Fact]
        public void ExtractBookmarks_NullFilePath_ShouldReturnEmptyResult()
        {
            // Act
            var result = PdfLogic.ExtractBookmarks(null!);
            
            // Assert
            Assert.Empty(result.Rows);
            Assert.Equal(0, result.MaxLevel);
        }

        [Fact]
        public void ExtractBookmarks_EmptyFilePath_ShouldReturnEmptyResult()
        {
            // Act
            var result = PdfLogic.ExtractBookmarks("");
            
            // Assert
            Assert.Empty(result.Rows);
            Assert.Equal(0, result.MaxLevel);
        }

        [Fact]
        public void ExtractBookmarks_NonExistentFile_ShouldReturnEmptyResult()
        {
            // Arrange
            var nonExistentPath = "C:\\NonExistent\\File.pdf";

            // Act
            var result = PdfLogic.ExtractBookmarks(nonExistentPath);
            
            // Assert
            Assert.Empty(result.Rows);
            Assert.Equal(0, result.MaxLevel);
        }

        [Fact]
        public void ExtractBookmarks_InvalidPdfFile_ShouldReturnEmptyResult()
        {
            // Arrange - テキストファイルをPDFとして渡す
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "This is not a PDF file");

            try
            {
                // Act
                var result = PdfLogic.ExtractBookmarks(tempFile);
                
                // Assert
                Assert.Empty(result.Rows);
                Assert.Equal(0, result.MaxLevel);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        #endregion

        #region ImportBookmarksToPdfテスト

        [Fact]
        public void ImportBookmarksToPdf_NullInputPath_ShouldReturnFalse()
        {
            // Arrange
            var bookmarks = new List<BookmarkEntry>();
            
            // Act
            var result = PdfLogic.ImportBookmarksToPdf(null!, bookmarks, "output.pdf");
            
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ImportBookmarksToPdf_NullOutputPath_ShouldReturnFalse()
        {
            // Arrange
            var bookmarks = new List<BookmarkEntry>();
            
            // Act
            var result = PdfLogic.ImportBookmarksToPdf("input.pdf", bookmarks, null!);
            
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ImportBookmarksToPdf_NullBookmarks_ShouldReturnFalse()
        {
            // Act
            var result = PdfLogic.ImportBookmarksToPdf("input.pdf", null!, "output.pdf");
            
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ImportBookmarksToPdf_EmptyBookmarks_ShouldReturnFalse()
        {
            // Arrange
            var tempInputFile = Path.GetTempFileName();
            var tempOutputFile = Path.GetTempFileName();
            var emptyBookmarks = new List<BookmarkEntry>();

            try
            {
                // 空のPDFファイルを作成（実際のテストでは有効なPDFが必要）
                File.WriteAllText(tempInputFile, "%PDF-1.4\n%%EOF");

                // Act
                var result = PdfLogic.ImportBookmarksToPdf(tempInputFile, emptyBookmarks, tempOutputFile);
                
                // Assert - 入力ファイルが有効でないためfalseが返される
                Assert.False(result);
            }
            finally
            {
                if (File.Exists(tempInputFile)) File.Delete(tempInputFile);
                if (File.Exists(tempOutputFile)) File.Delete(tempOutputFile);
            }
        }

        #endregion

        #region BookmarkEntry統合テスト

        [Fact]
        public void BookmarkEntry_Properties_ShouldSetAndGetCorrectly()
        {
            // Arrange
            var bookmark = new BookmarkEntry
            {
                Title = "Test Bookmark",
                Level = 2,
                ActionType = "GoTo",
                LinkPage = "5",
                DisplayOption = "XYZ",
                XCoord = "100",
                YCoord = "200",
                LinkFile = "external.pdf"
            };

            // Act & Assert
            Assert.Equal("Test Bookmark", bookmark.Title);
            Assert.Equal(2, bookmark.Level);
            Assert.Equal("GoTo", bookmark.ActionType);
            Assert.Equal("5", bookmark.LinkPage);
            Assert.Equal("XYZ", bookmark.DisplayOption);
            Assert.Equal("100", bookmark.XCoord);
            Assert.Equal("200", bookmark.YCoord);
            Assert.Equal("external.pdf", bookmark.LinkFile);
            Assert.NotNull(bookmark.Children);
            Assert.Empty(bookmark.Children);
        }

        [Fact]
        public void BookmarkEntry_ChildrenHierarchy_ShouldWorkCorrectly()
        {
            // Arrange
            var parent = new BookmarkEntry { Title = "Parent", Level = 1 };
            var child1 = new BookmarkEntry { Title = "Child 1", Level = 2 };
            var child2 = new BookmarkEntry { Title = "Child 2", Level = 2 };
            var grandChild = new BookmarkEntry { Title = "Grand Child", Level = 3 };

            // Act
            child1.Children.Add(grandChild);
            parent.Children.Add(child1);
            parent.Children.Add(child2);

            // Assert
            Assert.Equal(2, parent.Children.Count);
            Assert.Equal("Child 1", parent.Children[0].Title);
            Assert.Equal("Child 2", parent.Children[1].Title);
            Assert.Single(parent.Children[0].Children);
            Assert.Equal("Grand Child", parent.Children[0].Children[0].Title);
            Assert.Empty(parent.Children[1].Children);
        }

        #endregion

        #region エラーハンドリングテスト

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void ExtractBookmarks_WhitespaceFilePath_ShouldReturnEmptyResult(string filePath)
        {
            // Act
            var result = PdfLogic.ExtractBookmarks(filePath);
            
            // Assert
            Assert.Empty(result.Rows);
            Assert.Equal(0, result.MaxLevel);
        }

        [Theory]
        [InlineData("invalid_extension.txt")]
        [InlineData("no_extension")]
        [InlineData("multiple.dots.txt")]
        public void ExtractBookmarks_InvalidFileExtension_ShouldReturnEmptyResult(string fileName)
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), fileName);

            // Act
            var result = PdfLogic.ExtractBookmarks(tempPath);
            
            // Assert
            Assert.Empty(result.Rows);
            Assert.Equal(0, result.MaxLevel);
        }

        #endregion

        #region パフォーマンステスト

        [Fact]
        public void BookmarkEntry_LargeHierarchy_ShouldPerformWell()
        {
            // Arrange
            var root = new BookmarkEntry { Title = "Root", Level = 1 };
            var startTime = DateTime.Now;

            // Act - 大量の子要素を追加
            for (int i = 0; i < 1000; i++)
            {
                var child = new BookmarkEntry 
                { 
                    Title = $"Child {i}", 
                    Level = 2,
                    ActionType = "GoTo",
                    LinkPage = (i + 1).ToString()
                };
                root.Children.Add(child);

                // 一部の子要素にさらに子要素を追加
                if (i % 10 == 0)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        var grandChild = new BookmarkEntry 
                        { 
                            Title = $"GrandChild {i}-{j}", 
                            Level = 3,
                            ActionType = "GoTo",
                            LinkPage = $"{i + 1}.{j + 1}"
                        };
                        child.Children.Add(grandChild);
                    }
                }
            }

            var elapsed = DateTime.Now - startTime;

            // Assert
            Assert.Equal(1000, root.Children.Count);
            Assert.True(elapsed.TotalSeconds < 1, $"処理時間が長すぎます: {elapsed.TotalSeconds}秒");
            
            // 階層構造が正しく保持されているかを確認
            Assert.Equal(5, root.Children[0].Children.Count);
            Assert.Equal(5, root.Children[10].Children.Count);
            Assert.Empty(root.Children[1].Children);
        }

        #endregion

        #region 境界値テスト

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void BookmarkEntry_NegativeOrZeroLevel_ShouldAccept(int level)
        {
            // Arrange & Act
            var bookmark = new BookmarkEntry { Level = level };

            // Assert
            Assert.Equal(level, bookmark.Level);
        }

        [Fact]
        public void BookmarkEntry_VeryLongTitle_ShouldAccept()
        {
            // Arrange
            var longTitle = new string('A', 10000);
            var bookmark = new BookmarkEntry { Title = longTitle };

            // Act & Assert
            Assert.Equal(longTitle, bookmark.Title);
            Assert.Equal(10000, bookmark.Title.Length);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void BookmarkEntry_NullOrEmptyTitle_ShouldAccept(string? title)
        {
            // Arrange & Act
            var bookmark = new BookmarkEntry { Title = title };

            // Assert
            Assert.Equal(title, bookmark.Title);
        }

        #endregion
    }
} 