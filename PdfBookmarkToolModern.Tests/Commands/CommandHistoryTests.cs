using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using PdfBookmarkToolModern.Commands;

namespace PdfBookmarkToolModern.Tests.Commands
{
    /// <summary>
    /// CommandHistoryのテストクラス
    /// </summary>
    public class CommandHistoryTests
    {
        #region コンストラクターテスト

        [Fact]
        public void Constructor_Default_ShouldInitializeWithCorrectState()
        {
            // Act
            var history = new CommandHistory();

            // Assert
            Assert.False(history.CanUndo);
            Assert.False(history.CanRedo);
            Assert.Null(history.NextUndoDescription);
            Assert.Null(history.NextRedoDescription);
            Assert.Equal(0, history.UndoCount);
            Assert.Equal(0, history.RedoCount);
        }

        [Fact]
        public void Constructor_WithMaxHistorySize_ShouldSetCorrectLimit()
        {
            // Arrange
            const int maxSize = 10;

            // Act
            var history = new CommandHistory(maxSize);

            // Assert
            Assert.False(history.CanUndo);
            Assert.False(history.CanRedo);
        }

        #endregion

        #region ExecuteCommandテスト

        [Fact]
        public void ExecuteCommand_ValidCommand_ShouldExecuteAndAddToHistory()
        {
            // Arrange
            var history = new CommandHistory();
            var mockCommand = new Mock<IUndoableCommand>();
            mockCommand.Setup(c => c.CanExecute).Returns(true);
            mockCommand.Setup(c => c.Description).Returns("Test Command");

            // Act
            history.ExecuteCommand(mockCommand.Object);

            // Assert
            mockCommand.Verify(c => c.Execute(), Times.Once);
            Assert.True(history.CanUndo);
            Assert.False(history.CanRedo);
            Assert.Equal("Test Command", history.NextUndoDescription);
            Assert.Equal(1, history.UndoCount);
        }

        [Fact]
        public void ExecuteCommand_NullCommand_ShouldThrowArgumentNullException()
        {
            // Arrange
            var history = new CommandHistory();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => history.ExecuteCommand(null!));
        }

        [Fact]
        public void ExecuteCommand_CanExecuteFalse_ShouldNotExecute()
        {
            // Arrange
            var history = new CommandHistory();
            var mockCommand = new Mock<IUndoableCommand>();
            mockCommand.Setup(c => c.CanExecute).Returns(false);

            // Act
            history.ExecuteCommand(mockCommand.Object);

            // Assert
            mockCommand.Verify(c => c.Execute(), Times.Never);
            Assert.False(history.CanUndo);
        }

        [Fact]
        public void ExecuteCommand_NewCommandAfterUndo_ShouldClearRedoStack()
        {
            // Arrange
            var history = new CommandHistory();
            var command1 = CreateMockCommand("Command 1");
            var command2 = CreateMockCommand("Command 2");
            var command3 = CreateMockCommand("Command 3");

            // Act
            history.ExecuteCommand(command1);
            history.ExecuteCommand(command2);
            history.Undo(); // command2をアンドゥ（リドゥスタックに移動）
            
            Assert.True(history.CanRedo); // リドゥ可能であることを確認
            
            history.ExecuteCommand(command3); // 新しいコマンドを実行

            // Assert
            Assert.False(history.CanRedo); // リドゥスタックがクリアされているはず
            Assert.Equal(2, history.UndoCount); // command1とcommand3
        }

        #endregion

        #region Undoテスト

        [Fact]
        public void Undo_WithUndoableCommand_ShouldUndoAndMoveToRedoStack()
        {
            // Arrange
            var history = new CommandHistory();
            var mockCommand = new Mock<IUndoableCommand>();
            mockCommand.Setup(c => c.CanExecute).Returns(true);
            mockCommand.Setup(c => c.CanUndo).Returns(true);
            mockCommand.Setup(c => c.Description).Returns("Test Command");
            
            history.ExecuteCommand(mockCommand.Object);

            // Act
            history.Undo();

            // Assert
            mockCommand.Verify(c => c.Undo(), Times.Once);
            Assert.False(history.CanUndo);
            Assert.True(history.CanRedo);
            Assert.Equal("Test Command", history.NextRedoDescription);
        }

        [Fact]
        public void Undo_EmptyHistory_ShouldNotExecute()
        {
            // Arrange
            var history = new CommandHistory();

            // Act
            history.Undo(); // 何も起こらないはず

            // Assert
            Assert.False(history.CanUndo);
            Assert.False(history.CanRedo);
        }

        #endregion

        #region Clearテスト

        [Fact]
        public void Clear_ShouldRemoveAllHistory()
        {
            // Arrange
            var history = new CommandHistory();
            var command1 = CreateMockCommand("Command 1");
            var command2 = CreateMockCommand("Command 2");
            
            history.ExecuteCommand(command1);
            history.ExecuteCommand(command2);
            history.Undo();

            // Act
            history.Clear();

            // Assert
            Assert.False(history.CanUndo);
            Assert.False(history.CanRedo);
            Assert.Equal(0, history.UndoCount);
            Assert.Equal(0, history.RedoCount);
        }

        #endregion

        #region プロパティ変更通知テスト

        [Fact]
        public void ExecuteCommand_ShouldRaisePropertyChangedEvents()
        {
            // Arrange
            var history = new CommandHistory();
            var command = CreateMockCommand("Test Command");
            var propertyChangedEvents = new List<string>();

            history.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    propertyChangedEvents.Add(e.PropertyName);
            };

            // Act
            history.ExecuteCommand(command);

            // Assert
            Assert.Contains(nameof(CommandHistory.CanUndo), propertyChangedEvents);
            Assert.Contains(nameof(CommandHistory.CanRedo), propertyChangedEvents);
            Assert.Contains(nameof(CommandHistory.NextUndoDescription), propertyChangedEvents);
            Assert.Contains(nameof(CommandHistory.UndoCount), propertyChangedEvents);
        }

        #endregion

        #region ヘルパーメソッド

        private IUndoableCommand CreateMockCommand(string description)
        {
            var mock = new Mock<IUndoableCommand>();
            mock.Setup(c => c.CanExecute).Returns(true);
            mock.Setup(c => c.CanUndo).Returns(true);
            mock.Setup(c => c.Description).Returns(description);
            return mock.Object;
        }

        #endregion
    }
} 