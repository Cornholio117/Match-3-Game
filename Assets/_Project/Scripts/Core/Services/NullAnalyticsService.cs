namespace Game.Core.Services
{
    /// <summary>Stand-in used in the Editor and in tests until an analytics SDK is added. Records nothing.</summary>
    public sealed class NullAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string name)
        {
            // Intentionally empty: nothing is sent.
        }
    }
}
