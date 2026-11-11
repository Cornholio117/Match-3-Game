namespace Game.Core.Services
{
    /// <summary>Records game events (level started, level won) for analytics.</summary>
    public interface IAnalyticsService
    {
        void LogEvent(string name);
    }
}
