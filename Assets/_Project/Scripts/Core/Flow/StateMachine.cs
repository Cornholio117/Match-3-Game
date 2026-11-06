namespace Game.Core.Flow
{
    /// <summary>Holds exactly one current state and handles leaving the old one and entering the new one.</summary>
    public sealed class StateMachine
    {
        public IState Current { get; private set; }

        public void ChangeState(IState next)
        {
            if (next == Current) return;

            // Current is null on the very first call, so there is nothing to exit yet.
            Current?.Exit();
            Current = next;
            Current?.Enter();
        }
    }
}
