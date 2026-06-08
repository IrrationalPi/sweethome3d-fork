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

    [RelayCommand]
    private void NewHome()
    {
        CurrentHome = new HomeViewModel(new Home("New Home", "7.5", [], [], [], []));
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
}