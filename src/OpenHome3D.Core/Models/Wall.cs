namespace OpenHome3D.Core.Models;

public record Wall(
    string Id,
    float XStart,
    float YStart,
    float XEnd,
    float YEnd,
    float Thickness,
    float Height,
    Wall? WallAtStart = null,
    Wall? WallAtEnd = null
);
