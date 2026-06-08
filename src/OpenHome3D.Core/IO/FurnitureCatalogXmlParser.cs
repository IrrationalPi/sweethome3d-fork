using System.Globalization;
using System.Xml.Linq;
using OpenHome3D.Core.Models;

namespace OpenHome3D.Core.IO;

public static class FurnitureCatalogXmlParser
{
    public static FurnitureCatalog Parse(string xmlContent)
    {
        var doc = XDocument.Parse(xmlContent);
        var root = doc.Root;
        
        if (root == null || root.Name.LocalName != "catalog")
        {
            throw new InvalidOperationException("Invalid XML: root element must be 'catalog'.");
        }

        var pieces = root.Elements("piece").Select(e => new CatalogPieceOfFurniture(
            Id: e.Attribute("id")?.Value ?? string.Empty,
            Name: e.Attribute("name")?.Value ?? string.Empty,
            Category: e.Attribute("category")?.Value ?? string.Empty,
            Width: float.Parse(e.Attribute("width")?.Value ?? "0", CultureInfo.InvariantCulture),
            Depth: float.Parse(e.Attribute("depth")?.Value ?? "0", CultureInfo.InvariantCulture),
            Height: float.Parse(e.Attribute("height")?.Value ?? "0", CultureInfo.InvariantCulture),
            ModelName: e.Attribute("modelName")?.Value
        )).ToList().AsReadOnly();

        return new FurnitureCatalog(pieces);
    }
}