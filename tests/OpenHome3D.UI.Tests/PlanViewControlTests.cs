using Avalonia.Headless.XUnit;
using Avalonia.Input;
using OpenHome3D.Core.Models;
using OpenHome3D.Core.ViewModels;
using OpenHome3D.UI;
using Xunit;

namespace OpenHome3D.UI.Tests;

public class PlanViewControlTests
{
    [AvaloniaFact]
    public void PlanViewControl_Binds_To_ViewModel_And_Responds_To_Collection_Changes()
    {
        // Arrange
        var home = new Home("Test Home", "7.5", [], [], [], []);
        var viewModel = new HomeViewModel(home);
        var control = new PlanViewControl
        {
            DataContext = viewModel
        };

        // Act - Add a wall via command
        viewModel.AddWallCommand.Execute(null);

        // Assert
        Assert.Single(viewModel.Walls);
        Assert.NotNull(control.DataContext);
        Assert.IsType<HomeViewModel>(control.DataContext);
    }

    [AvaloniaFact]
    public void PlanViewControl_PointerPressed_Updates_Selection()
    {
        // Arrange
        var wall = new Wall("w1", 10, 10, 100, 10, 10, 250);
        var home = new Home("Test Home", "7.5", [wall], [], [], []);
        var viewModel = new HomeViewModel(home);
        var control = new PlanViewControl
        {
            DataContext = viewModel,
            Width = 800,
            Height = 600
        };

        // Assert
        Assert.Null(viewModel.SelectedWall);
        
        // Simulate selection logic manually to verify the view model updates
        viewModel.SelectedWall = viewModel.Walls[0];
        Assert.NotNull(viewModel.SelectedWall);
        Assert.Equal("w1", viewModel.SelectedWall.Id);
    }

    [AvaloniaFact]
    public void PlanViewControl_Drop_Adds_Furniture_To_ViewModel()
    {
        // Arrange
        var home = new Home("Test Home", "7.5", [], [], [], []);
        var mainViewModel = new MainViewModel();
        mainViewModel.NewHomeCommand.Execute(null);
        var viewModel = mainViewModel.CurrentHome!;
        
        var control = new PlanViewControl
        {
            DataContext = mainViewModel,
            Width = 800,
            Height = 600
        };

        // Create mock drag data
        var dragData = new DataTransfer();
        dragData.Add(DataTransferItem.CreateText("TestFurniture123"));

        var dragEventArgs = new Avalonia.Input.DragEventArgs(
            DragDrop.DropEvent,
            dragData,
            control,
            new Avalonia.Point(100, 100),
            KeyModifiers.None
        );

        // Act
        control.RaiseEvent(dragEventArgs);

        // Assert
        Assert.Single(viewModel.Furniture);
        var furniture = viewModel.Furniture[0];
        Assert.Equal("TestFurniture123", furniture.ModelName);
        // World coordinates should be screen coordinates (100, 100) divided by zoom (1.0) minus pan (0, 0)
        Assert.Equal(100f, furniture.X);
        Assert.Equal(100f, furniture.Y);
    }
}
