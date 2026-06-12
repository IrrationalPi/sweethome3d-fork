using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenHome3D.Core.Geometry;
using OpenHome3D.Core.Models;

namespace OpenHome3D.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private HomeViewModel? _currentHome;

    [ObservableProperty]
    private string _currentTool = "Select"; // "Select", "CreateWall"

    [ObservableProperty]
    private string? _selectedCatalogItem;

    public ObservableCollection<string> FurnitureCatalog { get; } = new()
    {
        "Sofa",
        "Table",
        "Chair",
        "Bed"
    };

    [RelayCommand]
    private void NewHome()
    {
        CurrentHome = new HomeViewModel(new Home("New Home", "7.5", [], [], [], []));
        CurrentTool = "Select";
    }

    [RelayCommand]
    private void SetSelectTool()
    {
        CurrentTool = "Select";
        SelectedCatalogItem = null;
    }

    [RelayCommand]
    private void SetCreateWallTool()
    {
        CurrentTool = "CreateWall";
        SelectedCatalogItem = null;
    }

    [RelayCommand]
    private void LoadTestHome()
    {
        var testWalls = new List<Wall>
        {
            new("w1", 0, 0, 200, 0, 10, 250),
            new("w2", 200, 0, 200, 200, 10, 250),
            new("w3", 200, 200, 0, 200, 10, 250),
            new("w4", 0, 200, 0, 0, 10, 250)
        };
        var testRooms = new List<Room>
        {
            new("r1", [new Point2D(0, 0), new Point2D(200, 0), new Point2D(200, 200), new Point2D(0, 200)], "Test Room")
        };
        var testHome = new Home("Test Home", "7.5", testWalls, testRooms, [], []);
        CurrentHome = new HomeViewModel(testHome);
        CurrentTool = "Select";
    }
}

public partial class HomeViewModel : ObservableObject
{
    private readonly Home _home;

    public HomeViewModel(Home home)
    {
        _home = home;
        Walls = new ObservableCollection<WallViewModel>(home.Walls.Select(w => new WallViewModel(w)));
        Rooms = new ObservableCollection<RoomViewModel>(home.Rooms.Select(r => new RoomViewModel(r)));
        Furniture = new ObservableCollection<FurnitureViewModel>(home.Furniture.Select(f => new FurnitureViewModel(f)));
    }

    public string Name => _home.Name;
    public string Version => _home.Version;

    public ObservableCollection<WallViewModel> Walls { get; }
    public ObservableCollection<RoomViewModel> Rooms { get; }
    public ObservableCollection<FurnitureViewModel> Furniture { get; }

    [ObservableProperty]
    private double _zoom = 1.0;

    [ObservableProperty]
    private double _panX = 0.0;

    [ObservableProperty]
    private double _panY = 0.0;

    [ObservableProperty]
    private WallViewModel? _selectedWall;

    [ObservableProperty]
    private RoomViewModel? _selectedRoom;

    [ObservableProperty]
    private FurnitureViewModel? _selectedFurniture;

    [RelayCommand]
    private void AddWall()
    {
        var newWall = new Wall("new", 0, 0, 100, 100, 10, 250);
        Walls.Add(new WallViewModel(newWall));
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        if (SelectedWall != null)
        {
            Walls.Remove(SelectedWall);
            SelectedWall = null;
        }
        else if (SelectedRoom != null)
        {
            Rooms.Remove(SelectedRoom);
            SelectedRoom = null;
        }
        else if (SelectedFurniture != null)
        {
            Furniture.Remove(SelectedFurniture);
            SelectedFurniture = null;
        }
    }

    public void AddFurniture(HomePieceOfFurniture furniture)
    {
        Furniture.Add(new FurnitureViewModel(furniture));
    }
}

public partial class WallViewModel : ObservableObject
{
    private readonly Wall _wall;

    public WallViewModel(Wall wall)
    {
        _wall = wall;
    }

    public string Id => _wall.Id;
    public float XStart => _wall.XStart;
    public float YStart => _wall.YStart;
    public float XEnd => _wall.XEnd;
    public float YEnd => _wall.YEnd;
    public float Thickness => _wall.Thickness;
    public float Height => _wall.Height;
}

public partial class RoomViewModel : ObservableObject
{
    private readonly Room _room;

    public RoomViewModel(Room room)
    {
        _room = room;
    }

    public string Id => _room.Id;
    public string? Name => _room.Name;
    public float Area => _room.Area;
    public IReadOnlyList<Point2D> Points => _room.Points;
}

public partial class FurnitureViewModel : ObservableObject
{
    private readonly HomePieceOfFurniture _furniture;

    public FurnitureViewModel(HomePieceOfFurniture furniture)
    {
        _furniture = furniture;
    }

    public string Id => _furniture.Id;
    public string ModelName => _furniture.ModelName;
    public float X => _furniture.X;
    public float Y => _furniture.Y;
    public float Z => _furniture.Z;
    public float Width => _furniture.Width;
    public float Depth => _furniture.Depth;
}