using Game.Core.Common;

namespace Game.Tests.EditMode.Common
{
    /// <summary>A fake IRandom that returns a fixed list of numbers in order, so tests can force an exact outcome.</summary>
    public sealed class ScriptedRandom : IRandom
    {
        private readonly int[] _values;
        private int _next;

        public ScriptedRandom(params int[] values)
        {
            _values = values;
        }

        public int Next(int maxExclusive)
        {
            // Wrapping around keeps long loops from running out of numbers.
            var value = _values[_next % _values.Length];
            _next++;
            return value % maxExclusive;
        }
    }
}
