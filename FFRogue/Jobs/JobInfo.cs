using System.Collections.Generic;

namespace FFRogue.Jobs
{
    public class JobInfo
    {
        public Job Job { get; init; }
        public string DisplayName { get; init; }
        public string Role { get; init; }
        public Stats BaseStats { get; init; } = new Stats();
        public int BaseHP { get; init; }
        public int BaseMP { get; init; }

        private static readonly Dictionary<Job, JobInfo> _byJob = new();

        static JobInfo()
        {
            void Add(Job j, string name, string role, Stats stats, int hp, int mp)
                => _byJob[j] = new JobInfo { Job = j, DisplayName = name, Role = role, BaseStats = stats, BaseHP = hp, BaseMP = mp };

            Add(Job.PLD, "Paladin", "Tank", new Stats { STR = 12, DEX = 8, INT = 6, MND = 8, VIT = 14 }, 40, 20);
            Add(Job.WAR, "Warrior", "Tank", new Stats { STR = 14, DEX = 7, INT = 5, MND = 7, VIT = 16 }, 45, 10);
            Add(Job.DRK, "Dark Knight", "Tank", new Stats { STR = 13, DEX = 7, INT = 8, MND = 6, VIT = 15 }, 42, 20);
            Add(Job.GNB, "Gunbreaker", "Tank", new Stats { STR = 13, DEX = 8, INT = 5, MND = 7, VIT = 15 }, 43, 10);

            Add(Job.WHM, "White Mage", "Healer", new Stats { STR = 6, DEX = 7, INT = 12, MND = 14, VIT = 9 }, 28, 40);
            Add(Job.SCH, "Scholar", "Healer", new Stats { STR = 6, DEX = 7, INT = 13, MND = 12, VIT = 10 }, 30, 38);
            Add(Job.AST, "Astrologian", "Healer", new Stats { STR = 6, DEX = 7, INT = 12, MND = 13, VIT = 10 }, 29, 40);
            Add(Job.SGE, "Sage", "Healer", new Stats { STR = 6, DEX = 8, INT = 13, MND = 12, VIT = 10 }, 30, 38);

            Add(Job.MNK, "Monk", "DPS", new Stats { STR = 15, DEX = 10, INT = 5, MND = 5, VIT = 11 }, 34, 8);
            Add(Job.DRG, "Dragoon", "DPS", new Stats { STR = 15, DEX = 9, INT = 5, MND = 5, VIT = 11 }, 34, 8);
            Add(Job.NIN, "Ninja", "DPS", new Stats { STR = 12, DEX = 14, INT = 6, MND = 5, VIT = 10 }, 32, 10);
            Add(Job.SAM, "Samurai", "DPS", new Stats { STR = 16, DEX = 10, INT = 5, MND = 5, VIT = 11 }, 35, 6);
            Add(Job.RPR, "Reaper", "DPS", new Stats { STR = 15, DEX = 9, INT = 7, MND = 5, VIT = 11 }, 34, 10);
            Add(Job.VPR, "Viper", "DPS", new Stats { STR = 15, DEX = 12, INT = 5, MND = 5, VIT = 10 }, 33, 8);

            Add(Job.BRD, "Bard", "DPS", new Stats { STR = 8, DEX = 15, INT = 7, MND = 8, VIT = 10 }, 31, 14);
            Add(Job.MCH, "Machinist", "DPS", new Stats { STR = 9, DEX = 16, INT = 6, MND = 6, VIT = 11 }, 32, 8);
            Add(Job.DNC, "Dancer", "DPS", new Stats { STR = 8, DEX = 15, INT = 8, MND = 10, VIT = 10 }, 31, 14);

            Add(Job.BLM, "Black Mage", "DPS", new Stats { STR = 5, DEX = 7, INT = 16, MND = 6, VIT = 9 }, 28, 42);
            Add(Job.SMN, "Summoner", "DPS", new Stats { STR = 5, DEX = 7, INT = 15, MND = 8, VIT = 9 }, 29, 40);
            Add(Job.RDM, "Red Mage", "DPS", new Stats { STR = 8, DEX = 9, INT = 14, MND = 10, VIT = 10 }, 31, 36);
            Add(Job.PCT, "Pictomancer", "DPS", new Stats { STR = 5, DEX = 8, INT = 15, MND = 9, VIT = 9 }, 29, 40);
            Add(Job.BLU, "Blue Mage", "DPS", new Stats { STR = 8, DEX = 8, INT = 14, MND = 10, VIT = 10 }, 31, 36);
        }

        // Add these missing methods
        public int BaseAttack(Stats stats)
        {
            // Calculate base attack based on job type and stats
            return Role switch
            {
                "Tank" => 5 + stats.STR / 2,
                "Healer" => 3 + stats.INT / 3,
                "DPS" => DisplayName.Contains("Black") || DisplayName.Contains("Summoner") || DisplayName.Contains("Red") || DisplayName.Contains("Pictomancer")
                    ? 4 + stats.INT / 2  // Magic DPS
                    : 6 + stats.STR / 2, // Physical DPS
                _ => 4 + stats.STR / 3
            };
        }

        public int BaseDefense(Stats stats)
        {
            // Calculate base defense based on job type and stats
            return Role switch
            {
                "Tank" => 4 + stats.VIT / 2,
                "Healer" => 2 + stats.VIT / 3,
                "DPS" => 3 + stats.VIT / 3,
                _ => 2 + stats.VIT / 4
            };
        }

        public static JobInfo Get(Job job) => _byJob[job];
        public static IEnumerable<JobInfo> All() { foreach (var kv in _byJob) yield return kv.Value; }
    }
}