using System;
using System.Collections.ObjectModel;
using PdfBookmarkToolModern.ViewModels;

namespace PdfBookmarkToolModern.Commands
{
    /// <summary>
    /// ブックマーク追加コマンド
    /// </summary>
    public class AddBookmarkCommand : IUndoableCommand
    {
        private readonly ObservableCollection<BookmarkViewModel> _collection;
        private readonly BookmarkViewModel _bookmark;
        private readonly int _index;

        public AddBookmarkCommand(ObservableCollection<BookmarkViewModel> collection, BookmarkViewModel bookmark, int? index = null)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _bookmark = bookmark ?? throw new ArgumentNullException(nameof(bookmark));
            _index = index ?? collection.Count;
        }

        public string Description => $"ブックマーク「{_bookmark.Title}」を追加";
        public bool CanExecute => _bookmark != null;
        public bool CanUndo => true;

        public void Execute()
        {
            if (_index >= 0 && _index <= _collection.Count)
            {
                _collection.Insert(_index, _bookmark);
            }
            else
            {
                _collection.Add(_bookmark);
            }
        }

        public void Undo()
        {
            _collection.Remove(_bookmark);
        }
    }

    /// <summary>
    /// ブックマーク削除コマンド
    /// </summary>
    public class RemoveBookmarkCommand : IUndoableCommand
    {
        private readonly ObservableCollection<BookmarkViewModel> _collection;
        private readonly BookmarkViewModel _bookmark;
        private int _originalIndex;

        public RemoveBookmarkCommand(ObservableCollection<BookmarkViewModel> collection, BookmarkViewModel bookmark)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _bookmark = bookmark ?? throw new ArgumentNullException(nameof(bookmark));
        }

        public string Description => $"ブックマーク「{_bookmark.Title}」を削除";
        public bool CanExecute => _collection.Contains(_bookmark);
        public bool CanUndo => true;

        public void Execute()
        {
            _originalIndex = _collection.IndexOf(_bookmark);
            _collection.Remove(_bookmark);
        }

        public void Undo()
        {
            if (_originalIndex >= 0 && _originalIndex <= _collection.Count)
            {
                _collection.Insert(_originalIndex, _bookmark);
            }
            else
            {
                _collection.Add(_bookmark);
            }
        }
    }

    /// <summary>
    /// ブックマーク移動コマンド
    /// </summary>
    public class MoveBookmarkCommand : IUndoableCommand
    {
        private readonly ObservableCollection<BookmarkViewModel> _sourceCollection;
        private readonly ObservableCollection<BookmarkViewModel> _targetCollection;
        private readonly BookmarkViewModel _bookmark;
        private readonly int _sourceIndex;
        private readonly int _targetIndex;

        public MoveBookmarkCommand(
            ObservableCollection<BookmarkViewModel> sourceCollection,
            ObservableCollection<BookmarkViewModel> targetCollection,
            BookmarkViewModel bookmark,
            int targetIndex)
        {
            _sourceCollection = sourceCollection ?? throw new ArgumentNullException(nameof(sourceCollection));
            _targetCollection = targetCollection ?? throw new ArgumentNullException(nameof(targetCollection));
            _bookmark = bookmark ?? throw new ArgumentNullException(nameof(bookmark));
            _targetIndex = targetIndex;
            _sourceIndex = sourceCollection.IndexOf(bookmark);
        }

        public string Description => $"ブックマーク「{_bookmark.Title}」を移動";
        public bool CanExecute => _sourceCollection.Contains(_bookmark);
        public bool CanUndo => true;

        public void Execute()
        {
            _sourceCollection.Remove(_bookmark);
            
            if (_targetIndex >= 0 && _targetIndex <= _targetCollection.Count)
            {
                _targetCollection.Insert(_targetIndex, _bookmark);
            }
            else
            {
                _targetCollection.Add(_bookmark);
            }
        }

        public void Undo()
        {
            _targetCollection.Remove(_bookmark);
            
            if (_sourceIndex >= 0 && _sourceIndex <= _sourceCollection.Count)
            {
                _sourceCollection.Insert(_sourceIndex, _bookmark);
            }
            else
            {
                _sourceCollection.Add(_bookmark);
            }
        }
    }

    /// <summary>
    /// ブックマーク編集コマンド
    /// </summary>
    public class EditBookmarkCommand : IUndoableCommand
    {
        private readonly BookmarkViewModel _bookmark;
        private readonly string _originalTitle;
        private readonly string _originalActionType;
        private readonly string _originalLinkPage;
        private readonly string _originalLinkFile;
        private readonly string _originalDisplayOption;
        private readonly string _originalXCoord;
        private readonly string _originalYCoord;
        private readonly string _newTitle;
        private readonly string _newActionType;
        private readonly string _newLinkPage;
        private readonly string _newLinkFile;
        private readonly string _newDisplayOption;
        private readonly string _newXCoord;
        private readonly string _newYCoord;

        public EditBookmarkCommand(BookmarkViewModel bookmark, 
            string? newTitle = null,
            string? newActionType = null,
            string? newLinkPage = null,
            string? newLinkFile = null,
            string? newDisplayOption = null,
            string? newXCoord = null,
            string? newYCoord = null)
        {
            _bookmark = bookmark ?? throw new ArgumentNullException(nameof(bookmark));
            
            // 元の値を保存
            _originalTitle = bookmark.Title ?? "";
            _originalActionType = bookmark.ActionType ?? "";
            _originalLinkPage = bookmark.LinkPage ?? "";
            _originalLinkFile = bookmark.LinkFile ?? "";
            _originalDisplayOption = bookmark.DisplayOption ?? "";
            _originalXCoord = bookmark.XCoord ?? "";
            _originalYCoord = bookmark.YCoord ?? "";
            
            // 新しい値（nullの場合は元の値を維持）
            _newTitle = newTitle ?? _originalTitle;
            _newActionType = newActionType ?? _originalActionType;
            _newLinkPage = newLinkPage ?? _originalLinkPage;
            _newLinkFile = newLinkFile ?? _originalLinkFile;
            _newDisplayOption = newDisplayOption ?? _originalDisplayOption;
            _newXCoord = newXCoord ?? _originalXCoord;
            _newYCoord = newYCoord ?? _originalYCoord;
        }

        public string Description => $"ブックマーク「{_originalTitle}」を編集";
        public bool CanExecute => _bookmark != null;
        public bool CanUndo => true;

        public void Execute()
        {
            _bookmark.Title = _newTitle;
            _bookmark.ActionType = _newActionType;
            _bookmark.LinkPage = _newLinkPage;
            _bookmark.LinkFile = _newLinkFile;
            _bookmark.DisplayOption = _newDisplayOption;
            _bookmark.XCoord = _newXCoord;
            _bookmark.YCoord = _newYCoord;
        }

        public void Undo()
        {
            _bookmark.Title = _originalTitle;
            _bookmark.ActionType = _originalActionType;
            _bookmark.LinkPage = _originalLinkPage;
            _bookmark.LinkFile = _originalLinkFile;
            _bookmark.DisplayOption = _originalDisplayOption;
            _bookmark.XCoord = _originalXCoord;
            _bookmark.YCoord = _originalYCoord;
        }
    }

    /// <summary>
    /// 複数ブックマーク一括操作コマンド
    /// </summary>
    public class BatchBookmarkCommand : IUndoableCommand
    {
        private readonly IUndoableCommand[] _commands;

        public BatchBookmarkCommand(string description, params IUndoableCommand[] commands)
        {
            Description = description;
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public string Description { get; }
        public bool CanExecute => Array.TrueForAll(_commands, cmd => cmd.CanExecute);
        public bool CanUndo => Array.TrueForAll(_commands, cmd => cmd.CanUndo);

        public void Execute()
        {
            foreach (var command in _commands)
            {
                command.Execute();
            }
        }

        public void Undo()
        {
            // 逆順でアンドゥを実行
            for (int i = _commands.Length - 1; i >= 0; i--)
            {
                _commands[i].Undo();
            }
        }
    }
} 