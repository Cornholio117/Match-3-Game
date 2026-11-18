using System;
using System.Collections.Generic;
using Game.Core.Common;

namespace Game.Core.Board
{
    /// <summary>What a resolved turn did: waves, gems cleared, whether it was cut short, and special gems made and fired.</summary>
    public readonly struct TurnResult
    {
        public int Cascades { get; }
        public int CellsCleared { get; }
        public bool HitLimit { get; }
        public int SpecialsCreated { get; }
        public int SpecialsFired { get; }

        public TurnResult(int cascades, int cellsCleared, bool hitLimit, int specialsCreated = 0, int specialsFired = 0)
        {
            Cascades = cascades;
            CellsCleared = cellsCleared;
            HitLimit = hitLimit;
            SpecialsCreated = specialsCreated;
            SpecialsFired = specialsFired;
        }
    }

    /// <summary>Runs the clear, fall, refill loop until the board has no runs left, or the cascade limit is reached.</summary>
    public sealed class TurnResolver
    {
        public const int DefaultMaxCascades = 50;

        private readonly struct Creation
        {
            public GridPosition Cell { get; }
            public PieceColor Color { get; }
            public PieceKind Kind { get; }

            public Creation(GridPosition cell, PieceColor color, PieceKind kind)
            {
                Cell = cell;
                Color = color;
                Kind = kind;
            }
        }

        private readonly int _maxCascades;
        private readonly List<Match> _matches = new List<Match>();
        private readonly HashSet<GridPosition> _cells = new HashSet<GridPosition>();
        private readonly HashSet<GridPosition> _protected = new HashSet<GridPosition>();
        private readonly HashSet<GridPosition> _fired = new HashSet<GridPosition>();
        private readonly List<GridPosition> _queue = new List<GridPosition>();
        private readonly List<Creation> _creations = new List<Creation>();

        public TurnResolver(int maxCascades = DefaultMaxCascades)
        {
            if (maxCascades < 1)
                throw new ArgumentOutOfRangeException(nameof(maxCascades));

            _maxCascades = maxCascades;
        }

        /// <summary>
        /// Resolves everything on the board, appending what happened to steps. swapFirst and swapSecond are the cells
        /// the player just swapped: in the first wave a new special gem appears on one of them. The limit stops a
        /// pathological refill from looping forever.
        /// </summary>
        public TurnResult Resolve(
            BoardModel board,
            IRandom random,
            int colorCount,
            List<BoardStep> steps,
            GridPosition? swapFirst = null,
            GridPosition? swapSecond = null)
        {
            var cascades = 0;
            var cleared = 0;
            var created = 0;
            var fired = 0;

            while (true)
            {
                MatchFinder.Find(board, _matches);
                if (_matches.Count == 0)
                    return new TurnResult(cascades, cleared, false, created, fired);

                if (cascades >= _maxCascades)
                    return new TurnResult(cascades, cleared, true, created, fired);

                // Only the first wave has a player move; later waves are cascades and use a fixed fallback cell.
                var first = cascades == 0 ? swapFirst : null;
                var second = cascades == 0 ? swapSecond : null;

                PlanCreations(board, first, second);
                CollectMatchedCells();
                fired += FireSpecials(board);
                cleared += ClearCells(board, steps);
                created += ApplyCreations(board, steps);

                BoardGravity.Apply(board, steps);
                BoardRefill.Apply(board, random, colorCount, steps);
                cascades++;
            }
        }

        // A run of exactly 4 makes a line gem: a horizontal run clears its row later, a vertical run its column.
        // It appears where the player moved if that cell is in the run, otherwise in the run's lower-left cell.
        private void PlanCreations(BoardModel board, GridPosition? swapFirst, GridPosition? swapSecond)
        {
            _creations.Clear();
            _protected.Clear();

            foreach (var match in _matches)
            {
                if (match.Length != 4)
                    continue;

                if (!TryChooseCreationCell(board, match, swapFirst, swapSecond, out var cell))
                    continue;

                var kind = match.Direction == Direction.Right ? PieceKind.LineHorizontal : PieceKind.LineVertical;
                _creations.Add(new Creation(cell, match.Color, kind));
                _protected.Add(cell);
            }
        }

        // The new gem must be a normal gem that is not already claimed, so an existing special still fires.
        private bool TryChooseCreationCell(BoardModel board, Match match, GridPosition? swapFirst, GridPosition? swapSecond, out GridPosition cell)
        {
            if (swapFirst.HasValue && CanCreateAt(board, match, swapFirst.Value))
            {
                cell = swapFirst.Value;
                return true;
            }

            if (swapSecond.HasValue && CanCreateAt(board, match, swapSecond.Value))
            {
                cell = swapSecond.Value;
                return true;
            }

            for (var i = 0; i < match.Length; i++)
            {
                var candidate = match.GetPosition(i);
                if (board.GetKind(candidate) == PieceKind.Normal && !_protected.Contains(candidate))
                {
                    cell = candidate;
                    return true;
                }
            }

            cell = default;
            return false;
        }

        private bool CanCreateAt(BoardModel board, Match match, GridPosition cell)
        {
            if (board.GetKind(cell) != PieceKind.Normal || _protected.Contains(cell))
                return false;

            for (var i = 0; i < match.Length; i++)
            {
                if (match.GetPosition(i) == cell)
                    return true;
            }

            return false;
        }

        // A gem in an L or T belongs to two runs, so collect the distinct cells first and clear each one once.
        private void CollectMatchedCells()
        {
            _cells.Clear();
            foreach (var match in _matches)
            {
                for (var i = 0; i < match.Length; i++)
                {
                    var cell = match.GetPosition(i);
                    if (!_protected.Contains(cell))
                        _cells.Add(cell);
                }
            }
        }

        // Every special gem among the cleared cells fires, clearing its row or column. That can reach more special
        // gems, which fire in turn (a chain reaction), until nothing new is added.
        private int FireSpecials(BoardModel board)
        {
            _queue.Clear();
            _fired.Clear();

            foreach (var cell in _cells)
            {
                if (board.GetKind(cell) != PieceKind.Normal)
                    _queue.Add(cell);
            }

            var fired = 0;
            for (var i = 0; i < _queue.Count; i++)
            {
                var cell = _queue[i];
                if (!_fired.Add(cell))
                    continue;

                fired++;
                if (board.GetKind(cell) == PieceKind.LineHorizontal)
                {
                    for (var x = 0; x < board.Width; x++)
                        AddToClear(board, new GridPosition(x, cell.Y));
                }
                else
                {
                    for (var y = 0; y < board.Height; y++)
                        AddToClear(board, new GridPosition(cell.X, y));
                }
            }

            return fired;
        }

        private void AddToClear(BoardModel board, GridPosition cell)
        {
            if (board.GetPiece(cell) == PieceColor.None || _protected.Contains(cell))
                return;

            if (_cells.Add(cell) && board.GetKind(cell) != PieceKind.Normal)
                _queue.Add(cell);
        }

        private int ClearCells(BoardModel board, List<BoardStep> steps)
        {
            foreach (var cell in _cells)
            {
                steps.Add(BoardStep.Clear(board.GetPiece(cell), cell));
                board.SetPiece(cell, PieceColor.None);
            }

            return _cells.Count;
        }

        private int ApplyCreations(BoardModel board, List<BoardStep> steps)
        {
            foreach (var creation in _creations)
            {
                board.SetPiece(creation.Cell, creation.Color, creation.Kind);
                steps.Add(BoardStep.Create(creation.Color, creation.Kind, creation.Cell));
            }

            return _creations.Count;
        }
    }
}
