using FFRogue.Map;
using System;
using System.Collections.Generic;

namespace FFRogue.Entities
{
    public class Monster : Entity
    {
        private static readonly Random Rng = new Random();
        public int SightRange { get; private set; } = 8;
        public bool IsBoss { get; private set; } = false;

        public Monster(string name, char glyph, int hp, int atk, int def, bool isBoss = false)
        {
            Name = name; Glyph = glyph; MaxHP = hp; CurrentHP = hp; Attack = atk; Defense = def; IsBoss = isBoss;
        }

        // New method: Get monsters for specific floor tier
        public static Monster RandomMonsterForFloorTier(int floor)
        {
            // Determine which tier this floor belongs to
            int tier = ((floor - 1) / 10) + 1; // Floor 1-10 = tier 1, 11-20 = tier 2, etc.

            // Base stats scale with floor
            int baseHP = 8 + floor * 2;
            int baseATK = 3 + floor / 2;
            int baseDEF = 1 + floor / 3;

            // Get monster pool for this tier
            List<Func<Monster>> pool = GetMonsterPoolForTier(tier, baseHP, baseATK, baseDEF);

            // Pick one randomly
            int choice = Rng.Next(pool.Count);
            return pool[choice]();
        }

        private static List<Func<Monster>> GetMonsterPoolForTier(int tier, int baseHP, int baseATK, int baseDEF)
        {
            var pool = new List<Func<Monster>>();

            switch (tier)
            {
                case 1: // Floors 1-10 - Classic FF early game enemies
                    pool.Add(() => new Monster("Goblin", 'g', baseHP, baseATK, baseDEF));
                    pool.Add(() => new Monster("Imp", 'i', baseHP + 1, baseATK, baseDEF));
                    pool.Add(() => new Monster("Skeleton", 's', baseHP + 2, baseATK, baseDEF));
                    pool.Add(() => new Monster("Wild Boar", 'b', baseHP + 3, baseATK + 1, baseDEF));
                    pool.Add(() => new Monster("Hornet", 'h', baseHP, baseATK + 2, baseDEF));
                    pool.Add(() => new Monster("Coeurl", 'c', baseHP + 2, baseATK + 1, baseDEF));
                    pool.Add(() => new Monster("Chocobo", 'C', baseHP + 4, baseATK, baseDEF + 1));
                    break;

                case 2: // Floors 11-20 - Mid-tier FF monsters
                    pool.Add(() => new Monster("Ochu", 'O', baseHP + 6, baseATK + 2, baseDEF + 1));
                    pool.Add(() => new Monster("Flan", 'F', baseHP + 8, baseATK, baseDEF + 3));
                    pool.Add(() => new Monster("Bomb", 'B', baseHP + 3, baseATK + 4, baseDEF));
                    pool.Add(() => new Monster("Sahagin", 'S', baseHP + 5, baseATK + 2, baseDEF + 1));
                    pool.Add(() => new Monster("Tonberry", 't', baseHP + 4, baseATK + 3, baseDEF + 2));
                    pool.Add(() => new Monster("Malboro", 'M', baseHP + 10, baseATK + 1, baseDEF + 2));
                    pool.Add(() => new Monster("Ahriman", 'A', baseHP + 7, baseATK + 3, baseDEF));
                    pool.Add(() => new Monster("Zu", 'Z', baseHP + 6, baseATK + 2, baseDEF + 1));
                    break;

                case 3: // Floors 21-30 - Stronger FF creatures
                    pool.Add(() => new Monster("Chimera", 'C', baseHP + 15, baseATK + 5, baseDEF + 3));
                    pool.Add(() => new Monster("Wyvern", 'W', baseHP + 12, baseATK + 6, baseDEF + 2));
                    pool.Add(() => new Monster("Gargoyle", 'G', baseHP + 14, baseATK + 4, baseDEF + 4));
                    pool.Add(() => new Monster("Minotaur", 'M', baseHP + 16, baseATK + 5, baseDEF + 3));
                    pool.Add(() => new Monster("Cockatrice", 'K', baseHP + 11, baseATK + 6, baseDEF + 2));
                    pool.Add(() => new Monster("Ogre", 'O', baseHP + 18, baseATK + 4, baseDEF + 4));
                    pool.Add(() => new Monster("Lamia", 'L', baseHP + 13, baseATK + 5, baseDEF + 3));
                    pool.Add(() => new Monster("Basilisk", 'B', baseHP + 14, baseATK + 5, baseDEF + 3));
                    break;

                case 4: // Floors 31-40 - High-tier FF monsters
                    pool.Add(() => new Monster("Behemoth", 'B', baseHP + 25, baseATK + 8, baseDEF + 4));
                    pool.Add(() => new Monster("Red Dragon", 'R', baseHP + 22, baseATK + 9, baseDEF + 3));
                    pool.Add(() => new Monster("Iron Giant", 'I', baseHP + 28, baseATK + 7, baseDEF + 6));
                    pool.Add(() => new Monster("Great Malboro", 'M', baseHP + 24, baseATK + 6, baseDEF + 5));
                    pool.Add(() => new Monster("Mindflayer", 'F', baseHP + 20, baseATK + 10, baseDEF + 2));
                    pool.Add(() => new Monster("Lich", 'L', baseHP + 18, baseATK + 9, baseDEF + 4));
                    pool.Add(() => new Monster("Hecteyes", 'H', baseHP + 21, baseATK + 8, baseDEF + 3));
                    pool.Add(() => new Monster("Vampyr", 'V', baseHP + 23, baseATK + 9, baseDEF + 3));
                    break;

                case 5: // Floors 41-50 - Elite FF creatures
                    pool.Add(() => new Monster("King Behemoth", 'K', baseHP + 35, baseATK + 12, baseDEF + 6));
                    pool.Add(() => new Monster("Ancient Dragon", 'A', baseHP + 32, baseATK + 14, baseDEF + 5));
                    pool.Add(() => new Monster("Wraith", 'W', baseHP + 28, baseATK + 15, baseDEF + 3));
                    pool.Add(() => new Monster("Demon", 'D', baseHP + 30, baseATK + 13, baseDEF + 4));
                    pool.Add(() => new Monster("Death Claw", 'C', baseHP + 26, baseATK + 16, baseDEF + 2));
                    pool.Add(() => new Monster("Great Wyrm", 'G', baseHP + 38, baseATK + 11, baseDEF + 7));
                    pool.Add(() => new Monster("Shadow", 'S', baseHP + 24, baseATK + 17, baseDEF + 1));
                    pool.Add(() => new Monster("Reaper", 'R', baseHP + 33, baseATK + 14, baseDEF + 4));
                    break;

                default: // Beyond floor 50 - mix of all high-tier monsters
                    // Add all tier 4 and 5 monsters
                    pool.AddRange(GetMonsterPoolForTier(4, baseHP, baseATK, baseDEF));
                    pool.AddRange(GetMonsterPoolForTier(5, baseHP, baseATK, baseDEF));
                    break;
            }

            return pool;
        }

        // Keep the old method for backward compatibility, but redirect to new system
        public static Monster RandomMonsterAtLevel(int floor)
        {
            return RandomMonsterForFloorTier(floor);
        }

        public static Monster CreateBossForFloor(int floor)
        {
            // Tier-based boss names for variety
            int tier = ((floor - 1) / 10) + 1;

            List<(string name, char glyph)> bossPool = tier switch
            {
                1 => new List<(string, char)>
                {
                    ("Tococo", '♔'), ("Amanita", '♕'), ("Bigmouth Billy", '♖'),
                    ("Bubbly Bernie", '♗'), ("Tom Tit Tat", '♘')
                },
                2 => new List<(string, char)>
                {
                    ("Leaping Lizzy", '♔'), ("Carnero", '♕'), ("Swamfisk", '♖'),
                    ("Lumbering Lambert", '♗'), ("Golden Bat", '♘')
                },
                3 => new List<(string, char)>
                {
                    ("Iron Giant", '♔'), ("Scarmiglione", '♕'), ("Cagnazzo", '♖'),
                    ("Barbariccia", '♗'), ("Rubicante", '♘')
                },
                4 => new List<(string, char)>
                {
                    ("Bahamut", '♔'), ("Phoenix", '♕'), ("Leviathan", '♖'),
                    ("Alexander", '♗'), ("Odin", '♘')
                },
                _ => new List<(string, char)>
                {
                    ("Chaos", '♔'), ("Garland", '♕'), ("Exdeath", '♖'),
                    ("Kefka", '♗'), ("Sephiroth", '♘')
                }
            };

            var (name, glyph) = bossPool[Rng.Next(bossPool.Count)];

            // Boss stats scale more dramatically
            int hp = 50 + floor * 15;
            int atk = 8 + floor * 3;
            int def = 5 + floor * 2;

            return new Monster(name, glyph, hp, atk, def, true);
        }

        public (int dx, int dy) GetNextStepToward(int tx, int ty, DungeonMap map)
        {
            if (System.Math.Abs(X - tx) + System.Math.Abs(Y - ty) <= SightRange)
            {
                int dx = System.Math.Sign(tx - X);
                int dy = System.Math.Sign(ty - Y);
                if (System.Math.Abs(tx - X) > System.Math.Abs(ty - Y)) return (dx, 0);
                if (System.Math.Abs(ty - Y) > System.Math.Abs(tx - X)) return (0, dy);
                return (dx, dy);
            }
            else
            {
                return (Rng.Next(-1, 2), Rng.Next(-1, 2));
            }
        }
    }
}