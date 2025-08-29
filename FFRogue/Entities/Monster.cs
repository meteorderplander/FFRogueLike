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
                case 1: // Floors 1-10
                    pool.Add(() => new Monster("Imp", 'i', baseHP, baseATK, baseDEF));
                    pool.Add(() => new Monster("Spriggan", 's', baseHP + 2, baseATK + 1, baseDEF));
                    pool.Add(() => new Monster("Goblin", 'g', baseHP + 3, baseATK, baseDEF));
                    pool.Add(() => new Monster("Armadillo", 'a', baseHP + 1, baseATK, baseDEF + 1));
                    pool.Add(() => new Monster("Wolf", 'w', baseHP + 2, baseATK + 1, baseDEF));
                    pool.Add(() => new Monster("Kobold", 'k', baseHP + 3, baseATK + 1, baseDEF + 1));
                    pool.Add(() => new Monster("Bat Swarm", 'b', baseHP, baseATK + 2, baseDEF));
                    pool.Add(() => new Monster("Slime", 'S', baseHP + 4, baseATK, baseDEF + 2));
                    break;

                case 2: // Floors 11-20
                    pool.Add(() => new Monster("Orc", 'o', baseHP + 3, baseATK + 2, baseDEF));
                    pool.Add(() => new Monster("Zombie", 'z', baseHP + 4, baseATK + 1, baseDEF));
                    pool.Add(() => new Monster("Bomb", 'B', baseHP + 5, baseATK + 3, baseDEF));
                    pool.Add(() => new Monster("Lizardman", 'l', baseHP + 6, baseATK + 2, baseDEF + 1));
                    pool.Add(() => new Monster("Basilisk", 'b', baseHP + 8, baseATK + 3, baseDEF + 2));
                    pool.Add(() => new Monster("Ghoul", 'G', baseHP + 7, baseATK + 2, baseDEF + 1));
                    pool.Add(() => new Monster("Wraith", 'W', baseHP + 5, baseATK + 4, baseDEF));
                    pool.Add(() => new Monster("Minotaur", 'm', baseHP + 10, baseATK + 5, baseDEF + 2));
                    break;

                case 3: // Floors 21-30
                    pool.Add(() => new Monster("Lamia", 'L', baseHP + 7, baseATK + 4, baseDEF + 1));
                    pool.Add(() => new Monster("Ogre", 'O', baseHP + 12, baseATK + 4, baseDEF + 2));
                    pool.Add(() => new Monster("Medusa", 'M', baseHP + 9, baseATK + 5, baseDEF + 2));
                    pool.Add(() => new Monster("Treant", 'T', baseHP + 15, baseATK + 3, baseDEF + 3));
                    pool.Add(() => new Monster("Chimera", 'C', baseHP + 14, baseATK + 6, baseDEF + 3));
                    pool.Add(() => new Monster("Golem", 'G', baseHP + 20, baseATK + 4, baseDEF + 6));
                    pool.Add(() => new Monster("Manticore", 'M', baseHP + 16, baseATK + 7, baseDEF + 2));
                    pool.Add(() => new Monster("Wendigo", 'W', baseHP + 12, baseATK + 6, baseDEF + 2));
                    break;

                case 4: // Floors 31-40
                    pool.Add(() => new Monster("Dragon Whelp", 'd', baseHP + 18, baseATK + 8, baseDEF + 3));
                    pool.Add(() => new Monster("Behemoth", 'B', baseHP + 24, baseATK + 9, baseDEF + 4));
                    pool.Add(() => new Monster("Evil Eye", 'e', baseHP + 15, baseATK + 7, baseDEF + 2));
                    pool.Add(() => new Monster("Banshee", 'b', baseHP + 12, baseATK + 8, baseDEF + 1));
                    pool.Add(() => new Monster("Malboro", 'M', baseHP + 20, baseATK + 8, baseDEF + 3));
                    pool.Add(() => new Monster("Hydra", 'H', baseHP + 25, baseATK + 9, baseDEF + 4));
                    pool.Add(() => new Monster("Vampire", 'V', baseHP + 18, baseATK + 10, baseDEF + 2));
                    pool.Add(() => new Monster("Lich", 'L', baseHP + 22, baseATK + 9, baseDEF + 3));
                    break;

                case 5: // Floors 41-50
                    pool.Add(() => new Monster("Ancient Dragon", 'D', baseHP + 30, baseATK + 12, baseDEF + 5));
                    pool.Add(() => new Monster("Arch Demon", 'A', baseHP + 28, baseATK + 14, baseDEF + 4));
                    pool.Add(() => new Monster("Death Knight", 'K', baseHP + 25, baseATK + 11, baseDEF + 6));
                    pool.Add(() => new Monster("Void Horror", 'V', baseHP + 32, baseATK + 13, baseDEF + 3));
                    pool.Add(() => new Monster("Titan", 'T', baseHP + 35, baseATK + 10, baseDEF + 8));
                    pool.Add(() => new Monster("Shadowlord", 'S', baseHP + 27, baseATK + 15, baseDEF + 4));
                    pool.Add(() => new Monster("Fallen Angel", 'F', baseHP + 29, baseATK + 12, baseDEF + 5));
                    pool.Add(() => new Monster("Apocalypse", 'Ω', baseHP + 40, baseATK + 16, baseDEF + 6));
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