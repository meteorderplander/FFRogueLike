using System;
using System.Collections.Generic;
using FFRogue.Jobs;
using FFRogue.Entities;

namespace FFRogue.Abilities
{
    public enum AbilityResult
    {
        Success,
        InsufficientMP,
        InvalidTarget,
        AlreadyAtMax,
        OnCooldown,
        Failed
    }

    public class AbilityInfo
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int MPCost { get; set; } = 0;
        public int Cooldown { get; set; } = 0; // Turns
        public char Hotkey { get; set; } = '1';
        public AbilityType Type { get; set; }
        public Func<Player, Game, AbilityResult> Execute { get; set; } = (p, g) => AbilityResult.Failed;
    }

    public enum AbilityType
    {
        Offensive,    // Damages enemies
        Defensive,    // Buffs/heals self
        Utility,      // Map effects, movement, etc
        Healing       // Restores HP/MP
    }

    public static class AbilitySystem
    {
        private static readonly Dictionary<Job, List<AbilityInfo>> JobAbilities = new();
        private static readonly Dictionary<string, int> AbilityCooldowns = new();

        static AbilitySystem()
        {
            InitializeAbilities();
        }

        private static void InitializeAbilities()
        {
            // TANKS
            JobAbilities[Job.PLD] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Cover",
                    Description = "Reduce damage taken for 5 turns",
                    MPCost = 8,
                    Hotkey = '1',
                    Type = AbilityType.Defensive,
                    Execute = (player, game) => {
                        // Temp buff implementation - just heal for now
                        int heal = player.MaxHP / 4;
                        player.CurrentHP = Math.Min(player.CurrentHP + heal, player.MaxHP);
                        game.AddMessage($"{player.Name} uses Cover! Defense increased!");
                        return AbilityResult.Success;
                    }
                },
                new AbilityInfo
                {
                    Name = "Holy Strike",
                    Description = "Powerful holy attack against adjacent enemies",
                    MPCost = 12,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Holy Strike", 1.5f)
                },
                new AbilityInfo
                {
                    Name = "Spirits Within",
                    Description = "Unleash inner power for massive single target damage",
                    MPCost = 15,
                    Cooldown = 8,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Spirits Within", 2.2f)
                },
                new AbilityInfo
                {
                    Name = "Circle of Scorn",
                    Description = "AoE damage over time effect around you",
                    MPCost = 18,
                    Cooldown = 12,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Circle of Scorn", 1.8f)
                }
            };

            JobAbilities[Job.WAR] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Berserk",
                    Description = "Increase attack power but take more damage",
                    MPCost = 6,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => {
                        player.Attack += 3;
                        game.AddMessage($"{player.Name} enters a berserker rage! Attack increased!");
                        return AbilityResult.Success;
                    }
                },
                new AbilityInfo
                {
                    Name = "Cleave",
                    Description = "Attack all adjacent enemies",
                    MPCost = 10,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Cleave", 1.2f)
                },
                new AbilityInfo
                {
                    Name = "Fell Cleave",
                    Description = "Devastating attack that ignores defense",
                    MPCost = 20,
                    Cooldown = 6,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Fell Cleave", 2.5f)
                },
                new AbilityInfo
                {
                    Name = "Upheaval",
                    Description = "Powerful single target strike",
                    MPCost = 12,
                    Cooldown = 10,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Upheaval", 1.9f)
                }
            };

            JobAbilities[Job.DRK] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Unleash",
                    Description = "Dark energy damages all nearby enemies",
                    MPCost = 14,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Unleash", 1.4f)
                },
                new AbilityInfo
                {
                    Name = "Abyssal Drain",
                    Description = "Dark spell that damages and heals you",
                    MPCost = 16,
                    Cooldown = 8,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => {
                        var result = CastRangedSpell(player, game, "Abyssal Drain", 1.6f);
                        if (result == AbilityResult.Success)
                        {
                            int heal = player.MaxHP / 6;
                            player.CurrentHP = Math.Min(player.CurrentHP + heal, player.MaxHP);
                            game.AddMessage($"Abyssal Drain restores {heal} HP!");
                        }
                        return result;
                    }
                },
                new AbilityInfo
                {
                    Name = "Carve and Spit",
                    Description = "Vicious attack that restores MP",
                    MPCost = 10,
                    Cooldown = 12,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => {
                        var result = CastRangedSpell(player, game, "Carve and Spit", 1.7f);
                        if (result == AbilityResult.Success)
                        {
                            int mpRestore = player.MaxMP / 4;
                            player.CurrentMP = Math.Min(player.CurrentMP + mpRestore, player.MaxMP);
                            game.AddMessage($"Carve and Spit restores {mpRestore} MP!");
                        }
                        return result;
                    }
                }
            };

            JobAbilities[Job.GNB] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Keen Edge",
                    Description = "Sharp gunblade strike",
                    MPCost = 8,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Keen Edge", 1.3f)
                },
                new AbilityInfo
                {
                    Name = "Demon Slice",
                    Description = "Wide arc attack hitting multiple enemies",
                    MPCost = 12,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Demon Slice", 1.2f)
                },
                new AbilityInfo
                {
                    Name = "Rough Divide",
                    Description = "Gap-closing attack with high damage",
                    MPCost = 15,
                    Cooldown = 10,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Rough Divide", 2.0f)
                }
            };

            // HEALERS
            JobAbilities[Job.WHM] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Cure",
                    Description = "Restore HP",
                    MPCost = 8,
                    Hotkey = '1',
                    Type = AbilityType.Healing,
                    Execute = (player, game) => {
                        if (player.CurrentHP >= player.MaxHP) return AbilityResult.AlreadyAtMax;
                        int heal = 20 + player.Stats.MND * 2;
                        player.CurrentHP = Math.Min(player.CurrentHP + heal, player.MaxHP);
                        game.AddMessage($"{player.Name} casts Cure! Restored {heal} HP!");
                        return AbilityResult.Success;
                    }
                },
                new AbilityInfo
                {
                    Name = "Holy",
                    Description = "Powerful light-based AoE attack",
                    MPCost = 15,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Holy", 2.0f)
                },
                new AbilityInfo
                {
                    Name = "Stone",
                    Description = "Earth-based ranged attack",
                    MPCost = 6,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Stone", 1.4f)
                },
                new AbilityInfo
                {
                    Name = "Assize",
                    Description = "Damages enemies while restoring your MP",
                    MPCost = 12,
                    Cooldown = 15,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => {
                        var result = AttackAdjacentEnemies(player, game, "Assize", 1.6f);
                        int mpRestore = player.MaxMP / 3;
                        player.CurrentMP = Math.Min(player.CurrentMP + mpRestore, player.MaxMP);
                        game.AddMessage($"Assize restores {mpRestore} MP!");
                        return result;
                    }
                }
            };

            JobAbilities[Job.SCH] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Bio",
                    Description = "Poison spell that damages over time",
                    MPCost = 8,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Bio", 1.2f)
                },
                new AbilityInfo
                {
                    Name = "Ruin",
                    Description = "Basic attack spell",
                    MPCost = 6,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Ruin", 1.1f)
                },
                new AbilityInfo
                {
                    Name = "Energy Drain",
                    Description = "Drains enemy energy and restores your MP",
                    MPCost = 5,
                    Cooldown = 8,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => {
                        var result = CastRangedSpell(player, game, "Energy Drain", 1.3f);
                        if (result == AbilityResult.Success)
                        {
                            int mpRestore = player.MaxMP / 4;
                            player.CurrentMP = Math.Min(player.CurrentMP + mpRestore, player.MaxMP);
                            game.AddMessage($"Energy Drain restores {mpRestore} MP!");
                        }
                        return result;
                    }
                }
            };

            JobAbilities[Job.AST] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Malefic",
                    Description = "Dark star magic damages target",
                    MPCost = 7,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Malefic", 1.3f)
                },
                new AbilityInfo
                {
                    Name = "Gravity",
                    Description = "Gravitational force damages all nearby enemies",
                    MPCost = 14,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Gravity", 1.4f)
                },
                new AbilityInfo
                {
                    Name = "Combust",
                    Description = "Burning star effect with damage over time",
                    MPCost = 9,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Combust", 1.5f)
                }
            };

            JobAbilities[Job.SGE] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Dosis",
                    Description = "Inject harmful toxins into target",
                    MPCost = 6,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Dosis", 1.2f)
                },
                new AbilityInfo
                {
                    Name = "Dyskrasia",
                    Description = "Area toxin attack affecting nearby enemies",
                    MPCost = 12,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Dyskrasia", 1.3f)
                },
                new AbilityInfo
                {
                    Name = "Toxikon",
                    Description = "Concentrated poison attack",
                    MPCost = 10,
                    Cooldown = 6,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Toxikon", 1.8f)
                }
            };

            // MELEE DPS
            JobAbilities[Job.MNK] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Focus",
                    Description = "Increase accuracy and critical hit chance",
                    MPCost = 5,
                    Hotkey = '1',
                    Type = AbilityType.Utility,
                    Execute = (player, game) => {
                        player.Attack += 2;
                        game.AddMessage($"{player.Name} focuses their chi! Accuracy increased!");
                        return AbilityResult.Success;
                    }
                },
                new AbilityInfo
                {
                    Name = "Chakra",
                    Description = "Restore MP and some HP",
                    MPCost = 0,
                    Cooldown = 10,
                    Hotkey = '2',
                    Type = AbilityType.Healing,
                    Execute = (player, game) => {
                        int mpRestore = player.MaxMP / 3;
                        int hpRestore = player.MaxHP / 6;
                        player.CurrentMP = Math.Min(player.CurrentMP + mpRestore, player.MaxMP);
                        player.CurrentHP = Math.Min(player.CurrentHP + hpRestore, player.MaxHP);
                        game.AddMessage($"{player.Name} channels chakra! MP and HP restored!");
                        return AbilityResult.Success;
                    }
                },
                new AbilityInfo
                {
                    Name = "Bootshine",
                    Description = "Critical strike combo opener",
                    MPCost = 8,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Bootshine", 1.8f)
                },
                new AbilityInfo
                {
                    Name = "Howling Fist",
                    Description = "Ranged wind-based attack",
                    MPCost = 12,
                    Cooldown = 8,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Howling Fist", 2.1f)
                }
            };

            JobAbilities[Job.DRG] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "True Thrust",
                    Description = "Precise spear attack",
                    MPCost = 7,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "True Thrust", 1.4f)
                },
                new AbilityInfo
                {
                    Name = "Jump",
                    Description = "Leap attack from above",
                    MPCost = 12,
                    Cooldown = 6,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Jump", 2.0f)
                },
                new AbilityInfo
                {
                    Name = "Doom Spike",
                    Description = "Line AoE spear attack",
                    MPCost = 15,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Doom Spike", 1.3f)
                },
                new AbilityInfo
                {
                    Name = "Spineshatter Dive",
                    Description = "Devastating diving attack",
                    MPCost = 18,
                    Cooldown = 12,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Spineshatter Dive", 2.4f)
                }
            };

            JobAbilities[Job.NIN] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Spinning Edge",
                    Description = "Swift blade attack",
                    MPCost = 6,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Spinning Edge", 1.2f)
                },
                new AbilityInfo
                {
                    Name = "Throwing Dagger",
                    Description = "Ranged throwing weapon",
                    MPCost = 8,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Throwing Dagger", 1.1f)
                },
                new AbilityInfo
                {
                    Name = "Assassinate",
                    Description = "High damage stealth attack",
                    MPCost = 15,
                    Cooldown = 10,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Assassinate", 2.3f)
                },
                new AbilityInfo
                {
                    Name = "Death Blossom",
                    Description = "Spinning attack hitting all nearby enemies",
                    MPCost = 14,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Death Blossom", 1.4f)
                }
            };

            JobAbilities[Job.SAM] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Hakaze",
                    Description = "Basic katana strike",
                    MPCost = 6,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Hakaze", 1.3f)
                },
                new AbilityInfo
                {
                    Name = "Iaijutsu",
                    Description = "Quick-draw technique",
                    MPCost = 12,
                    Cooldown = 8,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Iaijutsu", 2.1f)
                },
                new AbilityInfo
                {
                    Name = "Fuga",
                    Description = "Wide sweeping attack",
                    MPCost = 10,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Fuga", 1.2f)
                },
                new AbilityInfo
                {
                    Name = "Hissatsu: Shinten",
                    Description = "Secret sword technique",
                    MPCost = 16,
                    Cooldown = 6,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Shinten", 2.2f)
                }
            };

            JobAbilities[Job.RPR] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Slice",
                    Description = "Basic scythe attack",
                    MPCost = 7,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Slice", 1.3f)
                },
                new AbilityInfo
                {
                    Name = "Spinning Scythe",
                    Description = "Rotating scythe attack hitting multiple enemies",
                    MPCost = 12,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Spinning Scythe", 1.3f)
                },
                new AbilityInfo
                {
                    Name = "Shadow of Death",
                    Description = "Death magic that damages over time",
                    MPCost = 10,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Shadow of Death", 1.6f)
                },
                new AbilityInfo
                {
                    Name = "Soul Slice",
                    Description = "Soul-draining attack that restores MP",
                    MPCost = 8,
                    Cooldown = 10,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => {
                        var result = CastRangedSpell(player, game, "Soul Slice", 1.7f);
                        if (result == AbilityResult.Success)
                        {
                            int mpRestore = player.MaxMP / 5;
                            player.CurrentMP = Math.Min(player.CurrentMP + mpRestore, player.MaxMP);
                            game.AddMessage($"Soul Slice restores {mpRestore} MP!");
                        }
                        return result;
                    }
                }
            };

            // MAGIC DPS
            JobAbilities[Job.BLM] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Fire",
                    Description = "Ranged fire spell",
                    MPCost = 6,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Fire", 1.3f)
                },
                new AbilityInfo
                {
                    Name = "Thunder",
                    Description = "Lightning spell hitting multiple enemies",
                    MPCost = 12,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Thunder", 1.8f)
                },
                new AbilityInfo
                {
                    Name = "Blizzard",
                    Description = "Ice magic with slowing effect",
                    MPCost = 8,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Blizzard", 1.4f)
                },
                new AbilityInfo
                {
                    Name = "Flare",
                    Description = "Massive AoE explosion",
                    MPCost = 25,
                    Cooldown = 12,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Flare", 2.5f)
                }
            };

            JobAbilities[Job.SMN] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Ruin",
                    Description = "Basic summon magic",
                    MPCost = 6,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Ruin", 1.2f)
                },
                new AbilityInfo
                {
                    Name = "Outburst",
                    Description = "AoE summon attack",
                    MPCost = 12,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Outburst", 1.4f)
                },
                new AbilityInfo
                {
                    Name = "Fester",
                    Description = "Powerful single target summon spell",
                    MPCost = 15,
                    Cooldown = 8,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Fester", 2.0f)
                },
                new AbilityInfo
                {
                    Name = "Deathflare",
                    Description = "Ultimate summon attack",
                    MPCost = 22,
                    Cooldown = 15,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Deathflare", 2.8f)
                }
            };

            JobAbilities[Job.RDM] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Jolt",
                    Description = "Basic magic attack",
                    MPCost = 5,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Jolt", 1.1f)
                },
                new AbilityInfo
                {
                    Name = "Verthunder",
                    Description = "Black magic lightning spell",
                    MPCost = 8,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Verthunder", 1.5f)
                },
                new AbilityInfo
                {
                    Name = "Veraero",
                    Description = "White magic wind spell",
                    MPCost = 8,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Veraero", 1.5f)
                },
                new AbilityInfo
                {
                    Name = "Scatter",
                    Description = "AoE spell affecting nearby enemies",
                    MPCost = 14,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Scatter", 1.3f)
                }
            };

            JobAbilities[Job.BLU] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Water Cannon",
                    Description = "Learned water spell from monsters",
                    MPCost = 8,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Water Cannon", 1.3f)
                },
                new AbilityInfo
                {
                    Name = "Sonic Boom",
                    Description = "Sound-based AoE attack",
                    MPCost = 12,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Sonic Boom", 1.4f)
                },
                new AbilityInfo
                {
                    Name = "Bad Breath",
                    Description = "Debilitating breath attack",
                    MPCost = 15,
                    Cooldown = 10,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Bad Breath", 1.6f)
                },
                new AbilityInfo
                {
                    Name = "1000 Needles",
                    Description = "Fixed damage needle attack",
                    MPCost = 18,
                    Cooldown = 12,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "1000 Needles", 2.5f)
                }
            };

            // PHYSICAL RANGED DPS
            JobAbilities[Job.BRD] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Heavy Shot",
                    Description = "Powerful bow attack",
                    MPCost = 6,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Heavy Shot", 1.3f)
                },
                new AbilityInfo
                {
                    Name = "Quick Nock",
                    Description = "Wide shot hitting multiple targets",
                    MPCost = 10,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Quick Nock", 1.1f)
                },
                new AbilityInfo
                {
                    Name = "Bloodletter",
                    Description = "Critical shot with bleeding effect",
                    MPCost = 12,
                    Cooldown = 6,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Bloodletter", 1.8f)
                },
                new AbilityInfo
                {
                    Name = "Rain of Death",
                    Description = "Arrow rain affecting large area",
                    MPCost = 16,
                    Cooldown = 10,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Rain of Death", 1.7f)
                }
            };

            JobAbilities[Job.MCH] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Split Shot",
                    Description = "Basic firearm attack",
                    MPCost = 6,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Split Shot", 1.2f)
                },
                new AbilityInfo
                {
                    Name = "Spread Shot",
                    Description = "Shotgun blast hitting multiple enemies",
                    MPCost = 10,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Spread Shot", 1.2f)
                },
                new AbilityInfo
                {
                    Name = "Hot Shot",
                    Description = "Burning ammunition with damage over time",
                    MPCost = 12,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Hot Shot", 1.6f)
                },
                new AbilityInfo
                {
                    Name = "Wildfire",
                    Description = "Explosive delayed damage",
                    MPCost = 20,
                    Cooldown = 15,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Wildfire", 2.3f)
                }
            };

            JobAbilities[Job.DNC] = new List<AbilityInfo>
            {
                new AbilityInfo
                {
                    Name = "Cascade",
                    Description = "Graceful throwing weapon attack",
                    MPCost = 6,
                    Hotkey = '1',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Cascade", 1.1f)
                },
                new AbilityInfo
                {
                    Name = "Windmill",
                    Description = "Spinning attack hitting nearby enemies",
                    MPCost = 10,
                    Hotkey = '2',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Windmill", 1.2f)
                },
                new AbilityInfo
                {
                    Name = "Rising Windmill",
                    Description = "Enhanced spinning attack",
                    MPCost = 14,
                    Hotkey = '3',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => AttackAdjacentEnemies(player, game, "Rising Windmill", 1.5f)
                },
                new AbilityInfo
                {
                    Name = "Starfall Dance",
                    Description = "Elegant finishing move",
                    MPCost = 18,
                    Cooldown = 12,
                    Hotkey = '4',
                    Type = AbilityType.Offensive,
                    Execute = (player, game) => CastRangedSpell(player, game, "Starfall Dance", 2.2f)
                }
            };

            // Add basic abilities for any remaining jobs
            foreach (Job job in Enum.GetValues<Job>())
            {
                if (!JobAbilities.ContainsKey(job))
                {
                    JobAbilities[job] = new List<AbilityInfo>
                    {
                        new AbilityInfo
                        {
                            Name = "Focus",
                            Description = "Restore some MP",
                            MPCost = 0,
                            Cooldown = 8,
                            Hotkey = '1',
                            Type = AbilityType.Utility,
                            Execute = (player, game) => {
                                int restore = Math.Max(1, player.MaxMP / 5);
                                player.CurrentMP = Math.Min(player.CurrentMP + restore, player.MaxMP);
                                game.AddMessage($"{player.Name} focuses! MP restored!");
                                return AbilityResult.Success;
                            }
                        },
                        new AbilityInfo
                        {
                            Name = "Strike",
                            Description = "Basic attack ability",
                            MPCost = 5,
                            Hotkey = '2',
                            Type = AbilityType.Offensive,
                            Execute = (player, game) => CastRangedSpell(player, game, "Strike", 1.2f)
                        }
                    };
                }
            }
        }

        public static List<AbilityInfo> GetAbilitiesForJob(Job job)
        {
            return JobAbilities.ContainsKey(job) ? JobAbilities[job] : new List<AbilityInfo>();
        }

        public static AbilityResult UseAbility(Player player, Game game, AbilityInfo ability)
        {
            // Check cooldown
            string cooldownKey = $"{player.Name}_{ability.Name}";
            if (AbilityCooldowns.ContainsKey(cooldownKey) && AbilityCooldowns[cooldownKey] > 0)
            {
                return AbilityResult.OnCooldown;
            }

            // Check MP
            if (player.CurrentMP < ability.MPCost)
            {
                return AbilityResult.InsufficientMP;
            }

            // Use MP
            player.CurrentMP -= ability.MPCost;

            // Set cooldown
            if (ability.Cooldown > 0)
            {
                AbilityCooldowns[cooldownKey] = ability.Cooldown;
            }

            // Execute ability
            return ability.Execute(player, game);
        }

        public static void UpdateCooldowns()
        {
            var keys = new List<string>(AbilityCooldowns.Keys);
            foreach (var key in keys)
            {
                if (AbilityCooldowns[key] > 0)
                {
                    AbilityCooldowns[key]--;
                    if (AbilityCooldowns[key] <= 0)
                    {
                        AbilityCooldowns.Remove(key);
                    }
                }
            }
        }

        public static int GetCooldownRemaining(Player player, AbilityInfo ability)
        {
            string cooldownKey = $"{player.Name}_{ability.Name}";
            return AbilityCooldowns.ContainsKey(cooldownKey) ? AbilityCooldowns[cooldownKey] : 0;
        }

        // Helper methods for common ability effects
        private static AbilityResult AttackAdjacentEnemies(Player player, Game game, string abilityName, float damageMultiplier)
        {
            int hitCount = 0;
            var directions = new[] { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1) };

            foreach (var (dx, dy) in directions)
            {
                int targetX = player.X + dx;
                int targetY = player.Y + dy;

                var monster = game.Monsters.Find(m => m.IsAlive && m.X == targetX && m.Y == targetY);
                if (monster != null)
                {
                    int damage = (int)(player.Attack * damageMultiplier);
                    monster.CurrentHP -= damage;
                    game.AddMessage($"{abilityName} hits {monster.Name} for {damage}!");

                    if (monster.CurrentHP <= 0)
                    {
                        monster.CurrentHP = 0;
                        int xpGain = monster.IsBoss ? 100 + 10 * game.Floor : 10 + 2 * game.Floor;
                        player.GainXP(xpGain);
                        game.AddMessage($"{monster.Name} is defeated! {player.Name} gains {xpGain} XP!");
                    }
                    hitCount++;
                }
            }

            if (hitCount == 0)
            {
                game.AddMessage($"{player.Name} uses {abilityName}, but there are no enemies nearby!");
                return AbilityResult.InvalidTarget;
            }

            return AbilityResult.Success;
        }

        private static AbilityResult CastRangedSpell(Player player, Game game, string spellName, float damageMultiplier)
        {
            // Find nearest visible enemy within range
            Monster target = null;
            int minDistance = int.MaxValue;
            int spellRange = 5;

            foreach (var monster in game.Monsters)
            {
                if (!monster.IsAlive) continue;

                int distance = Math.Abs(monster.X - player.X) + Math.Abs(monster.Y - player.Y);
                if (distance <= spellRange && distance < minDistance)
                {
                    target = monster;
                    minDistance = distance;
                }
            }

            if (target == null)
            {
                game.AddMessage($"No enemies in range for {spellName}!");
                return AbilityResult.InvalidTarget;
            }

            int damage = (int)((player.Attack + player.Stats.INT) * damageMultiplier);
            target.CurrentHP -= damage;
            game.AddMessage($"{spellName} hits {target.Name} for {damage}!");

            if (target.CurrentHP <= 0)
            {
                target.CurrentHP = 0;
                int xpGain = target.IsBoss ? 100 + 10 * game.Floor : 10 + 2 * game.Floor;
                player.GainXP(xpGain);
                game.AddMessage($"{target.Name} is defeated! {player.Name} gains {xpGain} XP!");
            }

            return AbilityResult.Success;
        }
    }
}