using System;
using System.Collections.Generic;
using System.Linq;

namespace FFRogue.Map
{
    public class DungeonMap
    {
        private readonly int _width;
        private readonly int _height;
        private readonly char[,] _tiles;
        private readonly Random _rng = new Random();
        private readonly List<Room> _rooms = new List<Room>();

        public Point? UpStairs { get; private set; }
        public Point? DownStairs { get; private set; }

        public DungeonMap(int width, int height)
        {
            _width = width; _height = height;
            _tiles = new char[_width, _height];
            for (int x = 0; x < _width; x++) for (int y = 0; y < _height; y++) _tiles[x, y] = '#';
        }

        public void Generate()
        {
            // reset to walls
            for (int x = 0; x < _width; x++) for (int y = 0; y < _height; y++) _tiles[x, y] = '#';
            _rooms.Clear();

            int roomCount = _rng.Next(6, 10);
            int minSize = 4, maxSize = 9;

            for (int i = 0; i < roomCount; i++)
            {
                int w = _rng.Next(minSize, maxSize);
                int h = _rng.Next(minSize, maxSize);
                int x = _rng.Next(1, _width - w - 1);
                int y = _rng.Next(1, _height - h - 1);
                var room = new Room(x, y, w, h);
                if (_rooms.Any(r => r.Intersects(room))) { i--; continue; }
                CreateRoom(room);
                if (_rooms.Count > 0)
                {
                    var (px, py) = _rooms[^1].Center();
                    var (cx, cy) = room.Center();
                    if (_rng.Next(2) == 0) { CreateHTunnel(px, cx, py); CreateVTunnel(py, cy, cx); }
                    else { CreateVTunnel(py, cy, px); CreateHTunnel(px, cx, cy); }
                }
                _rooms.Add(room);
            }

            if (_rooms.Count == 0)
            {
                var r = new Room(2, 2, _width - 4, _height - 4);
                CreateRoom(r); _rooms.Add(r);
            }

            // Only create down stairs now (no more up stairs)
            DownStairs = _rooms[^1].CenterPoint();
            _tiles[DownStairs.Value.X, DownStairs.Value.Y] = '>';
        }

        private void CreateRoom(Room r)
        {
            for (int x = r.X1; x < r.X2; x++) for (int y = r.Y1; y < r.Y2; y++) _tiles[x, y] = '.';
        }

        private void CreateHTunnel(int x1, int x2, int y)
        {
            for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++) _tiles[x, y] = '.';
        }

        private void CreateVTunnel(int y1, int y2, int x)
        {
            for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++) _tiles[x, y] = '.';
        }

        public FFRogue.Map.Point GetCenterRoom() => _rooms.Count > 0 ? _rooms[0].CenterPoint() : new FFRogue.Map.Point(_width / 2, _height / 2);
        public FFRogue.Map.Point RandomRoomCenter() { var r = _rooms[_rng.Next(_rooms.Count)]; return r.CenterPoint(); }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < _width && y < _height;
        public bool IsWalkable(int x, int y) => InBounds(x, y) && (_tiles[x, y] == '.' || _tiles[x, y] == '<' || _tiles[x, y] == '>');

        public char GetGlyph(int x, int y) => InBounds(x, y) ? _tiles[x, y] : ' ';
        public bool IsVisible(int px, int py, int x, int y) { int dx = px - x, dy = py - y; return dx * dx + dy * dy <= 8 * 8; }

        public bool HasUpStairs(int x, int y) => UpStairs.HasValue && UpStairs.Value.X == x && UpStairs.Value.Y == y;
        public bool HasDownStairs(int x, int y) => DownStairs.HasValue && DownStairs.Value.X == x && DownStairs.Value.Y == y;

        // New methods for boss floor support
        public void SetTile(int x, int y, char tile)
        {
            if (InBounds(x, y))
                _tiles[x, y] = tile;
        }

        public void ClearStairs()
        {
            UpStairs = null;
            DownStairs = null;
        }

        public void RemoveUpStairs()
        {
            UpStairs = null;
        }

        public void SetDownStairs(Point point)
        {
            DownStairs = point;
        }
    }

    public class Room
    {
        public int X1, Y1, X2, Y2;
        public Room(int x, int y, int w, int h) { X1 = x; Y1 = y; X2 = x + w; Y2 = y + h; }
        public (int X, int Y) Center() => ((X1 + X2) / 2, (Y1 + Y2) / 2);
        public FFRogue.Map.Point CenterPoint() => new FFRogue.Map.Point((X1 + X2) / 2, (Y1 + Y2) / 2);
        public bool Intersects(Room o) => !(X2 <= o.X1 || X1 >= o.X2 || Y2 <= o.Y1 || Y1 >= o.Y2);
    }
}