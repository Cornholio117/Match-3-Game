using System;
using System.Collections.Generic;
using Game.Core.Board;

namespace Game.Core.Level
{
    /// <summary>
    /// Turns what happened in one turn (the recorded board steps) into points: each cleared gem is worth a fixed
    /// amount times its cascade wave (1 for the first wave, 2 for the next, and so on), and each special gem made
    /// adds a bonus.
    /// </summary>
    public sealed class ScoreRules
    {
        public const int DefaultPointsPerGem = 10;
        public const int DefaultSpecialBonus = 50;

        /// <summary>Chosen by measuring virtual players, not by guessing: uncapped, a lucky cascade could win level 1 in two turns.</summary>
        public const int DefaultMaxWaveMultiplier = 2;

        private readonly int _pointsPerGem;
        private readonly int _specialBonus;
        private readonly int _maxWaveMultiplier;

        /// <summary>maxWaveMultiplier limits how much a long cascade can multiply the points (1 means no multiplier at all).</summary>
        public ScoreRules(
            int pointsPerGem = DefaultPointsPerGem, int specialBonus = DefaultSpecialBonus, int maxWaveMultiplier = DefaultMaxWaveMultiplier)
        {
            if (pointsPerGem <= 0)
                throw new ArgumentOutOfRangeException(nameof(pointsPerGem));

            if (specialBonus < 0)
                throw new ArgumentOutOfRangeException(nameof(specialBonus));

            if (maxWaveMultiplier < 1)
                throw new ArgumentOutOfRangeException(nameof(maxWaveMultiplier));

            _pointsPerGem = pointsPerGem;
            _specialBonus = specialBonus;
            _maxWaveMultiplier = maxWaveMultiplier;
        }

        /// <summary>
        /// A new wave starts at a Clear step that comes right after a Spawn step: the resolver always records a wave
        /// as Clear, Create, Fall, Spawn. Plain loops keep this free of allocations.
        /// </summary>
        public int Calculate(List<BoardStep> steps)
        {
            var total = 0;
            var wave = 1;
            var previousWasSpawn = false;

            for (var i = 0; i < steps.Count; i++)
            {
                var kind = steps[i].Kind;

                if (kind == BoardStepKind.Clear)
                {
                    if (previousWasSpawn)
                        wave++;

                    total += _pointsPerGem * Math.Min(wave, _maxWaveMultiplier);
                }
                else if (kind == BoardStepKind.Create)
                {
                    total += _specialBonus;
                }

                previousWasSpawn = kind == BoardStepKind.Spawn;
            }

            return total;
        }
    }
}
