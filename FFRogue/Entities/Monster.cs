
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

        public static Monster RandomMonsterAtLevel(int floor)
        {
            // Base stats scale with floor
            int baseHP = 8 + floor * 2;
            int baseATK = 3 + floor / 2;
            int baseDEF = 1 + floor / 3;

            // Monster pool for this floor
            List<Func<Monster>> pool = new List<Func<Monster>>();

            if (floor <= 5)
            {
                pool.Add(() => new Monster("Imp", 'i', baseHP, baseATK, baseDEF));
                pool.Add(() => new Monster("Spriggan", 's', baseHP + 2, baseATK + 1, baseDEF));
                pool.Add(() => new Monster("Goblin", 'g', baseHP + 3, baseATK, baseDEF));
                pool.Add(() => new Monster("Armadillo", 'a', baseHP + 1, baseATK, baseDEF + 1));
            }
            else if (floor <= 10)
            {
                pool.Add(() => new Monster("Wolf", 'w', baseHP + 2, baseATK + 1, baseDEF));
                pool.Add(() => new Monster("Kobold", 'k', baseHP + 3, baseATK + 1, baseDEF + 1));
                pool.Add(() => new Monster("Bat Swarm", 'b', baseHP, baseATK + 2, baseDEF));
                pool.Add(() => new Monster("Slime", 'S', baseHP + 4, baseATK, baseDEF + 2));
            }
            else if (floor <= 15)
            {
                pool.Add(() => new Monster("Orc", 'o', baseHP + 3, baseATK + 2, baseDEF));
                pool.Add(() => new Monster("Zombie", 'z', baseHP + 4, baseATK + 1, baseDEF));
                pool.Add(() => new Monster("Bomb", 'B', baseHP + 5, baseATK + 3, baseDEF));
                pool.Add(() => new Monster("Lizardman", 'l', baseHP + 6, baseATK + 2, baseDEF + 1));
            }
            else if (floor <= 20)
            {
                pool.Add(() => new Monster("Basilisk", 'b', baseHP + 8, baseATK + 3, baseDEF + 2));
                pool.Add(() => new Monster("Ghoul", 'G', baseHP + 7, baseATK + 2, baseDEF + 1));
                pool.Add(() => new Monster("Wraith", 'W', baseHP + 5, baseATK + 4, baseDEF));
                pool.Add(() => new Monster("Minotaur", 'm', baseHP + 10, baseATK + 5, baseDEF + 2));
            }
            else if (floor <= 25)
            {
                pool.Add(() => new Monster("Lamia", 'L', baseHP + 7, baseATK + 4, baseDEF + 1));
                pool.Add(() => new Monster("Ogre", 'O', baseHP + 12, baseATK + 4, baseDEF + 2));
                pool.Add(() => new Monster("Medusa", 'M', baseHP + 9, baseATK + 5, baseDEF + 2));
                pool.Add(() => new Monster("Treant", 'T', baseHP + 15, baseATK + 3, baseDEF + 3));
            }
            else if (floor <= 30)
            {
                pool.Add(() => new Monster("Chimera", 'C', baseHP + 14, baseATK + 6, baseDEF + 3));
                pool.Add(() => new Monster("Golem", 'G', baseHP + 20, baseATK + 4, baseDEF + 6));
                pool.Add(() => new Monster("Manticore", 'M', baseHP + 16, baseATK + 7, baseDEF + 2));
                pool.Add(() => new Monster("Wendigo", 'W', baseHP + 12, baseATK + 6, baseDEF + 2));
            }
            else if (floor <= 35)
            {
                pool.Add(() => new Monster("Dragon Whelp", 'd', baseHP + 18, baseATK + 8, baseDEF + 3));
                pool.Add(() => new Monster("Behemoth", 'B', baseHP + 24, baseATK + 9, baseDEF + 4));
                pool.Add(() => new Monster("Evil Eye", 'e', baseHP + 15, baseATK + 7, baseDEF + 2));
                pool.Add(() => new Monster("Banshee", 'b', baseHP + 12, baseATK + 8, baseDEF + 1));
            }
            else if (floor <= 40)
            {
                pool.Add(() => new Monster("Malboro", 'M', baseHP + 20, baseATK + 8, baseDEF + 3));
                pool.Add(() => new Monster("Hydra", 'H', baseHP + 25, baseATK + 9, baseDEF + 4));
                pool.Add(() => new Monster("Vampire", 'V', baseHP + 18, baseATK + 10, baseDEF + 2));
                pool.Add(() => new Monster("Lich", 'L', baseHP + 22, baseATK + 9, baseDEF + 3));
            }
            else // 45+ floors
            {
                // Unlock ALL monsters
                pool.AddRange(new List<Func<Monster>>
        {
            () => new Monster("Imp", 'i', baseHP, baseATK, baseDEF),
            () => new Monster("Spriggan", 's', baseHP + 2, baseATK + 1, baseDEF),
            () => new Monster("Goblin", 'g', baseHP + 3, baseATK, baseDEF),
            () => new Monster("Armadillo", 'a', baseHP + 1, baseATK, baseDEF + 1),
            () => new Monster("Wolf", 'w', baseHP + 2, baseATK + 1, baseDEF),
            () => new Monster("Kobold", 'k', baseHP + 3, baseATK + 1, baseDEF + 1),
            () => new Monster("Bat Swarm", 'b', baseHP, baseATK + 2, baseDEF),
            () => new Monster("Slime", 'S', baseHP + 4, baseATK, baseDEF + 2),
            () => new Monster("Orc", 'o', baseHP + 3, baseATK + 2, baseDEF),
            () => new Monster("Zombie", 'z', baseHP + 4, baseATK + 1, baseDEF),
            () => new Monster("Bomb", 'B', baseHP + 5, baseATK + 3, baseDEF),
            () => new Monster("Lizardman", 'l', baseHP + 6, baseATK + 2, baseDEF + 1),
            () => new Monster("Basilisk", 'b', baseHP + 8, baseATK + 3, baseDEF + 2),
            () => new Monster("Ghoul", 'G', baseHP + 7, baseATK + 2, baseDEF + 1),
            () => new Monster("Wraith", 'W', baseHP + 5, baseATK + 4, baseDEF),
            () => new Monster("Minotaur", 'm', baseHP + 10, baseATK + 5, baseDEF + 2),
            () => new Monster("Lamia", 'L', baseHP + 7, baseATK + 4, baseDEF + 1),
            () => new Monster("Ogre", 'O', baseHP + 12, baseATK + 4, baseDEF + 2),
            () => new Monster("Medusa", 'M', baseHP + 9, baseATK + 5, baseDEF + 2),
            () => new Monster("Treant", 'T', baseHP + 15, baseATK + 3, baseDEF + 3),
            () => new Monster("Chimera", 'C', baseHP + 14, baseATK + 6, baseDEF + 3),
            () => new Monster("Golem", 'G', baseHP + 20, baseATK + 4, baseDEF + 6),
            () => new Monster("Manticore", 'M', baseHP + 16, baseATK + 7, baseDEF + 2),
            () => new Monster("Wendigo", 'W', baseHP + 12, baseATK + 6, baseDEF + 2),
            () => new Monster("Dragon Whelp", 'd', baseHP + 18, baseATK + 8, baseDEF + 3),
            () => new Monster("Behemoth", 'B', baseHP + 24, baseATK + 9, baseDEF + 4),
            () => new Monster("Evil Eye", 'e', baseHP + 15, baseATK + 7, baseDEF + 2),
            () => new Monster("Banshee", 'b', baseHP + 12, baseATK + 8, baseDEF + 1),
            () => new Monster("Malboro", 'M', baseHP + 20, baseATK + 8, baseDEF + 3),
            () => new Monster("Hydra", 'H', baseHP + 25, baseATK + 9, baseDEF + 4),
            () => new Monster("Vampire", 'V', baseHP + 18, baseATK + 10, baseDEF + 2),
            () => new Monster("Lich", 'L', baseHP + 22, baseATK + 9, baseDEF + 3)
        });
            }

            // Pick one randomly
            int choice = Rng.Next(pool.Count);
            return pool[choice]();
        }


        public static Monster CreateBossForFloor(int floor)
        {
            string[] names = { "Tococo", "Amanita", "Bigmouth Billy", "Bubbly Bernie", "Tom Tit Tat", "Leaping Lizzy", "Carnero", "Swamfisk", "Lumbering Lambert", "Golden Bat" };
            string n = names[Rng.Next(names.Length)];
            int hp = 30 + floor * 12;
            int atk = 5 + floor * 2;
            int def = 3 + floor;
            return new Monster(n, 'B', hp, atk, def, true);
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
