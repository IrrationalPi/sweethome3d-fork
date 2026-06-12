using Avalonia.Headless.XUnit;
using OpenHome3D.Core.ViewModels;
using OpenHome3D.UI;
using Xunit;

namespace OpenHome3D.UI.Tests;

public class MainWindowTests
{
    [AvaloniaFact]
    public void MainWindow_Instantiates_With_MainViewModel_DataContext()
    {
        // Arrange & Act
        var window = new MainWindow();
        var viewModel = new MainViewModel();
        window.DataContext = viewModel;

        // Assert
        Assert.NotNull(window);
        Assert.IsType<MainViewModel>(window.DataContext);
        Assert.Same(viewModel, window.DataContext);
    }
}
