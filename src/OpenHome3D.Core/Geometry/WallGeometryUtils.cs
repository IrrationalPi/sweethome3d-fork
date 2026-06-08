using OpenHome3D.Core.Models;

namespace OpenHome3D.Core.Geometry;

public static class WallGeometryUtils
{
    public static Point2D[] GetCornerPoints(Wall wall)
    {
        float dx = wall.XEnd - wall.XStart;
        float dy = wall.YEnd - wall.YStart;
        float length = MathF.Sqrt(dx * dx + dy * dy);
        
        if (length == 0)
        {
            return [];
        }

        float nx = -dy / length;
        float ny = dx / length;
        float halfThickness = wall.Thickness / 2f;

        return 
        [
            new Point2D(wall.XStart + nx * halfThickness, wall.YStart + ny * halfThickness),
            new Point2D(wall.XStart - nx * halfThickness, wall.YStart - ny * halfThickness),
            new Point2D(wall.XEnd - nx * halfThickness, wall.YEnd - ny * halfThickness),
            new Point2D(wall.XEnd + nx * halfThickness, wall.YEnd + ny * halfThickness)
        ];
    }

    public static Wall SnapStartTo(this Wall wall, Point2D target, float margin)
    {
        if (GeometryUtils.Distance(new Point2D(wall.XStart, wall.YStart), target) <= margin)
        {
            return wall with { XStart = target.X, YStart = target.Y };
        }
        return wall;
    }

    public static Wall SnapEndTo(this Wall wall, Point2D target, float margin)
    {
        if (GeometryUtils.Distance(new Point2D(wall.XEnd, wall.YEnd), target) <= margin)
        {
            return wall with { XEnd = target.X, YEnd = target.Y };
        }
        return wall;
    }

    public static Wall SnapToWall(this Wall wall, Wall other, float margin)
    {
        var startOther = new Point2D(other.XStart, other.YStart);
        var endOther = new Point2D(other.XEnd, other.YEnd);

        var snapped = wall;

        if (GeometryUtils.Distance(new Point2D(snapped.XStart, snapped.YStart), startOther) <= margin)
        {
            snapped = snapped with { XStart = startOther.X, YStart = startOther.Y, WallAtStart = other };
        }
        else if (GeometryUtils.Distance(new Point2D(snapped.XStart, snapped.YStart), endOther) <= margin)
        {
            snapped = snapped with { XStart = endOther.X, YStart = endOther.Y, WallAtStart = other };
        }

        if (GeometryUtils.Distance(new Point2D(snapped.XEnd, snapped.YEnd), startOther) <= margin)
        {
            snapped = snapped with { XEnd = startOther.X, YEnd = startOther.Y, WallAtEnd = other };
        }
        else if (GeometryUtils.Distance(new Point2D(snapped.XEnd, snapped.YEnd), endOther) <= margin)
        {
            snapped = snapped with { XEnd = endOther.X, YEnd = endOther.Y, WallAtEnd = other };
        }

        return snapped;
    }
}