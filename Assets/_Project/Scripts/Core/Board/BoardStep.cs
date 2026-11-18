namespace Game.Core.Board
{
    public enum BoardStepKind
    {
        Clear,
        Fall,
        Spawn,
        Create
    }

    /// <summary>One thing that happened to the board during a turn, so a view can play it back as an animation.</summary>
    public readonly struct BoardStep
    {
        public BoardStepKind Kind { get; }
        public PieceColor Color { get; }

        /// <summary>For a Create step, the special kind the gem became; Normal for every other step.</summary>
        public PieceKind SpecialKind { get; }

        /// <summary>The cell involved; for a fall, the cell the gem left.</summary>
        public GridPosition From { get; }

        /// <summary>Where the gem ended up; the same as From for every step except a fall.</summary>
        public GridPosition To { get; }

        private BoardStep(BoardStepKind kind, PieceColor color, PieceKind specialKind, GridPosition from, GridPosition to)
        {
            Kind = kind;
            Color = color;
            SpecialKind = specialKind;
            From = from;
            To = to;
        }

        public static BoardStep Clear(PieceColor color, GridPosition position)
        {
            return new BoardStep(BoardStepKind.Clear, color, PieceKind.Normal, position, position);
        }

        public static BoardStep Fall(PieceColor color, GridPosition from, GridPosition to)
        {
            return new BoardStep(BoardStepKind.Fall, color, PieceKind.Normal, from, to);
        }

        public static BoardStep Spawn(PieceColor color, GridPosition position)
        {
            return new BoardStep(BoardStepKind.Spawn, color, PieceKind.Normal, position, position);
        }

        /// <summary>A normal gem on the board turns into a special gem, staying in its cell.</summary>
        public static BoardStep Create(PieceColor color, PieceKind specialKind, GridPosition position)
        {
            return new BoardStep(BoardStepKind.Create, color, specialKind, position, position);
        }
    }
}
