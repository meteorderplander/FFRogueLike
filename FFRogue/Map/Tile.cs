namespace FFRogue.Map
{
    public class Tile
    {
        public bool Walkable { get; set; }
        public char Glyph { get; set; }
        public static Tile Wall() => new Tile { Walkable = false, Glyph = '#' };
        public static Tile Floor() => new Tile { Walkable = true, Glyph = '.' };
    }
}