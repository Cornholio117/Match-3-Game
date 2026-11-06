namespace Game.Core.Flow
{
    /// <summary>One state of a state machine, such as Menu or Level.</summary>
    public interface IState
    {
        void Enter();
        void Exit();
    }
}
