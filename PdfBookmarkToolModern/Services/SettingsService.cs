using System;
using System.IO;
using System.Text.Json;
using NLog;
using PdfBookmarkToolModern.Models;

namespace PdfBookmarkToolModern.Services
{
    /// <summary>
    /// アプリケーション設定の保存・読み込みサービス
    /// </summary>
    public static class SettingsService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string SettingsFileName = "settings.json";
        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PdfBookmarkToolModern");
        private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, SettingsFileName);

        /// <summary>
        /// 設定を読み込み
        /// </summary>
        public static AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    Logger.Info("設定ファイルが存在しないため、デフォルト設定を作成します");
                    return CreateDefaultSettings();
                }

                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                
                if (settings == null)
                {
                    Logger.Warn("設定ファイルの読み込みに失敗したため、デフォルト設定を使用します");
                    return CreateDefaultSettings();
                }

                Logger.Info("設定ファイルを読み込みました");
                return settings;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "設定ファイルの読み込み中にエラーが発生しました");
                return CreateDefaultSettings();
            }
        }

        /// <summary>
        /// 設定を保存
        /// </summary>
        public static bool SaveSettings(AppSettings settings)
        {
            try
            {
                // ディレクトリが存在しない場合は作成
                if (!Directory.Exists(SettingsDirectory))
                {
                    Directory.CreateDirectory(SettingsDirectory);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsFilePath, json);

                Logger.Info("設定ファイルを保存しました");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "設定ファイルの保存中にエラーが発生しました");
                return false;
            }
        }

        /// <summary>
        /// デフォルト設定を作成
        /// </summary>
        private static AppSettings CreateDefaultSettings()
        {
            var settings = new AppSettings();
            
            // 初回起動時にデフォルト設定を保存
            SaveSettings(settings);
            
            return settings;
        }

        /// <summary>
        /// 設定ファイルを削除（リセット用）
        /// </summary>
        public static bool ResetSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    File.Delete(SettingsFilePath);
                    Logger.Info("設定ファイルを削除しました");
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "設定ファイルの削除中にエラーが発生しました");
                return false;
            }
        }

        /// <summary>
        /// 設定ファイルの場所を取得
        /// </summary>
        public static string GetSettingsFilePath()
        {
            return SettingsFilePath;
        }

        /// <summary>
        /// 設定のバックアップを作成
        /// </summary>
        public static bool BackupSettings()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                    return false;

                string backupPath = SettingsFilePath + $".backup.{DateTime.Now:yyyyMMddHHmmss}";
                File.Copy(SettingsFilePath, backupPath);
                
                Logger.Info($"設定のバックアップを作成しました: {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "設定のバックアップ作成中にエラーが発生しました");
                return false;
            }
        }
    }
} 