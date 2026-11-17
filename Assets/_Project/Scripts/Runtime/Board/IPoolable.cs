namespace Game.Runtime.Board
{
    /// <summary>An object that is reused from a pool and must reset its own state when it comes and goes.</summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
