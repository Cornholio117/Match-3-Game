namespace Game.Core.Board
{
    /// <summary>What a gem does besides having a colour. A line gem clears its row or column when it is matched.</summary>
    public enum PieceKind
    {
        Normal = 0,
        LineHorizontal = 1,
        LineVertical = 2
    }
}
