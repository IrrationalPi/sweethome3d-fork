using OpenHome3D.Core.Geometry;
using Xunit;

namespace OpenHome3D.Core.Tests.Geometry;

public class GeometryUtilsTests
{
    [Fact]
    public void Distance_CalculatesCorrectly()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(3, 4);
        Assert.Equal(5f, GeometryUtils.Distance(a, b));
        Assert.Equal(25f, GeometryUtils.DistanceSquared(a, b));
    }

    [Fact]
    public void ComputeIntersection_IntersectingLines_ReturnsPoint()
    {
        var p1 = new Point2D(0, 0);
        var p2 = new Point2D(10, 10);
        var p3 = new Point2D(0, 10);
        var p4 = new Point2D(10, 0);
        
        var result = GeometryUtils.ComputeIntersection(p1, p2, p3, p4, 0.0001f);
        
        Assert.NotNull(result);
        Assert.Equal(5f, result.Value.X);
        Assert.Equal(5f, result.Value.Y);
    }

    [Fact]
    public void ComputeIntersection_ParallelLines_ReturnsNull()
    {
        var p1 = new Point2D(0, 0);
        var p2 = new Point2D(10, 10);
        var p3 = new Point2D(0, 5);
        var p4 = new Point2D(10, 15);
        
        var result = GeometryUtils.ComputeIntersection(p1, p2, p3, p4, 0.0001f);
        
        Assert.Null(result);
    }

    [Fact]
    public void ComputeIntersection_VerticalLines_ReturnsPoint()
    {
        var p1 = new Point2D(5, 0);
        var p2 = new Point2D(5, 10);
        var p3 = new Point2D(0, 5);
        var p4 = new Point2D(10, 5);
        
        var result = GeometryUtils.ComputeIntersection(p1, p2, p3, p4, 0.0001f);
        
        Assert.NotNull(result);
        Assert.Equal(5f, result.Value.X);
        Assert.Equal(5f, result.Value.Y);
    }
}