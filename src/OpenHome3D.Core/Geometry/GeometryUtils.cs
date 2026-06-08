namespace OpenHome3D.Core.Geometry;

public static class GeometryUtils
{
    public static float Distance(Point2D a, Point2D b)
    {
        return MathF.Sqrt(DistanceSquared(a, b));
    }

    public static float DistanceSquared(Point2D a, Point2D b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return dx * dx + dy * dy;
    }

    public static Point2D? ComputeIntersection(Point2D p1, Point2D p2, Point2D p3, Point2D p4, float limit)
    {
        float x1 = p1.X, y1 = p1.Y;
        float x2 = p2.X, y2 = p2.Y;
        float x3 = p3.X, y3 = p3.Y;
        float x4 = p4.X, y4 = p4.Y;

        float denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (MathF.Abs(denom) < limit)
        {
            return null;
        }

        float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
        float px = x1 + t * (x2 - x1);
        float py = y1 + t * (y2 - y1);

        return new Point2D(px, py);
    }
}