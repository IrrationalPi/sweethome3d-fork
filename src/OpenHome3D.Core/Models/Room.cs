using System.Linq;
using OpenHome3D.Core.Geometry;

namespace OpenHome3D.Core.Models;

public record Room(
    string Id,
    IReadOnlyList<Point2D> Points,
    string? Name = null
)
{
    public float X => Points.Count > 0 ? Points.Average(p => p.X) : 0f;
    public float Y => Points.Count > 0 ? Points.Average(p => p.Y) : 0f;
    public float Area => Math.Abs(GetSignedArea());

    public float GetSignedArea() => GetSignedArea(Points);

    public static float GetSignedArea(IReadOnlyList<Point2D> points)
    {
        float area = 0f;
        int n = points.Count;
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            area += (points[i].X * points[j].Y) - (points[j].X * points[i].Y);
        }
        return area / 2f;
    }

    public bool IsClockwise() => GetSignedArea() < 0;

    public bool ContainsPoint(Point2D point, float margin = 0.001f)
    {
        bool inside = false;
        int n = Points.Count;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            float xi = Points[i].X, yi = Points[i].Y;
            float xj = Points[j].X, yj = Points[j].Y;

            bool intersect = ((yi > point.Y) != (yj > point.Y))
                && (point.X < (xj - xi) * (point.Y - yi) / (yj - yi) + xi);
            if (intersect)
            {
                inside = !inside;
            }
        }

        if (!inside)
        {
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                if (DistanceToSegment(point, Points[i], Points[j]) <= margin)
                {
                    return true;
                }
            }
        }

        return inside;
    }

    private static float DistanceToSegment(Point2D p, Point2D v, Point2D w)
    {
        float l2 = (v.X - w.X) * (v.X - w.X) + (v.Y - w.Y) * (v.Y - w.Y);
        if (l2 == 0f) return (float)Math.Sqrt((p.X - v.X) * (p.X - v.X) + (p.Y - v.Y) * (p.Y - v.Y));
        float t = ((p.X - v.X) * (w.X - v.X) + (p.Y - v.Y) * (w.Y - v.Y)) / l2;
        t = Math.Max(0f, Math.Min(1f, t));
        float projX = v.X + t * (w.X - v.X);
        float projY = v.Y + t * (w.Y - v.Y);
        return (float)Math.Sqrt((p.X - projX) * (p.X - projX) + (p.Y - projY) * (p.Y - projY));
    }
}
