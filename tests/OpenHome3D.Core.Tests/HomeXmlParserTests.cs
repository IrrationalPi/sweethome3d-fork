using OpenHome3D.Core.IO;

namespace OpenHome3D.Core.Tests;

public class HomeXmlParserTests
{
    [Fact]
    public void Parse_MinimalXml_ReturnsCorrectHome()
    {
        string xml = "<home version=\"7.5\" name=\"Test\" />";
        var home = HomeXmlParser.Parse(xml);
        Assert.Equal("Test", home.Name);
        Assert.Equal("7.5", home.Version);
        Assert.Empty(home.Walls);
        Assert.Empty(home.Rooms);
        Assert.Empty(home.Furniture);
        Assert.Empty(home.Properties);
    }

    [Fact]
    public void Parse_XmlWithWallsAndRooms_ReturnsCorrectCollections()
    {
        string xml = @"
            <home version=""7.5"" name=""TestHome"">
                <wall xStart=""0"" yStart=""0"" xEnd=""100"" yEnd=""0"" thickness=""10"" height=""250"" />
                <wall xStart=""100"" yStart=""0"" xEnd=""100"" yEnd=""100"" thickness=""10"" height=""250"" />
                <room id=""1"" name=""Living Room"">
                    <point x=""0"" y=""0"" />
                    <point x=""100"" y=""0"" />
                    <point x=""100"" y=""100"" />
                    <point x=""0"" y=""100"" />
                </room>
            </home>";
            
        var home = HomeXmlParser.Parse(xml);
        
        Assert.Equal("TestHome", home.Name);
        Assert.Equal(2, home.Walls.Count);
        Assert.Equal(100f, home.Walls[0].XEnd);
        Assert.Equal(10f, home.Walls[0].Thickness);
        
        Assert.Single(home.Rooms);
        Assert.Equal("Living Room", home.Rooms[0].Name);
        Assert.Equal(10000f, home.Rooms[0].Area);
        Assert.Equal(4, home.Rooms[0].Points.Count);
    }

    [Fact]
    public void Parse_XmlWithFurnitureAndProperties_ReturnsCorrectCollections()
    {
        string xml = @"
            <home version=""7.5"" name=""TestHome"">
                <pieceOfFurniture id=""1"" x=""10"" y=""20"" z=""0"" angle=""1.57"" width=""100"" depth=""50"" height=""80"" modelName=""Sofa"" />
                <property name=""wallHeight"" value=""250"" />
                <property name=""unit"" value=""Centimeter"" />
            </home>";
            
        var home = HomeXmlParser.Parse(xml);
        
        Assert.Single(home.Furniture);
        Assert.Equal("1", home.Furniture[0].Id);
        Assert.Equal(10f, home.Furniture[0].X);
        Assert.Equal(20f, home.Furniture[0].Y);
        Assert.Equal(0f, home.Furniture[0].Z);
        Assert.Equal(1.57f, home.Furniture[0].Angle);
        Assert.Equal(100f, home.Furniture[0].Width);
        Assert.Equal(50f, home.Furniture[0].Depth);
        Assert.Equal(80f, home.Furniture[0].Height);
        Assert.Equal("Sofa", home.Furniture[0].ModelName);
        
        Assert.Equal(2, home.Properties.Count);
        Assert.Equal("wallHeight", home.Properties[0].Name);
        Assert.Equal("250", home.Properties[0].Value);
        Assert.Equal("unit", home.Properties[1].Name);
        Assert.Equal("Centimeter", home.Properties[1].Value);
    }
}
