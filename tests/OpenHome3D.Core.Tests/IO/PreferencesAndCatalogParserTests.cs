using OpenHome3D.Core.IO;
using OpenHome3D.Core.Models;

namespace OpenHome3D.Core.Tests.IO;

public class PreferencesAndCatalogParserTests
{
    [Fact]
    public void UserPreferencesXmlParser_Parse_ValidXml_ReturnsCorrectPreferences()
    {
        string xml = @"
            <preferences unit=""Meter"" magnetismEnabled=""false"" newWallThickness=""15"" newWallHeight=""300"" />";
            
        var prefs = UserPreferencesXmlParser.Parse(xml);
        
        Assert.Equal("Meter", prefs.Unit);
        Assert.False(prefs.MagnetismEnabled);
        Assert.Equal(15f, prefs.NewWallThickness);
        Assert.Equal(300f, prefs.NewWallHeight);
    }

    [Fact]
    public void UserPreferencesXmlParser_Parse_MinimalXml_ReturnsDefaultPreferences()
    {
        string xml = @"<preferences />";
            
        var prefs = UserPreferencesXmlParser.Parse(xml);
        
        Assert.Equal("Centimeter", prefs.Unit);
        Assert.True(prefs.MagnetismEnabled);
        Assert.Equal(10f, prefs.NewWallThickness);
        Assert.Equal(250f, prefs.NewWallHeight);
    }

    [Fact]
    public void UserPreferencesXmlParser_Parse_InvalidXml_Throws()
    {
        string xml = @"<invalid />";
        Assert.Throws<InvalidOperationException>(() => UserPreferencesXmlParser.Parse(xml));
    }

    [Fact]
    public void FurnitureCatalogXmlParser_Parse_ValidXml_ReturnsCorrectCatalog()
    {
        string xml = @"
            <catalog>
                <piece id=""1"" name=""Sofa"" category=""Living Room"" width=""200"" depth=""100"" height=""80"" modelName=""sofa.obj"" />
                <piece id=""2"" name=""Table"" category=""Dining Room"" width=""150"" depth=""80"" height=""75"" />
            </catalog>";
            
        var catalog = FurnitureCatalogXmlParser.Parse(xml);
        
        Assert.Equal(2, catalog.Pieces.Count);
        Assert.Equal("1", catalog.Pieces[0].Id);
        Assert.Equal("Sofa", catalog.Pieces[0].Name);
        Assert.Equal("Living Room", catalog.Pieces[0].Category);
        Assert.Equal(200f, catalog.Pieces[0].Width);
        Assert.Equal(100f, catalog.Pieces[0].Depth);
        Assert.Equal(80f, catalog.Pieces[0].Height);
        Assert.Equal("sofa.obj", catalog.Pieces[0].ModelName);
        
        Assert.Equal("2", catalog.Pieces[1].Id);
        Assert.Equal("Table", catalog.Pieces[1].Name);
        Assert.Null(catalog.Pieces[1].ModelName);
    }

    [Fact]
    public void FurnitureCatalogXmlParser_Parse_InvalidXml_Throws()
    {
        string xml = @"<invalid />";
        Assert.Throws<InvalidOperationException>(() => FurnitureCatalogXmlParser.Parse(xml));
    }
}