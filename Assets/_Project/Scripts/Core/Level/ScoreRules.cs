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

        private readonly int _pointsPerGem;
        private readonly int _specialBonus;

        public ScoreRules(int pointsPerGem = DefaultPointsPerGem, int specialBonus = DefaultSpecialBonus)
        {
            if (pointsPerGem <= 0)
                throw new ArgumentOutOfRangeException(nameof(pointsPerGem));

            if (specialBonus < 0)
                throw new ArgumentOutOfRangeException(nameof(specialBonus));

            _pointsPerGem = pointsPerGem;
            _specialBonus = specialBonus;
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

                    total += _pointsPerGem * wave;
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
