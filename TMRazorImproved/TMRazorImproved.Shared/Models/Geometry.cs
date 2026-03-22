using System;

namespace TMRazorImproved.Shared.Models
{
    /// <summary>
    /// FR-084: Strutture geometriche per compatibilità script legacy (RazorEnhanced.Point2D/Point3D/Rectangle2D).
    /// </summary>

    public struct Point2D : IEquatable<Point2D>
    {
        public static readonly Point2D Zero    = new(0, 0);
        public static readonly Point2D MinusOne = new(-1, -1);

        public int X { get; set; }
        public int Y { get; set; }

        public Point2D(int x, int y) { X = x; Y = y; }

        public double DistanceTo(Point2D other) => Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        public int ChebDistanceTo(Point2D other) => Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));

        public bool Equals(Point2D other) => X == other.X && Y == other.Y;
        public override bool Equals(object? obj) => obj is Point2D p && Equals(p);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";

        public static bool operator ==(Point2D l, Point2D r) => l.X == r.X && l.Y == r.Y;
        public static bool operator !=(Point2D l, Point2D r) => !(l == r);
    }

    public struct Point3D : IEquatable<Point3D>
    {
        public static readonly Point3D Zero    = new(0, 0, 0);
        public static readonly Point3D MinusOne = new(-1, -1, 0);

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public Point3D(int x, int y, int z) { X = x; Y = y; Z = z; }

        public Point2D ToPoint2D() => new(X, Y);
        public double DistanceTo(Point3D other) => Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2) + Math.Pow(Z - other.Z, 2));
        public int ChebDistanceTo(Point3D other) => Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));

        public bool Equals(Point3D other) => X == other.X && Y == other.Y && Z == other.Z;
        public override bool Equals(object? obj) => obj is Point3D p && Equals(p);
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);
        public override string ToString() => $"({X}, {Y}, {Z})";

        public static bool operator ==(Point3D l, Point3D r) => l.X == r.X && l.Y == r.Y && l.Z == r.Z;
        public static bool operator !=(Point3D l, Point3D r) => !(l == r);
    }

    /// <summary>
    /// Rettangolo 2D con origine (X, Y) e dimensioni (Width, Height).
    /// </summary>
    public struct Rectangle2D : IEquatable<Rectangle2D>
    {
        public int X      { get; set; }
        public int Y      { get; set; }
        public int Width  { get; set; }
        public int Height { get; set; }

        public int Start_X => X;
        public int Start_Y => Y;
        public int End_X   => X + Width;
        public int End_Y   => Y + Height;

        public Rectangle2D(int x, int y, int width, int height) { X = x; Y = y; Width = width; Height = height; }
        public Rectangle2D(Point2D start, Point2D end)
        {
            X = start.X; Y = start.Y;
            Width = end.X - start.X; Height = end.Y - start.Y;
        }

        public bool Contains(int x, int y) => x >= X && x < X + Width && y >= Y && y < Y + Height;
        public bool Contains(Point2D p)    => Contains(p.X, p.Y);
        public bool Contains(Point3D p)    => Contains(p.X, p.Y);

        public bool Intersects(Rectangle2D r) =>
            r.X < X + Width && r.X + r.Width > X &&
            r.Y < Y + Height && r.Y + r.Height > Y;

        public bool Equals(Rectangle2D other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        public override bool Equals(object? obj) => obj is Rectangle2D r && Equals(r);
        public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
        public override string ToString() => $"({X}, {Y}) {Width}x{Height}";

        public static bool operator ==(Rectangle2D l, Rectangle2D r) => l.Equals(r);
        public static bool operator !=(Rectangle2D l, Rectangle2D r) => !(l == r);
    }
}
