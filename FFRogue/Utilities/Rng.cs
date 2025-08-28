using System;

namespace FFRogue
{
    public static class Rng
    {
        private static readonly Random _r = new Random();
        public static int Next(int maxExclusive) => _r.Next(maxExclusive);
        public static int Next(int minInclusive, int maxExclusive) => _r.Next(minInclusive, maxExclusive);
    }
}