using System.Collections.Generic;

namespace OpenHome3D.Core.Models;

public record Home(
    string Name,
    string Version,
    IReadOnlyList<Wall> Walls,
    IReadOnlyList<Room> Rooms,
    IReadOnlyList<HomePieceOfFurniture> Furniture,
    IReadOnlyList<HomeProperty> Properties
);
