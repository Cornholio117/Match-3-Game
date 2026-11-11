namespace Game.Core.Services
{
    /// <summary>Stand-in used in the Editor and in tests until the real save service exists. Saves nothing.</summary>
    public sealed class NullSaveService : ISaveService
    {
        public void Save(string key, string json)
        {
            // Intentionally empty: nothing is stored.
        }

        public bool TryLoad(string key, out string json)
        {
            json = null;
            return false;
        }
    }
}
