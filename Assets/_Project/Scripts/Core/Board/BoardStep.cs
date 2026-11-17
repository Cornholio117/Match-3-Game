namespace Game.Core.Board
{
    public enum BoardStepKind
    {
        Clear,
        Fall,
        Spawn
    }

    /// <summary>One thing that happened to the board during a turn, so a view can play it back as an animation.</summary>
    public readonly struct BoardStep
    {
        public BoardStepKind Kind { get; }
        public PieceColor Color { get; }

        /// <summary>The cell involved; for a fall, the cell the gem left.</summary>
        public GridPosition From { get; }

        /// <summary>Where the gem ended up; the same as From for a clear or a spawn.</summary>
        public GridPosition To { get; }

        private BoardStep(BoardStepKind kind, PieceColor color, GridPosition from, GridPosition to)
        {
            Kind = kind;
            Color = color;
            From = from;
            To = to;
        }

        public static BoardStep Clear(PieceColor color, GridPosition position)
        {
            return new BoardStep(BoardStepKind.Clear, color, position, position);
        }

        public static BoardStep Fall(PieceColor color, GridPosition from, GridPosition to)
        {
            return new BoardStep(BoardStepKind.Fall, color, from, to);
        }

        public static BoardStep Spawn(PieceColor color, GridPosition position)
        {
            return new BoardStep(BoardStepKind.Spawn, color, position, position);
        }
    }
}
