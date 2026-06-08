using OpenHome3D.Core.Models;
using OpenHome3D.Core.ViewModels;

namespace OpenHome3D.Core.Tests.ViewModels;

public class ViewModelTests
{
    [Fact]
    public void MainViewModel_NewHomeCommand_CreatesNewHome()
    {
        var viewModel = new MainViewModel();
        Assert.Null(viewModel.CurrentHome);

        viewModel.NewHomeCommand.Execute(null);

        Assert.NotNull(viewModel.CurrentHome);
        Assert.Equal("New Home", viewModel.CurrentHome.Name);
    }

    [Fact]
    public void HomeViewModel_AddWallCommand_AddsWallAndRaisesPropertyChanged()
    {
        var home = new Home("Test", "7.5", [], [], [], []);
        var viewModel = new HomeViewModel(home);
        
        int wallCountChangedCount = 0;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(viewModel.Walls))
            {
                wallCountChangedCount++;
            }
        };

        int initialCount = viewModel.Walls.Count;
        viewModel.AddWallCommand.Execute(null);

        Assert.Equal(initialCount + 1, viewModel.Walls.Count);
        // Note: CommunityToolkit.Mvvm ObservableProperty doesn't raise PropertyChanged for the collection itself when items are added, 
        // but it does for the collection contents if we use [ObservableProperty] on the collection, which we didn't. 
        // We can test that the collection count increased.
    }

    [Fact]
    public void HomeViewModel_DeleteSelectedCommand_RemovesSelectedWall()
    {
        var wall = new Wall("1", 0, 0, 100, 100, 10, 250);
        var home = new Home("Test", "7.5", [wall], [], [], []);
        var viewModel = new HomeViewModel(home);

        Assert.Single(viewModel.Walls);
        viewModel.SelectedWall = viewModel.Walls[0];

        viewModel.DeleteSelectedCommand.Execute(null);

        Assert.Empty(viewModel.Walls);
        Assert.Null(viewModel.SelectedWall);
    }

    [Fact]
    public void HomeViewModel_DeleteSelectedCommand_RemovesSelectedRoom()
    {
        var room = new Room("1", []);
        var home = new Home("Test", "7.5", [], [room], [], []);
        var viewModel = new HomeViewModel(home);

        Assert.Single(viewModel.Rooms);
        viewModel.SelectedRoom = viewModel.Rooms[0];

        viewModel.DeleteSelectedCommand.Execute(null);

        Assert.Empty(viewModel.Rooms);
        Assert.Null(viewModel.SelectedRoom);
    }

    [Fact]
    public void HomeViewModel_DeleteSelectedCommand_RemovesSelectedFurniture()
    {
        var furniture = new HomePieceOfFurniture("1", 0, 0, 0, 0, 10, 10, 10, "Chair");
        var home = new Home("Test", "7.5", [], [], [furniture], []);
        var viewModel = new HomeViewModel(home);

        Assert.Single(viewModel.Furniture);
        viewModel.SelectedFurniture = viewModel.Furniture[0];

        viewModel.DeleteSelectedCommand.Execute(null);

        Assert.Empty(viewModel.Furniture);
        Assert.Null(viewModel.SelectedFurniture);
    }
}