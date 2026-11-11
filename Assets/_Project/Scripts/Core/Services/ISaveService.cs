namespace Game.Core.Services
{
    /// <summary>Stores and loads saved game data as text.</summary>
    public interface ISaveService
    {
        void Save(string key, string json);

        /// <summary>Returns false (and a null json) when nothing is saved under the key.</summary>
        bool TryLoad(string key, out string json);
    }
}
