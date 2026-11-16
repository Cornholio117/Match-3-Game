namespace Game.Core.Common
{
    /// <summary>Source of random numbers. Game logic takes this instead of creating its own, so tests can control it.</summary>
    public interface IRandom
    {
        /// <summary>Returns a number from 0 up to, but not including, maxExclusive.</summary>
        int Next(int maxExclusive);
    }
}
