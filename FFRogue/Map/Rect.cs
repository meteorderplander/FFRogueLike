namespace FFRogue.Map
{
    public class Rect
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        public Point Center => new(X + Width / 2, Y + Height / 2);
        public Rect(int x, int y, int w, int h) { X = x; Y = y; Width = w; Height = h; }
        public bool Intersects(Rect o) => !(o.X >= X + Width || o.X + o.Width <= X || o.Y >= Y + Height || o.Y + o.Height <= Y);
    }
}

