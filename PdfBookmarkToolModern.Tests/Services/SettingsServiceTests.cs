using System;
using System.IO;
using Xunit;
using PdfBookmarkToolModern.Models;
using PdfBookmarkToolModern.Services;

namespace PdfBookmarkToolModern.Tests.Services
{
    /// <summary>
    /// SettingsServiceのテストクラス
    /// </summary>
    public class SettingsServiceTests : IDisposable
    {
        public SettingsServiceTests()
        {
            // 各テスト前に設定ファイルをクリーンアップ
            CleanupSettingsFile();
        }

        public void Dispose()
        {
            // 各テスト後に設定ファイルをクリーンアップ
            CleanupSettingsFile();
        }

        private void CleanupSettingsFile()
        {
            try
            {
                var settingsPath = SettingsService.GetSettingsFilePath();
                if (File.Exists(settingsPath))
                {
                    File.Delete(settingsPath);
                }

                // バックアップファイルも削除
                var settingsDir = Path.GetDirectoryName(settingsPath);
                if (settingsDir != null && Directory.Exists(settingsDir))
                {
                    var backupFiles = Directory.GetFiles(settingsDir, "*.backup.*");
                    foreach (var backupFile in backupFiles)
                    {
                        try
                        {
                            File.Delete(backupFile);
                        }
                        catch
                        {
                            // バックアップファイルの削除に失敗しても続行
                        }
                    }
                }
            }
            catch
            {
                // クリーンアップの失敗は無視
            }
        }

        #region LoadSettingsテスト

        [Fact]
        public void LoadSettings_NoSettingsFile_ShouldReturnDefaultSettings()
        {
            // Act
            var settings = SettingsService.LoadSettings();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal(Theme.Light, settings.CurrentTheme);
            Assert.Equal("ja-JP", settings.Language);
            Assert.True(settings.ExpandBookmarksOnSave);
            Assert.True(settings.ShowConfirmationDialogs);
            Assert.Equal(10, settings.RecentFileCount);
            Assert.Equal("output", settings.DefaultOutputDirectory);
            Assert.True(settings.AutoBackup);
            Assert.Equal(300, settings.AutoSaveInterval);
        }

        [Fact]
        public void LoadSettings_ValidSettingsFile_ShouldLoadCorrectly()
        {
            // Arrange
            var expectedSettings = new AppSettings
            {
                CurrentTheme = Theme.Dark,
                Language = "en-US",
                ExpandBookmarksOnSave = false,
                ShowConfirmationDialogs = false,
                RecentFileCount = 5,
                DefaultOutputDirectory = "custom_output",
                AutoBackup = false,
                AutoSaveInterval = 600
            };

            // 設定を保存
            SettingsService.SaveSettings(expectedSettings);

            // Act
            var loadedSettings = SettingsService.LoadSettings();

            // Assert
            Assert.Equal(expectedSettings.CurrentTheme, loadedSettings.CurrentTheme);
            Assert.Equal(expectedSettings.Language, loadedSettings.Language);
            Assert.Equal(expectedSettings.ExpandBookmarksOnSave, loadedSettings.ExpandBookmarksOnSave);
            Assert.Equal(expectedSettings.ShowConfirmationDialogs, loadedSettings.ShowConfirmationDialogs);
            Assert.Equal(expectedSettings.RecentFileCount, loadedSettings.RecentFileCount);
            Assert.Equal(expectedSettings.DefaultOutputDirectory, loadedSettings.DefaultOutputDirectory);
            Assert.Equal(expectedSettings.AutoBackup, loadedSettings.AutoBackup);
            Assert.Equal(expectedSettings.AutoSaveInterval, loadedSettings.AutoSaveInterval);
        }

        [Fact]
        public void LoadSettings_CorruptedSettingsFile_ShouldReturnDefaultSettings()
        {
            // Arrange - 破損したJSONファイルを作成
            var settingsPath = SettingsService.GetSettingsFilePath();
            var settingsDir = Path.GetDirectoryName(settingsPath);
            if (settingsDir != null && !Directory.Exists(settingsDir))
            {
                Directory.CreateDirectory(settingsDir);
            }
            
            File.WriteAllText(settingsPath, "{ invalid json content }");

            // Act
            var settings = SettingsService.LoadSettings();

            // Assert - デフォルト設定が返されるはず
            Assert.NotNull(settings);
            Assert.Equal(Theme.Light, settings.CurrentTheme);
            Assert.Equal("ja-JP", settings.Language);
        }

        #endregion

        #region SaveSettingsテスト

        [Fact]
        public void SaveSettings_ValidSettings_ShouldSaveSuccessfully()
        {
            // Arrange
            var settings = new AppSettings
            {
                CurrentTheme = Theme.Dark,
                Language = "en-US",
                ExpandBookmarksOnSave = false,
                RecentFileCount = 15
            };

            // Act
            var result = SettingsService.SaveSettings(settings);

            // Assert
            Assert.True(result);
            
            // 実際に保存されているかを確認
            var settingsPath = SettingsService.GetSettingsFilePath();
            Assert.True(File.Exists(settingsPath));
        }

        [Fact]
        public void SaveSettings_NullSettings_ShouldReturnFalse()
        {
            // Act
            var result = SettingsService.SaveSettings(null!);
            
            // Assert
            Assert.False(result); // null設定の場合はfalseを返すはず
        }

        [Fact]
        public void SaveSettings_CreateDirectoryIfNotExists_ShouldWork()
        {
            // Arrange
            var settingsPath = SettingsService.GetSettingsFilePath();
            var settingsDir = Path.GetDirectoryName(settingsPath);
            
            // ディレクトリが存在する場合は削除
            if (settingsDir != null && Directory.Exists(settingsDir))
            {
                Directory.Delete(settingsDir, true);
            }

            var settings = new AppSettings();

            // Act
            var result = SettingsService.SaveSettings(settings);

            // Assert
            Assert.True(result);
            Assert.True(Directory.Exists(settingsDir));
            Assert.True(File.Exists(settingsPath));
        }

        #endregion

        #region ResetSettingsテスト

        [Fact]
        public void ResetSettings_ExistingSettingsFile_ShouldDeleteFile()
        {
            // Arrange - 設定ファイルを作成
            var settings = new AppSettings();
            SettingsService.SaveSettings(settings);
            var settingsPath = SettingsService.GetSettingsFilePath();
            
            Assert.True(File.Exists(settingsPath)); // ファイルが存在することを確認

            // Act
            var result = SettingsService.ResetSettings();

            // Assert
            Assert.True(result);
            Assert.False(File.Exists(settingsPath)); // ファイルが削除されていることを確認
        }

        [Fact]
        public void ResetSettings_NoSettingsFile_ShouldReturnTrue()
        {
            // Arrange - 設定ファイルが存在しないことを確認
            var settingsPath = SettingsService.GetSettingsFilePath();
            if (File.Exists(settingsPath))
            {
                File.Delete(settingsPath);
            }

            // Act
            var result = SettingsService.ResetSettings();

            // Assert
            Assert.True(result); // ファイルが存在しなくても true を返すはず
        }

        #endregion

        #region BackupSettingsテスト

        [Fact]
        public void BackupSettings_ExistingSettingsFile_ShouldCreateBackup()
        {
            // Arrange - 設定ファイルを作成
            var settings = new AppSettings { CurrentTheme = Theme.Dark };
            SettingsService.SaveSettings(settings);

            // Act
            var result = SettingsService.BackupSettings();

            // Assert
            Assert.True(result);
            
            // バックアップファイルが作成されているかを確認
            var settingsDir = Path.GetDirectoryName(SettingsService.GetSettingsFilePath());
            if (settingsDir != null)
            {
                var backupFiles = Directory.GetFiles(settingsDir, "*.backup.*");
                Assert.True(backupFiles.Length > 0);
            }
        }

        [Fact]
        public void BackupSettings_NoSettingsFile_ShouldReturnFalse()
        {
            // Arrange - 設定ファイルが存在しないことを確認
            var settingsPath = SettingsService.GetSettingsFilePath();
            if (File.Exists(settingsPath))
            {
                File.Delete(settingsPath);
            }

            // Act
            var result = SettingsService.BackupSettings();

            // Assert
            Assert.False(result); // ファイルが存在しない場合は false を返すはず
        }

        #endregion

        #region GetSettingsFilePathテスト

        [Fact]
        public void GetSettingsFilePath_ShouldReturnValidPath()
        {
            // Act
            var path = SettingsService.GetSettingsFilePath();

            // Assert
            Assert.NotNull(path);
            Assert.NotEmpty(path);
            Assert.True(Path.IsPathRooted(path)); // 絶対パスであることを確認
            Assert.EndsWith("settings.json", path);
        }

        #endregion

        #region 統合テスト

        [Fact]
        public void SaveAndLoad_RoundTrip_ShouldPreserveAllSettings()
        {
            // Arrange
            var originalSettings = new AppSettings
            {
                CurrentTheme = Theme.Auto,
                Language = "en-US",
                ExpandBookmarksOnSave = false,
                ShowConfirmationDialogs = false,
                RecentFileCount = 25,
                DefaultOutputDirectory = "/custom/path",
                AutoBackup = false,
                AutoSaveInterval = 1800
            };

            // Act
            var saveResult = SettingsService.SaveSettings(originalSettings);
            var loadedSettings = SettingsService.LoadSettings();

            // Assert
            Assert.True(saveResult);
            Assert.Equal(originalSettings.CurrentTheme, loadedSettings.CurrentTheme);
            Assert.Equal(originalSettings.Language, loadedSettings.Language);
            Assert.Equal(originalSettings.ExpandBookmarksOnSave, loadedSettings.ExpandBookmarksOnSave);
            Assert.Equal(originalSettings.ShowConfirmationDialogs, loadedSettings.ShowConfirmationDialogs);
            Assert.Equal(originalSettings.RecentFileCount, loadedSettings.RecentFileCount);
            Assert.Equal(originalSettings.DefaultOutputDirectory, loadedSettings.DefaultOutputDirectory);
            Assert.Equal(originalSettings.AutoBackup, loadedSettings.AutoBackup);
            Assert.Equal(originalSettings.AutoSaveInterval, loadedSettings.AutoSaveInterval);
        }

        [Fact]
        public void MultipleOperations_ShouldWorkCorrectly()
        {
            // Arrange
            var settings1 = new AppSettings { CurrentTheme = Theme.Light };
            var settings2 = new AppSettings { CurrentTheme = Theme.Dark };

            // Act & Assert
            // 最初の保存
            Assert.True(SettingsService.SaveSettings(settings1));
            var loaded1 = SettingsService.LoadSettings();
            Assert.Equal(Theme.Light, loaded1.CurrentTheme);

            // バックアップ作成
            Assert.True(SettingsService.BackupSettings());

            // 設定変更
            Assert.True(SettingsService.SaveSettings(settings2));
            var loaded2 = SettingsService.LoadSettings();
            Assert.Equal(Theme.Dark, loaded2.CurrentTheme);

            // リセット
            Assert.True(SettingsService.ResetSettings());
            var loaded3 = SettingsService.LoadSettings();
            Assert.Equal(Theme.Light, loaded3.CurrentTheme); // デフォルト値
        }

        #endregion
    }
} 