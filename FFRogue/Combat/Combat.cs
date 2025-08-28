using System;
using FFRogue.Entities;

namespace FFRogue.Combat
{
    public static class Combat
    {
        public static void Attack(Entity attacker, Entity defender, out string result)
        {
            // super simple: hit chance + damage = atk +/- rng - def
            int hitRoll = Rng.Next(100);
            int hitChance = 70 + (attacker.Attack - defender.Defense) * 2;
            hitChance = Math.Clamp(hitChance, 20, 95);

            if (hitRoll >= hitChance)
            {
                result = $"{attacker.Name} misses {defender.Name}.";
                return;
            }

            int variance = Rng.Next(-2, 3);
            int dmg = Math.Max(1, attacker.Attack + variance - defender.Defense);
            defender.CurrentHP -= dmg;

            result = $"{attacker.Name} hits {defender.Name} for {dmg}!";
            if (defender.CurrentHP <= 0)
            {
                defender.CurrentHP = 0;
                result += $" {defender.Name} is defeated.";
            }
        }
    }
}
