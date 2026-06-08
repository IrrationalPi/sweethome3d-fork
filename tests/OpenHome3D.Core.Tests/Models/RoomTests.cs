using OpenHome3D.Core.Geometry;
using OpenHome3D.Core.Models;

namespace OpenHome3D.Core.Tests.Models;

public class RoomTests
{
    [Fact]
    public void GetSignedArea_SimpleSquare_ReturnsCorrectArea()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(10, 0),
            new(10, 10),
            new(0, 10)
        };
        var room = new Room("1", points);
        Assert.Equal(100f, room.Area);
        Assert.False(room.IsClockwise());
    }

    [Fact]
    public void GetSignedArea_ClockwiseSquare_ReturnsNegativeSignedArea()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(0, 10),
            new(10, 10),
            new(10, 0)
        };
        var room = new Room("1", points);
        Assert.Equal(-100f, room.GetSignedArea());
        Assert.True(room.IsClockwise());
    }

    [Fact]
    public void ContainsPoint_PointInside_ReturnsTrue()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(10, 0),
            new(10, 10),
            new(0, 10)
        };
        var room = new Room("1", points);
        Assert.True(room.ContainsPoint(new Point2D(5, 5)));
    }

    [Fact]
    public void ContainsPoint_PointOutside_ReturnsFalse()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(10, 0),
            new(10, 10),
            new(0, 10)
        };
        var room = new Room("1", points);
        Assert.False(room.ContainsPoint(new Point2D(15, 5)));
    }

    [Fact]
    public void ContainsPoint_PointOnBoundary_ReturnsTrue()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(10, 0),
            new(10, 10),
            new(0, 10)
        };
        var room = new Room("1", points);
        Assert.True(room.ContainsPoint(new Point2D(5, 0), 0.1f));
    }
}