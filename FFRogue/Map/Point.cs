using System;

namespace FFRogue.Map
{
    public readonly struct Point
    {
        public int X { get; }
        public int Y { get; }
        public Point(int x, int y) { X = x; Y = y; }
        public void Deconstruct(out int x, out int y) { x = X; y = Y; }
        public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
        public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
        public int ManhattanTo(Point other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        public override string ToString() => $"({X},{Y})";
    }
}
