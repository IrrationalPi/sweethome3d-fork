using System.Globalization;
using System.Xml.Linq;
using OpenHome3D.Core.Geometry;
using OpenHome3D.Core.Models;

namespace OpenHome3D.Core.IO;

public static class HomeXmlParser
{
    public static Home Parse(string xmlContent)
    {
        var doc = XDocument.Parse(xmlContent);
        var root = doc.Root;
        
        if (root == null || root.Name.LocalName != "home")
        {
            throw new InvalidOperationException("Invalid XML: root element must be 'home'.");
        }

        var name = root.Attribute("name")?.Value ?? string.Empty;
        var version = root.Attribute("version")?.Value ?? string.Empty;

        var walls = root.Elements("wall").Select(e => new Wall(
            e.Attribute("id")?.Value ?? string.Empty,
            float.Parse(e.Attribute("xStart")?.Value ?? "0", CultureInfo.InvariantCulture),
            float.Parse(e.Attribute("yStart")?.Value ?? "0", CultureInfo.InvariantCulture),
            float.Parse(e.Attribute("xEnd")?.Value ?? "0", CultureInfo.InvariantCulture),
            float.Parse(e.Attribute("yEnd")?.Value ?? "0", CultureInfo.InvariantCulture),
            float.Parse(e.Attribute("thickness")?.Value ?? "0", CultureInfo.InvariantCulture),
            float.Parse(e.Attribute("height")?.Value ?? "0", CultureInfo.InvariantCulture)
        )).ToList().AsReadOnly();

        var rooms = root.Elements("room").Select(e => new Room(
            e.Attribute("id")?.Value ?? string.Empty,
            e.Elements("point").Select(p => new Point2D(
                float.Parse(p.Attribute("x")?.Value ?? "0", CultureInfo.InvariantCulture),
                float.Parse(p.Attribute("y")?.Value ?? "0", CultureInfo.InvariantCulture)
            )).ToList().AsReadOnly(),
            e.Attribute("name")?.Value
        )).ToList().AsReadOnly();

        var furniture = root.Elements("pieceOfFurniture").Select(e => new HomePieceOfFurniture(
            e.Attribute("id")?.Value ?? string.Empty,
            float.Parse(e.Attribute("x")?.Value ?? "0", CultureInfo.InvariantCulture),
            float.Parse(e.Attribute("y")?.Value ?? "0", CultureInfo.InvariantCulture),
            float.Parse(e.Attribute("z")?.Value ?? "0", CultureInfo.InvariantCulture),
            float.Parse(e.Attribute("angle")?.Value ?? "0", CultureInfo.InvariantCulture),
            float.Parse(e.Attribute("width")?.Value ?? "0", CultureInfo.InvariantCulture),
            float.Parse(e.Attribute("depth")?.Value ?? "0", CultureInfo.InvariantCulture),
            float.Parse(e.Attribute("height")?.Value ?? "0", CultureInfo.InvariantCulture),
            e.Attribute("modelName")?.Value ?? string.Empty
        )).ToList().AsReadOnly();

        var properties = root.Elements("property").Select(e => new HomeProperty(
            e.Attribute("name")?.Value ?? string.Empty,
            e.Attribute("value")?.Value ?? string.Empty
        )).ToList().AsReadOnly();

        return new Home(name, version, walls, rooms, furniture, properties);
    }
}
