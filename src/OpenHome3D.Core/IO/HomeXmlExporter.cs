using System.Globalization;
using System.Xml;
using OpenHome3D.Core.Models;

namespace OpenHome3D.Core.IO;

public static class HomeXmlExporter
{
    public static string Export(Home home)
    {
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, new XmlWriterSettings 
        { 
            Indent = true, 
            OmitXmlDeclaration = false,
            Encoding = System.Text.Encoding.UTF8
        });

        writer.WriteStartDocument();
        writer.WriteStartElement("home");
        writer.WriteAttributeString("version", home.Version);
        writer.WriteAttributeString("name", home.Name);

        foreach (var wall in home.Walls)
        {
            writer.WriteStartElement("wall");
            if (!string.IsNullOrEmpty(wall.Id))
                writer.WriteAttributeString("id", wall.Id);
            writer.WriteAttributeString("xStart", wall.XStart.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("yStart", wall.YStart.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("xEnd", wall.XEnd.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("yEnd", wall.YEnd.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("thickness", wall.Thickness.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("height", wall.Height.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        foreach (var room in home.Rooms)
        {
            writer.WriteStartElement("room");
            if (!string.IsNullOrEmpty(room.Id))
                writer.WriteAttributeString("id", room.Id);
            writer.WriteAttributeString("x", room.X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("y", room.Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("area", room.Area.ToString(CultureInfo.InvariantCulture));
            if (room.Name != null)
                writer.WriteAttributeString("name", room.Name);
            writer.WriteEndElement();
        }

        foreach (var furniture in home.Furniture)
        {
            writer.WriteStartElement("pieceOfFurniture");
            writer.WriteAttributeString("id", furniture.Id);
            writer.WriteAttributeString("x", furniture.X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("y", furniture.Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("z", furniture.Z.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("angle", furniture.Angle.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("width", furniture.Width.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("depth", furniture.Depth.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("height", furniture.Height.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("modelName", furniture.ModelName);
            writer.WriteEndElement();
        }

        foreach (var property in home.Properties)
        {
            writer.WriteStartElement("property");
            writer.WriteAttributeString("name", property.Name);
            writer.WriteAttributeString("value", property.Value);
            writer.WriteEndElement();
        }

        writer.WriteEndElement(); // home
        writer.WriteEndDocument();
        writer.Flush();

        return sw.ToString();
    }
}
