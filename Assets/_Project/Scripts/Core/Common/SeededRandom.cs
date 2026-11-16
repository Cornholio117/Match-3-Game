using System;

namespace Game.Core.Common
{
    /// <summary>Random numbers from a seed: the same seed always gives the same sequence, so a level can be replayed.</summary>
    public sealed class SeededRandom : IRandom
    {
        private readonly Random _random;

        public SeededRandom(int seed)
        {
            _random = new Random(seed);
        }

        public int Next(int maxExclusive)
        {
            return _random.Next(maxExclusive);
        }
    }
}
