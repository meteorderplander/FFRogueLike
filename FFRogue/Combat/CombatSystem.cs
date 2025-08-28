
using System;
using FFRogue.Entities;

namespace FFRogue.Combat
{
    public static class CombatSystem
    {
        private static readonly Random Rng = new Random();

        public static string Attack(Entity attacker, Entity defender)
        {
            int hitRoll = Rng.Next(100);
            int hitChance = 70 + (attacker.Attack - defender.Defense) * 2;
            hitChance = Math.Clamp(hitChance, 20, 95);
            if (hitRoll >= hitChance) return $"{attacker.Name} misses {defender.Name}.";

            int variance = Rng.Next(-2, 3);
            int dmg = System.Math.Max(1, attacker.Attack + variance - defender.Defense);
            defender.CurrentHP -= dmg;
            string msg = $"{attacker.Name} hits {defender.Name} for {dmg}!";
            if (defender.CurrentHP <= 0) { defender.CurrentHP = 0; msg += $" {defender.Name} is defeated."; }
            return msg;
        }
    }
}
