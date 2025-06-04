using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PdfBookmarkToolModern.Commands
{
    /// <summary>
    /// コマンド履歴管理クラス
    /// </summary>
    public class CommandHistory : INotifyPropertyChanged
    {
        private readonly Stack<IUndoableCommand> _undoStack = new();
        private readonly Stack<IUndoableCommand> _redoStack = new();
        private readonly int _maxHistorySize;

        public CommandHistory(int maxHistorySize = 50)
        {
            _maxHistorySize = maxHistorySize;
        }

        /// <summary>
        /// アンドゥ可能かどうか
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>
        /// リドゥ可能かどうか
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// 次にアンドゥされるコマンドの説明
        /// </summary>
        public string? NextUndoDescription => CanUndo ? _undoStack.Peek().Description : null;

        /// <summary>
        /// 次にリドゥされるコマンドの説明
        /// </summary>
        public string? NextRedoDescription => CanRedo ? _redoStack.Peek().Description : null;

        /// <summary>
        /// アンドゥスタックのサイズ
        /// </summary>
        public int UndoCount => _undoStack.Count;

        /// <summary>
        /// リドゥスタックのサイズ
        /// </summary>
        public int RedoCount => _redoStack.Count;

        /// <summary>
        /// コマンドを実行し、履歴に追加
        /// </summary>
        public void ExecuteCommand(IUndoableCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (!command.CanExecute)
                return;

            try
            {
                command.Execute();
                
                // 新しいコマンドが実行されたらリドゥスタックをクリア
                _redoStack.Clear();
                
                // アンドゥスタックに追加
                _undoStack.Push(command);
                
                // 履歴サイズの制限をチェック
                while (_undoStack.Count > _maxHistorySize)
                {
                    var oldCommands = new List<IUndoableCommand>();
                    while (_undoStack.Count > 0)
                    {
                        oldCommands.Add(_undoStack.Pop());
                    }
                    
                    // 最新のコマンド以外を削除
                    for (int i = 0; i < Math.Min(_maxHistorySize, oldCommands.Count); i++)
                    {
                        _undoStack.Push(oldCommands[i]);
                    }
                }
                
                OnPropertyChanged(nameof(CanUndo));
                OnPropertyChanged(nameof(CanRedo));
                OnPropertyChanged(nameof(NextUndoDescription));
                OnPropertyChanged(nameof(NextRedoDescription));
                OnPropertyChanged(nameof(UndoCount));
                OnPropertyChanged(nameof(RedoCount));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"コマンドの実行に失敗しました: {command.Description}", ex);
            }
        }

        /// <summary>
        /// アンドゥを実行
        /// </summary>
        public void Undo()
        {
            if (!CanUndo)
                return;

            var command = _undoStack.Pop();
            
            try
            {
                if (command.CanUndo)
                {
                    command.Undo();
                    _redoStack.Push(command);
                }
            }
            catch (Exception ex)
            {
                // アンドゥに失敗した場合、コマンドをスタックに戻す
                _undoStack.Push(command);
                throw new InvalidOperationException($"アンドゥの実行に失敗しました: {command.Description}", ex);
            }
            
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(NextUndoDescription));
            OnPropertyChanged(nameof(NextRedoDescription));
            OnPropertyChanged(nameof(UndoCount));
            OnPropertyChanged(nameof(RedoCount));
        }

        /// <summary>
        /// リドゥを実行
        /// </summary>
        public void Redo()
        {
            if (!CanRedo)
                return;

            var command = _redoStack.Pop();
            
            try
            {
                if (command.CanExecute)
                {
                    command.Execute();
                    _undoStack.Push(command);
                }
            }
            catch (Exception ex)
            {
                // リドゥに失敗した場合、コマンドをスタックに戻す
                _redoStack.Push(command);
                throw new InvalidOperationException($"リドゥの実行に失敗しました: {command.Description}", ex);
            }
            
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(NextUndoDescription));
            OnPropertyChanged(nameof(NextRedoDescription));
            OnPropertyChanged(nameof(UndoCount));
            OnPropertyChanged(nameof(RedoCount));
        }

        /// <summary>
        /// 履歴をクリア
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(NextUndoDescription));
            OnPropertyChanged(nameof(NextRedoDescription));
            OnPropertyChanged(nameof(UndoCount));
            OnPropertyChanged(nameof(RedoCount));
        }

        /// <summary>
        /// アンドゥ履歴の一覧を取得
        /// </summary>
        public IEnumerable<string> GetUndoHistory()
        {
            foreach (var command in _undoStack)
            {
                yield return command.Description;
            }
        }

        /// <summary>
        /// リドゥ履歴の一覧を取得
        /// </summary>
        public IEnumerable<string> GetRedoHistory()
        {
            foreach (var command in _redoStack)
            {
                yield return command.Description;
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
} 