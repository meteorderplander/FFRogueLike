using FFRogue.Jobs;

namespace FFRogue.Entities
{
    public class Player : Entity
    {
        public Job Job { get; private set; }
        public Stats Stats { get; private set; } = new Stats();
        public int Level { get; private set; } = 1;
        public int XP { get; private set; } = 0;

        public Player(string name, Job job)
        {
            Name = name; Job = job; Glyph = '@';
        }

        public void InitializeStats(Job job)
        {
            var info = JobInfo.Get(job);
            Stats.STR = info.BaseStats.STR;
            Stats.DEX = info.BaseStats.DEX;
            Stats.INT = info.BaseStats.INT;
            Stats.MND = info.BaseStats.MND;
            Stats.VIT = info.BaseStats.VIT;

            MaxHP = info.BaseHP + Stats.VIT;
            CurrentHP = MaxHP;
            MaxMP = info.BaseMP + Stats.MND / 2;
            CurrentMP = MaxMP;
            Attack = info.BaseAttack(Stats);
            Defense = info.BaseDefense(Stats);
        }

        public void RecoverHP()
        {
            if (!IsAlive) return;
            int amount = System.Math.Max(1, MaxHP / 10);
            CurrentHP = System.Math.Min(CurrentHP + amount, MaxHP);
        }

        public void RecoverMP()
        {
            if (!IsAlive || MaxMP <= 0) return;
            int amount = System.Math.Max(1, MaxMP / 10);
            CurrentMP = System.Math.Min(CurrentMP + amount, MaxMP);
        }

        public void GainXP(int amount)
        {
            XP += amount;
            while (XP >= Level * 100)
            {
                XP -= Level * 100;
                Level++;
                MaxHP += 5; CurrentHP = MaxHP;
                Attack += 1; Defense += 1;
            }
        }
    }
}