# PDFブックマークツール Modern 🔖

**Windows Forms から WPF への完全移行版**

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Latest-blue.svg)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![PDFsharp](https://img.shields.io/badge/PDFsharp-6.2.0-green.svg)](https://www.pdfsharp.net/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## 概要 📋

PDFファイルのブックマーク（しおり）を **GUI上で直感的に編集** できるWPFアプリケーションです。ドラッグ&ドロップによる階層変更、リアルタイムデータバインディング、モダンなMVVMパターンを採用しています。

## 🎯 主要機能

### ✅ **実装完了**
- **🎨 モダンWPF UI**: 直感的で美しいユーザーインターフェース
- **🔄 ドラッグ&ドロップ**: ブックマークの並び替えと階層変更
- **📊 階層TreeView**: ブックマーク構造の視覚的表示
- **⚡ リアルタイム編集**: プロパティ変更の即座反映
- **🎭 MVVM パターン**: 保守性の高いアーキテクチャ
- **🔧 豊富な編集機能**: タイトル、アクション、座標等の詳細編集

### 🚧 **実装予定**
- **📂 PDF読み込み**: PDFファイルからブックマーク抽出
- **📊 Excel連携**: Excel形式でのインポート/エクスポート
- **💾 PDF書き込み**: 編集したブックマークのPDF保存
- **⚡ 一括処理**: フォルダ内PDF一括変換
- **🔍 検索機能**: ブックマーク検索とフィルタリング

## 🏗️ アーキテクチャ

```
📁 PdfBookmarkToolModern/
├── 🖼️ App.xaml/App.xaml.cs          # WPFアプリケーション
├── 🏠 MainWindow.xaml/xaml.cs       # メインUI + ドラッグ&ドロップ
├── 📊 ViewModels/
│   ├── MainViewModel.cs             # メインViewModel (MVVM)
│   └── BookmarkViewModel.cs         # ブックマーク用ViewModel
├── 📋 Models/
│   ├── BookmarkEntry.cs             # データモデル
│   └── Constants.cs                 # 定数定義
├── 🔧 Services/
│   └── ExcelService.cs              # Excel操作サービス
├── ⚡ Commands/
│   └── RelayCommand.cs              # WPFコマンド実装
└── 🎨 Converters/
    ├── ActionTypeToVisibilityConverter.cs
    └── AdditionalConverters.cs      # UI制御用コンバーター
```

## 🚀 技術スタック

- **フレームワーク**: .NET 9.0 + WPF
- **PDF処理**: PDFsharp 6.2.0
- **Excel処理**: ClosedXML 0.102.2
- **ログ**: NLog 5.2.8
- **パターン**: MVVM + データバインディング
- **UI**: XAML + Converters + Behaviors

## 🔧 セットアップ

### 前提条件
- Windows 10/11
- .NET 9.0 SDK
- Visual Studio 2022 または Visual Studio Code

### インストール手順

1. **リポジトリクローン**
   ```bash
   git clone https://github.com/your-username/PdfBookmarkToolModern.git
   cd PdfBookmarkToolModern
   ```

2. **依存関係復元**
   ```bash
   dotnet restore
   ```

3. **ビルド**
   ```bash
   dotnet build
   ```

4. **実行**
   ```bash
   dotnet run --project PdfBookmarkToolModern
   ```

## 📖 使い方

### 基本操作
1. **アプリケーション起動**: メニューバーから各機能にアクセス
2. **ブックマーク表示**: 左パネルのTreeViewで階層表示
3. **詳細編集**: 右パネルでプロパティ編集
4. **ドラッグ&ドロップ**: マウスで直感的に並び替え

### 編集機能
- **追加**: 「追加」ボタンで新しいブックマーク作成
- **削除**: 「削除」ボタンで選択したブックマーク削除
- **移動**: ドラッグ&ドロップで階層変更
- **編集**: 右パネルでタイトル、アクション等を編集

## 🎨 UI スクリーンショット

*実装完了後に追加予定*

## 📋 開発ロードマップ

### Phase 1: 基盤完成 ✅
- [x] WPF基本UI構築
- [x] MVVMパターン実装
- [x] ドラッグ&ドロップ機能
- [x] データバインディング

### Phase 2: コア機能実装 🚧
- [ ] PDF読み込み機能
- [ ] Excel連携機能
- [ ] PDF書き込み機能
- [ ] エラーハンドリング強化

### Phase 3: 高度機能 📋
- [ ] 一括処理機能
- [ ] 検索・フィルタ機能
- [ ] 設定画面
- [ ] プレビュー機能

### Phase 4: 品質向上 📋
- [ ] 単体テスト
- [ ] パフォーマンス最適化
- [ ] UI/UX改善
- [ ] ドキュメント充実

## 🤝 コントリビューション

プロジェクトへの貢献を歓迎します！

1. フォーク
2. フィーチャーブランチ作成 (`git checkout -b feature/amazing-feature`)
3. コミット (`git commit -m 'Add amazing feature'`)
4. プッシュ (`git push origin feature/amazing-feature`)
5. プルリクエスト作成

## 📄 ライセンス

このプロジェクトはMITライセンスの下で公開されています。詳細は [LICENSE](LICENSE) ファイルをご覧ください。

## 🆕 更新履歴

### v2.0.0 (現在開発中)
- ✅ Windows Forms から WPF への完全移行
- ✅ MVVMパターン採用
- ✅ ドラッグ&ドロップ機能実装
- ✅ モダンUI設計

### v1.0.0 (レガシー版)
- Windows Forms ベース
- 基本的なPDF/Excel連携機能

## 🔗 関連リンク

- [PDFsharp 公式ドキュメント](https://www.pdfsharp.net/)
- [WPF 公式ドキュメント](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [.NET 9.0 ドキュメント](https://docs.microsoft.com/en-us/dotnet/)

---

**Made with ❤️ for PDF enthusiasts** 