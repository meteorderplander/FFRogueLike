
namespace FFRogue.Entities
{
    public class Entity
    {
        public string Name { get; set; } = "Entity";
        public int X { get; set; }
        public int Y { get; set; }

        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }
        public int MaxMP { get; set; }
        public int CurrentMP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }

        public bool IsAlive => CurrentHP > 0;
        public char Glyph { get; set; } = '?';
    }
}
