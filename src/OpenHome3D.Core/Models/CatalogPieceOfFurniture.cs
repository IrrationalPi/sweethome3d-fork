namespace OpenHome3D.Core.Models;

public record CatalogPieceOfFurniture(
    string Id,
    string Name,
    string Category,
    float Width,
    float Depth,
    float Height,
    string? ModelName = null
);

public record FurnitureCatalog(
    IReadOnlyList<CatalogPieceOfFurniture> Pieces
);