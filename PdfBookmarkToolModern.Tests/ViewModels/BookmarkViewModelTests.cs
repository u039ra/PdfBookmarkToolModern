using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xunit;
using PdfBookmarkToolModern.ViewModels;
using PdfBookmarkToolModern.Models;

namespace PdfBookmarkToolModern.Tests.ViewModels
{
    /// <summary>
    /// BookmarkViewModelのテストクラス
    /// </summary>
    public class BookmarkViewModelTests
    {
        #region コンストラクターテスト

        [Fact]
        public void Constructor_WithValidBookmarkEntry_ShouldInitializeProperties()
        {
            // Arrange
            var bookmarkEntry = new BookmarkEntry
            {
                Title = "Test Bookmark",
                Level = 2,
                ActionType = "GoTo",
                LinkPage = "5",
                DisplayOption = "XYZ",
                XCoord = "100",
                YCoord = "200"
            };

            // Act
            var viewModel = new BookmarkViewModel(bookmarkEntry);

            // Assert
            Assert.Equal("Test Bookmark", viewModel.Title);
            Assert.Equal(2, viewModel.Level);
            Assert.Equal("GoTo", viewModel.ActionType);
            Assert.Equal("5", viewModel.LinkPage);
            Assert.Equal("XYZ", viewModel.DisplayOption);
            Assert.Equal("100", viewModel.XCoord);
            Assert.Equal("200", viewModel.YCoord);
            Assert.NotNull(viewModel.Children);
            Assert.Empty(viewModel.Children);
        }

        [Fact]
        public void Constructor_WithNullBookmarkEntry_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BookmarkViewModel(null!));
        }

        [Fact]
        public void Constructor_Default_ShouldInitializeWithDefaults()
        {
            // Act
            var viewModel = new BookmarkViewModel();

            // Assert
            Assert.Equal("新しいブックマーク", viewModel.Title);
            Assert.Equal(1, viewModel.Level);
            Assert.Equal("GoTo", viewModel.ActionType);
            Assert.Equal("1", viewModel.LinkPage);
            Assert.Equal("XYZ", viewModel.DisplayOption);
            Assert.NotNull(viewModel.Children);
            Assert.Empty(viewModel.Children);
        }

        #endregion

        #region プロパティ変更通知テスト

        [Theory]
        [InlineData(nameof(BookmarkViewModel.Title), "New Title")]
        [InlineData(nameof(BookmarkViewModel.ActionType), "GoToR")]
        [InlineData(nameof(BookmarkViewModel.LinkPage), "10")]
        [InlineData(nameof(BookmarkViewModel.LinkFile), "test.pdf")]
        [InlineData(nameof(BookmarkViewModel.DisplayOption), "Fit")]
        [InlineData(nameof(BookmarkViewModel.XCoord), "300")]
        [InlineData(nameof(BookmarkViewModel.YCoord), "400")]
        public void PropertyChanges_ShouldRaisePropertyChangedEvent(string propertyName, string newValue)
        {
            // Arrange
            var viewModel = new BookmarkViewModel();
            var eventRaised = false;
            string? raisedPropertyName = null;

            viewModel.PropertyChanged += (sender, e) =>
            {
                eventRaised = true;
                raisedPropertyName = e.PropertyName;
            };

            // Act
            switch (propertyName)
            {
                case nameof(BookmarkViewModel.Title):
                    viewModel.Title = newValue;
                    break;
                case nameof(BookmarkViewModel.ActionType):
                    viewModel.ActionType = newValue;
                    break;
                case nameof(BookmarkViewModel.LinkPage):
                    viewModel.LinkPage = newValue;
                    break;
                case nameof(BookmarkViewModel.LinkFile):
                    viewModel.LinkFile = newValue;
                    break;
                case nameof(BookmarkViewModel.DisplayOption):
                    viewModel.DisplayOption = newValue;
                    break;
                case nameof(BookmarkViewModel.XCoord):
                    viewModel.XCoord = newValue;
                    break;
                case nameof(BookmarkViewModel.YCoord):
                    viewModel.YCoord = newValue;
                    break;
            }

            // Assert
            Assert.True(eventRaised);
            Assert.Equal(propertyName, raisedPropertyName);
        }

        [Fact]
        public void IsSelected_PropertyChange_ShouldRaisePropertyChangedEvent()
        {
            // Arrange
            var viewModel = new BookmarkViewModel();
            var eventRaised = false;

            viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(BookmarkViewModel.IsSelected))
                    eventRaised = true;
            };

            // Act
            viewModel.IsSelected = true;

            // Assert
            Assert.True(eventRaised);
            Assert.True(viewModel.IsSelected);
        }

        [Fact]
        public void IsExpanded_PropertyChange_ShouldRaisePropertyChangedEvent()
        {
            // Arrange
            var viewModel = new BookmarkViewModel();
            var eventRaised = false;

            viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(BookmarkViewModel.IsExpanded))
                    eventRaised = true;
            };

            // Act - デフォルトがtrueなので、falseに変更してからtrueに戻す
            viewModel.IsExpanded = false;
            Assert.False(viewModel.IsExpanded);
            
            eventRaised = false; // リセット
            viewModel.IsExpanded = true;

            // Assert
            Assert.True(eventRaised);
            Assert.True(viewModel.IsExpanded);
        }

        #endregion

        #region DisplayTextテスト

        [Fact]
        public void DisplayText_ShouldFormatCorrectly()
        {
            // Arrange
            var viewModel = new BookmarkViewModel
            {
                Title = "Chapter 1",
                ActionType = "GoTo",
                LinkPage = "10"
            };

            // Act
            var displayText = viewModel.DisplayText;

            // Assert
            Assert.Contains("Chapter 1", displayText);
            Assert.Contains("GoTo", displayText);
            Assert.Contains("10", displayText);
        }

        [Fact]
        public void DisplayText_WithGoToRAction_ShouldIncludeLinkFile()
        {
            // Arrange
            var viewModel = new BookmarkViewModel
            {
                Title = "External Link",
                ActionType = "GoToR",
                LinkFile = "external.pdf",
                LinkPage = "5"
            };

            // Act
            var displayText = viewModel.DisplayText;

            // Assert
            Assert.Contains("External Link", displayText);
            Assert.Contains("GoToR", displayText);
            Assert.Contains("external.pdf", displayText);
            Assert.Contains("5", displayText);
        }

        #endregion

        #region 子要素管理テスト

        [Fact]
        public void Children_ShouldBeObservableCollection()
        {
            // Arrange & Act
            var viewModel = new BookmarkViewModel();

            // Assert
            Assert.IsType<ObservableCollection<BookmarkViewModel>>(viewModel.Children);
        }

        [Fact]
        public void AddChild_ShouldUpdateChildrenCollection()
        {
            // Arrange
            var parentViewModel = new BookmarkViewModel { Title = "Parent" };
            var childViewModel = new BookmarkViewModel { Title = "Child" };

            // Act
            parentViewModel.Children.Add(childViewModel);

            // Assert
            Assert.Single(parentViewModel.Children);
            Assert.Equal(childViewModel, parentViewModel.Children[0]);
        }

        #endregion

        #region ToBookmarkEntryテスト

        [Fact]
        public void ToBookmarkEntry_ShouldConvertCorrectly()
        {
            // Arrange
            var viewModel = new BookmarkViewModel
            {
                Title = "Test Title",
                Level = 3,
                ActionType = "GoTo",
                LinkPage = "15",
                DisplayOption = "FitH",
                XCoord = "50",
                YCoord = "75"
            };

            // Act
            var bookmarkEntry = viewModel.ToBookmarkEntry();

            // Assert
            Assert.Equal("Test Title", bookmarkEntry.Title);
            Assert.Equal(3, bookmarkEntry.Level);
            Assert.Equal("GoTo", bookmarkEntry.ActionType);
            Assert.Equal("15", bookmarkEntry.LinkPage);
            Assert.Equal("FitH", bookmarkEntry.DisplayOption);
            Assert.Equal("50", bookmarkEntry.XCoord);
            Assert.Equal("75", bookmarkEntry.YCoord);
        }

        [Fact]
        public void ToBookmarkEntry_WithChildren_ShouldConvertHierarchy()
        {
            // Arrange
            var parentViewModel = new BookmarkViewModel { Title = "Parent", Level = 1 };
            var childViewModel = new BookmarkViewModel { Title = "Child", Level = 2 };
            parentViewModel.Children.Add(childViewModel);

            // Act
            var bookmarkEntry = parentViewModel.ToBookmarkEntry();

            // Assert
            Assert.Equal("Parent", bookmarkEntry.Title);
            Assert.Single(bookmarkEntry.Children);
            Assert.Equal("Child", bookmarkEntry.Children[0].Title);
        }

        #endregion

        #region フラットリスト変換テスト

        [Fact]
        public void ToFlatList_ShouldConvertHierarchyToFlat()
        {
            // Arrange
            var rootCollection = new ObservableCollection<BookmarkViewModel>();
            
            var parent = new BookmarkViewModel { Title = "Parent", Level = 1 };
            var child1 = new BookmarkViewModel { Title = "Child1", Level = 2 };
            var child2 = new BookmarkViewModel { Title = "Child2", Level = 2 };
            var grandChild = new BookmarkViewModel { Title = "GrandChild", Level = 3 };
            
            child1.Children.Add(grandChild);
            parent.Children.Add(child1);
            parent.Children.Add(child2);
            rootCollection.Add(parent);

            // Act
            var flatList = BookmarkViewModel.ToFlatList(rootCollection);

            // Assert
            Assert.Equal(4, flatList.Count);
            Assert.Equal("Parent", flatList[0].Title);
            Assert.Equal("Child1", flatList[1].Title);
            Assert.Equal("GrandChild", flatList[2].Title);
            Assert.Equal("Child2", flatList[3].Title);
        }

        [Fact]
        public void ToFlatList_EmptyCollection_ShouldReturnEmpty()
        {
            // Arrange
            var emptyCollection = new ObservableCollection<BookmarkViewModel>();

            // Act
            var flatList = BookmarkViewModel.ToFlatList(emptyCollection);

            // Assert
            Assert.Empty(flatList);
        }

        #endregion

        #region エッジケーステスト

        [Fact]
        public void PropertyChange_SameValue_ShouldNotRaiseEvent()
        {
            // Arrange
            var viewModel = new BookmarkViewModel { Title = "Same Title" };
            var eventRaisedCount = 0;

            viewModel.PropertyChanged += (sender, e) => eventRaisedCount++;

            // Act
            viewModel.Title = "Same Title"; // 同じ値を設定

            // Assert
            Assert.Equal(0, eventRaisedCount);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Title_NullOrWhiteSpace_ShouldStillWork(string? title)
        {
            // Arrange & Act
            var viewModel = new BookmarkViewModel { Title = title };

            // Assert
            Assert.Equal(title, viewModel.Title);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(100)]
        public void Level_AnyIntegerValue_ShouldAccept(int level)
        {
            // Arrange & Act
            var viewModel = new BookmarkViewModel { Level = level };

            // Assert
            Assert.Equal(level, viewModel.Level);
        }

        #endregion
    }
} 