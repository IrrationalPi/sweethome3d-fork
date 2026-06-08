using System.IO;
using System.Linq;
using OpenHome3D.Core.IO;
using OpenHome3D.Core.Models;

namespace OpenHome3D.Core.Tests;

public class Sh3dIOTests
{
    [Fact]
    public void Load_ValidSh3dFile_ReturnsPopulatedHomeAndContent()
    {
        string testFilePath = Path.Combine("Resources", "test.sh3d");
        
        var result = Sh3dIO.Load(testFilePath);
        
        Assert.NotNull(result.Home);
        Assert.Equal("TestHome", result.Home.Name);
        Assert.Equal("7.5", result.Home.Version);
        Assert.Single(result.Home.Walls);
        Assert.Equal(100f, result.Home.Walls[0].XEnd);
        Assert.Single(result.Home.Furniture);
        Assert.Equal("1", result.Home.Furniture[0].Id);
        Assert.Equal("Sofa", result.Home.Furniture[0].ModelName);
        
        Assert.NotEmpty(result.Content);
        Assert.Contains("content/1.png", result.Content.Keys);
    }

    [Fact]
    public void Load_NonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => Sh3dIO.Load("nonexistent.sh3d"));
    }

    [Theory]
    [InlineData("Resources/empty.sh3d")]
    [InlineData("Resources/test.sh3d")]
    [InlineData("Resources/actual_sh3d_save.sh3d")]
    public void Roundtrip_SaveAndLoad_PreservesCoreProperties(string testFilePath)
    {
        var original = Sh3dIO.Load(testFilePath);
        
        string tempFilePath = Path.GetTempFileName() + ".sh3d";
        try
        {
            Sh3dIO.Save(original.Home, original.Content, tempFilePath);
            var roundtripped = Sh3dIO.Load(tempFilePath);
            
            Assert.Equal(original.Home.Name, roundtripped.Home.Name);
            Assert.Equal(original.Home.Version, roundtripped.Home.Version);
            
            Assert.Equal(original.Home.Walls.Count, roundtripped.Home.Walls.Count);
            for (int i = 0; i < original.Home.Walls.Count; i++)
            {
                var origWall = original.Home.Walls[i];
                var rtWall = roundtripped.Home.Walls[i];
                Assert.Equal(origWall.XStart, rtWall.XStart);
                Assert.Equal(origWall.YStart, rtWall.YStart);
                Assert.Equal(origWall.XEnd, rtWall.XEnd);
                Assert.Equal(origWall.YEnd, rtWall.YEnd);
                Assert.Equal(origWall.Thickness, rtWall.Thickness);
                Assert.Equal(origWall.Height, rtWall.Height);
            }
            
            Assert.Equal(original.Home.Furniture.Count, roundtripped.Home.Furniture.Count);
            for (int i = 0; i < original.Home.Furniture.Count; i++)
            {
                var origFurn = original.Home.Furniture[i];
                var rtFurn = roundtripped.Home.Furniture[i];
                Assert.Equal(origFurn.Id, rtFurn.Id);
                Assert.Equal(origFurn.X, rtFurn.X);
                Assert.Equal(origFurn.Y, rtFurn.Y);
                Assert.Equal(origFurn.Z, rtFurn.Z);
                Assert.Equal(origFurn.Angle, rtFurn.Angle);
                Assert.Equal(origFurn.Width, rtFurn.Width);
                Assert.Equal(origFurn.Depth, rtFurn.Depth);
                Assert.Equal(origFurn.Height, rtFurn.Height);
                Assert.Equal(origFurn.ModelName, rtFurn.ModelName);
            }
            
            Assert.Equal(original.Home.Rooms.Count, roundtripped.Home.Rooms.Count);
            Assert.Equal(original.Home.Properties.Count, roundtripped.Home.Properties.Count);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }
}
