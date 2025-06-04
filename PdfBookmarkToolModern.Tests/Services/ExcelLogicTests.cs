using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using PdfBookmarkToolModern.Models;
using PdfBookmarkToolModern.Services;

namespace PdfBookmarkToolModern.Tests.Services
{
    /// <summary>
    /// ExcelLogicのテストクラス
    /// </summary>
    public class ExcelLogicTests : IDisposable
    {
        private readonly string _tempDirectory;

        public ExcelLogicTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "ExcelLogicTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        #region SaveBookmarksToExcelテスト

        [Fact]
        public void SaveBookmarksToExcel_NullExcelPath_ShouldReturnFalse()
        {
            // Arrange
            var bookmarks = new List<BookmarkExcelRow>
            {
                new BookmarkExcelRow { Title = "Test", Level = 1 }
            };

            // Act
            var result = ExcelLogic.SaveBookmarksToExcel(null!, bookmarks, 1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SaveBookmarksToExcel_EmptyBookmarks_ShouldReturnFalse()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                var result = ExcelLogic.SaveBookmarksToExcel(tempFile, new List<BookmarkExcelRow>(), 0);

                // Assert
                Assert.False(result);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void SaveBookmarksToExcel_NullBookmarks_ShouldReturnFalse()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                var result = ExcelLogic.SaveBookmarksToExcel(tempFile, null!, 1);

                // Assert
                Assert.False(result);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public Task SaveBookmarksToExcel_ValidData_ShouldCreateFile()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var bookmarks = new List<BookmarkExcelRow>
            {
                new BookmarkExcelRow { Title = "Chapter 1", Level = 1, ActionType = "GoTo", LinkPage = "1" },
                new BookmarkExcelRow { Title = "Section 1.1", Level = 2, ActionType = "GoTo", LinkPage = "2" }
            };

            try
            {
                // Act
                var result = ExcelLogic.SaveBookmarksToExcel(tempFile, bookmarks, 2);

                // Assert
                Assert.True(result);
                Assert.True(File.Exists(tempFile));
                Assert.True(new FileInfo(tempFile).Length > 0);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }

            return Task.CompletedTask;
        }

        [Fact]
        public Task SaveBookmarksToExcel_InvalidPath_ShouldReturnFalse()
        {
            // Arrange
            var invalidPath = "\\\\invalid\\path\\file.xlsx";
            var bookmarks = new List<BookmarkExcelRow>
            {
                new BookmarkExcelRow { Title = "Test", Level = 1 }
            };

            // Act
            var result = ExcelLogic.SaveBookmarksToExcel(invalidPath, bookmarks, 1);

            // Assert
            Assert.False(result);

            return Task.CompletedTask;
        }

        [Fact]
        public Task SaveBookmarksToExcel_LargeDataSet_ShouldHandleCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var bookmarks = new List<BookmarkExcelRow>();

            // 大量のデータを生成
            for (int i = 1; i <= 1000; i++)
            {
                bookmarks.Add(new BookmarkExcelRow
                {
                    Title = $"Bookmark {i}",
                    Level = (i % 5) + 1,
                    ActionType = "GoTo",
                    LinkPage = i.ToString()
                });
            }

            try
            {
                // Act
                var result = ExcelLogic.SaveBookmarksToExcel(tempFile, bookmarks, 5);

                // Assert
                Assert.True(result);
                Assert.True(File.Exists(tempFile));

                // ファイルサイズが妥当かチェック
                var fileInfo = new FileInfo(tempFile);
                Assert.True(fileInfo.Length > 10000); // 最低10KB以上
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }

            return Task.CompletedTask;
        }

        #endregion

        #region LoadBookmarksFromExcelテスト

        [Fact]
        public void LoadBookmarksFromExcel_NonExistentFile_ShouldReturnEmptyList()
        {
            // Act
            var result = ExcelLogic.LoadBookmarksFromExcel("nonexistent.xlsx");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void LoadBookmarksFromExcel_EmptyFile_ShouldReturnEmptyList()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, ""); // 空ファイル

            try
            {
                // Act
                var result = ExcelLogic.LoadBookmarksFromExcel(tempFile);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadBookmarksFromExcel_InvalidFormat_ShouldReturnEmptyList()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "Invalid Excel content");

            try
            {
                // Act
                var result = ExcelLogic.LoadBookmarksFromExcel(tempFile);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadBookmarksFromExcel_NullPath_ShouldReturnEmptyList()
        {
            // Act
            var result = ExcelLogic.LoadBookmarksFromExcel(null!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region 統合テスト

        [Fact]
        public Task SaveAndLoad_RoundTrip_ShouldPreserveData()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var originalBookmarks = new List<BookmarkExcelRow>
            {
                new BookmarkExcelRow 
                { 
                    Title = "Chapter 1", 
                    Level = 1, 
                    ActionType = "GoTo", 
                    LinkPage = "1",
                    DisplayOption = "XYZ",
                    XCoord = "100",
                    YCoord = "200"
                },
                new BookmarkExcelRow 
                { 
                    Title = "Section 1.1", 
                    Level = 2, 
                    ActionType = "GoToR", 
                    LinkFile = "external.pdf",
                    LinkPage = "5"
                }
            };

            try
            {
                // Act - 保存
                var saveResult = ExcelLogic.SaveBookmarksToExcel(tempFile, originalBookmarks, 2);
                var loadedBookmarks = ExcelLogic.LoadBookmarksFromExcel(tempFile);

                // Assert
                Assert.True(saveResult);
                Assert.NotNull(loadedBookmarks);
                Assert.Equal(originalBookmarks.Count, loadedBookmarks.Count);

                // 最初のブックマークの詳細チェック
                Assert.Equal(originalBookmarks[0].Title, loadedBookmarks[0].Title);
                Assert.Equal(originalBookmarks[0].Level, loadedBookmarks[0].Level);
                Assert.Equal(originalBookmarks[0].ActionType, loadedBookmarks[0].ActionType);
                Assert.Equal(originalBookmarks[0].LinkPage, loadedBookmarks[0].LinkPage);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }

            return Task.CompletedTask;
        }

        [Fact]
        public Task SaveAndLoad_ComplexHierarchy_ShouldPreserveStructure()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var bookmarks = new List<BookmarkExcelRow>();

            // 複雑な階層構造を作成
            for (int chapter = 1; chapter <= 3; chapter++)
            {
                bookmarks.Add(new BookmarkExcelRow 
                { 
                    Title = $"Chapter {chapter}", 
                    Level = 1, 
                    ActionType = "GoTo", 
                    LinkPage = chapter.ToString() 
                });

                for (int section = 1; section <= 2; section++)
                {
                    bookmarks.Add(new BookmarkExcelRow 
                    { 
                        Title = $"Section {chapter}.{section}", 
                        Level = 2, 
                        ActionType = "GoTo", 
                        LinkPage = $"{chapter}{section}" 
                    });
                }
            }

            try
            {
                // Act
                var saveResult = ExcelLogic.SaveBookmarksToExcel(tempFile, bookmarks, 2);
                var loadedBookmarks = ExcelLogic.LoadBookmarksFromExcel(tempFile);

                // Assert
                Assert.True(saveResult);
                Assert.Equal(9, loadedBookmarks.Count); // 3章 + 6節

                // 階層構造の確認
                var level1Count = loadedBookmarks.Count(b => b.Level == 1);
                var level2Count = loadedBookmarks.Count(b => b.Level == 2);
                Assert.Equal(3, level1Count);
                Assert.Equal(6, level2Count);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }

            return Task.CompletedTask;
        }

        #endregion

        #region パフォーマンステスト

        [Fact]
        public Task SaveBookmarksToExcel_LargeDataSet_ShouldCompleteInReasonableTime()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var bookmarks = new List<BookmarkExcelRow>();

            for (int i = 1; i <= 5000; i++)
            {
                bookmarks.Add(new BookmarkExcelRow
                {
                    Title = $"Bookmark {i}",
                    Level = (i % 3) + 1,
                    ActionType = "GoTo",
                    LinkPage = i.ToString(),
                    DisplayOption = "XYZ",
                    XCoord = (i * 10).ToString(),
                    YCoord = (i * 20).ToString()
                });
            }

            try
            {
                var startTime = DateTime.Now;

                // Act
                var result = ExcelLogic.SaveBookmarksToExcel(tempFile, bookmarks, 3);

                var elapsed = DateTime.Now - startTime;

                // Assert
                Assert.True(result);
                Assert.True(elapsed.TotalSeconds < 10, $"処理時間が長すぎます: {elapsed.TotalSeconds}秒");
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }

            return Task.CompletedTask;
        }

        #endregion

        #region エラーハンドリングテスト

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public Task SaveBookmarksToExcel_EmptyPath_ShouldReturnFalse(string path)
        {
            // Arrange
            var bookmarks = new List<BookmarkExcelRow>
            {
                new BookmarkExcelRow { Title = "Test", Level = 1 }
            };

            // Act
            var result = ExcelLogic.SaveBookmarksToExcel(path, bookmarks, 1);

            // Assert
            Assert.False(result);

            return Task.CompletedTask;
        }

        [Fact]
        public Task LoadBookmarksFromExcel_CorruptedFile_ShouldReturnEmptyList()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            
            // 破損したExcelファイルのシミュレーション（バイナリデータ）
            var corruptedData = new byte[] { 0x50, 0x4B, 0x03, 0x04, 0xFF, 0xFF, 0xFF, 0xFF };
            File.WriteAllBytes(tempFile, corruptedData);

            try
            {
                // Act
                var result = ExcelLogic.LoadBookmarksFromExcel(tempFile);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }

            return Task.CompletedTask;
        }

        #endregion

        #region 境界値テスト

        [Fact]
        public Task SaveBookmarksToExcel_MaxLevelValue_ShouldHandleCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var bookmarks = new List<BookmarkExcelRow>
            {
                new BookmarkExcelRow { Title = "Deep Level", Level = int.MaxValue, ActionType = "GoTo", LinkPage = "1" }
            };

            try
            {
                // Act
                var result = ExcelLogic.SaveBookmarksToExcel(tempFile, bookmarks, int.MaxValue);

                // Assert
                Assert.True(result);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }

            return Task.CompletedTask;
        }

        [Fact]
        public Task SaveBookmarksToExcel_EmptyStrings_ShouldHandleCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var bookmarks = new List<BookmarkExcelRow>
            {
                new BookmarkExcelRow 
                { 
                    Title = "", 
                    Level = 1, 
                    ActionType = "", 
                    LinkPage = "",
                    DisplayOption = "",
                    XCoord = "",
                    YCoord = "",
                    LinkFile = ""
                }
            };

            try
            {
                // Act
                var result = ExcelLogic.SaveBookmarksToExcel(tempFile, bookmarks, 1);

                // Assert
                Assert.True(result);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }

            return Task.CompletedTask;
        }

        #endregion
    }
} 