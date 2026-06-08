using System.Globalization;
using System.Xml.Linq;
using OpenHome3D.Core.Models;

namespace OpenHome3D.Core.IO;

public static class UserPreferencesXmlParser
{
    public static UserPreferences Parse(string xmlContent)
    {
        var doc = XDocument.Parse(xmlContent);
        var root = doc.Root;
        
        if (root == null || root.Name.LocalName != "preferences")
        {
            throw new InvalidOperationException("Invalid XML: root element must be 'preferences'.");
        }

        return new UserPreferences(
            Unit: root.Attribute("unit")?.Value ?? "Centimeter",
            MagnetismEnabled: bool.Parse(root.Attribute("magnetismEnabled")?.Value ?? "true"),
            NewWallThickness: float.Parse(root.Attribute("newWallThickness")?.Value ?? "10", CultureInfo.InvariantCulture),
            NewWallHeight: float.Parse(root.Attribute("newWallHeight")?.Value ?? "250", CultureInfo.InvariantCulture)
        );
    }
}