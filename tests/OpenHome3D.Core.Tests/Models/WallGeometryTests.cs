using OpenHome3D.Core.Geometry;
using OpenHome3D.Core.Models;
using Xunit;

namespace OpenHome3D.Core.Tests.Models;

public class WallGeometryTests
{
    [Fact]
    public void GetCornerPoints_HorizontalWall_ReturnsCorrectCorners()
    {
        var wall = new Wall("1", 0, 0, 10, 0, 2, 3);
        var corners = WallGeometryUtils.GetCornerPoints(wall);

        Assert.Equal(4, corners.Length);
        Assert.Equal(0, corners[0].X);
        Assert.Equal(1, corners[0].Y);
        Assert.Equal(0, corners[1].X);
        Assert.Equal(-1, corners[1].Y);
        Assert.Equal(10, corners[2].X);
        Assert.Equal(-1, corners[2].Y);
        Assert.Equal(10, corners[3].X);
        Assert.Equal(1, corners[3].Y);
    }

    [Fact]
    public void GetCornerPoints_VerticalWall_ReturnsCorrectCorners()
    {
        var wall = new Wall("1", 0, 0, 0, 10, 2, 3);
        var corners = WallGeometryUtils.GetCornerPoints(wall);

        Assert.Equal(4, corners.Length);
        Assert.Equal(-1, corners[0].X);
        Assert.Equal(0, corners[0].Y);
        Assert.Equal(1, corners[1].X);
        Assert.Equal(0, corners[1].Y);
        Assert.Equal(1, corners[2].X);
        Assert.Equal(10, corners[2].Y);
        Assert.Equal(-1, corners[3].X);
        Assert.Equal(10, corners[3].Y);
    }

    [Fact]
    public void SnapStartTo_WithinMargin_SnapsAndReturnsNewWall()
    {
        var wall = new Wall("1", 0, 0, 10, 10, 2, 3);
        var target = new Point2D(0.5f, 0.5f);
        float margin = 1f;

        var snapped = wall.SnapStartTo(target, margin);

        Assert.Equal(0.5f, snapped.XStart);
        Assert.Equal(0.5f, snapped.YStart);
        Assert.Equal(10, snapped.XEnd);
        Assert.Equal(10, snapped.YEnd);
        Assert.NotSame(wall, snapped);
    }

    [Fact]
    public void SnapStartTo_OutsideMargin_ReturnsOriginalWall()
    {
        var wall = new Wall("1", 0, 0, 10, 10, 2, 3);
        var target = new Point2D(2f, 2f);
        float margin = 1f;

        var snapped = wall.SnapStartTo(target, margin);

        Assert.Same(wall, snapped);
    }

    [Fact]
    public void SnapToWall_StartToStart_SnapsAndSetsWallAtStart()
    {
        var wall1 = new Wall("1", 0, 0, 10, 10, 2, 3);
        var wall2 = new Wall("2", 0.5f, 0.5f, 20, 20, 2, 3);
        float margin = 1f;

        var snapped = wall1.SnapToWall(wall2, margin);

        Assert.Equal(0.5f, snapped.XStart);
        Assert.Equal(0.5f, snapped.YStart);
        Assert.Equal(wall2, snapped.WallAtStart);
    }

    [Fact]
    public void SnapToWall_EndToEnd_SnapsAndSetsWallAtEnd()
    {
        var wall1 = new Wall("1", 0, 0, 10, 10, 2, 3);
        var wall2 = new Wall("2", 20, 20, 9.5f, 9.5f, 2, 3);
        float margin = 1f;

        var snapped = wall1.SnapToWall(wall2, margin);

        Assert.Equal(9.5f, snapped.XEnd);
        Assert.Equal(9.5f, snapped.YEnd);
        Assert.Equal(wall2, snapped.WallAtEnd);
    }
}