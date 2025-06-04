namespace PdfBookmarkToolModern.Commands
{
    /// <summary>
    /// アンドゥ可能なコマンドのインターフェース
    /// </summary>
    public interface IUndoableCommand
    {
        /// <summary>
        /// コマンドを実行
        /// </summary>
        void Execute();

        /// <summary>
        /// コマンドを取り消し
        /// </summary>
        void Undo();

        /// <summary>
        /// コマンドの説明
        /// </summary>
        string Description { get; }

        /// <summary>
        /// コマンドが実行可能かどうか
        /// </summary>
        bool CanExecute { get; }

        /// <summary>
        /// コマンドがアンドゥ可能かどうか
        /// </summary>
        bool CanUndo { get; }
    }
} 