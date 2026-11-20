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
        private readonly List<bool> _consumed = new List<bool>();

        public TurnResolver(int maxCascades = DefaultMaxCascades)
        {
            if (maxCascades < 1)
                throw new ArgumentOutOfRangeException(nameof(maxCascades));

            _maxCascades = maxCascades;
        }

        /// <summary>
        /// Resolves everything on the board, appending what happened to steps. swapFirst and swapSecond are the cells
        /// the player just swapped: in the first wave a new special gem appears on one of them. When combo is true
        /// the swap joined two special gems: both are used up and the first wave also clears the row and column
        /// through swapSecond, the cell the player moved to. The limit stops a pathological refill from looping forever.
        /// </summary>
        public TurnResult Resolve(
            BoardModel board,
            IRandom random,
            int colorCount,
            List<BoardStep> steps,
            GridPosition? swapFirst = null,
            GridPosition? swapSecond = null,
            bool combo = false)
        {
            var cascades = 0;
            var cleared = 0;
            var created = 0;
            var fired = 0;

            while (true)
            {
                MatchFinder.Find(board, _matches);

                // Only the first wave has a player move; later waves are cascades and use a fixed fallback cell.
                var first = cascades == 0 ? swapFirst : null;
                var second = cascades == 0 ? swapSecond : null;
                var isComboWave = combo && first.HasValue && second.HasValue;

                if (_matches.Count == 0 && !isComboWave)
                    return new TurnResult(cascades, cleared, false, created, fired);

                if (cascades >= _maxCascades)
                    return new TurnResult(cascades, cleared, true, created, fired);

                PlanCreations(board, first, second);
                CollectMatchedCells();
                fired += FireSpecials(board, isComboWave ? first : null, isComboWave ? second : null);
                cleared += ClearCells(board, steps);
                created += ApplyCreations(board, steps);

                BoardGravity.Apply(board, steps);
                BoardRefill.Apply(board, random, colorCount, steps);
                cascades++;
            }
        }

        // Two kinds of new special gem, in priority order:
        // 1. An L, T or + shape (a horizontal and a vertical run that cross) makes a diagonal gem.
        // 2. A run of exactly 4 that is not part of such a shape makes a line gem: a horizontal run clears its row
        //    later, a vertical run its column.
        // Either appears where the player moved if that cell is in the shape, otherwise in a fixed fallback cell.
        private void PlanCreations(BoardModel board, GridPosition? swapFirst, GridPosition? swapSecond)
        {
            _creations.Clear();
            _protected.Clear();
            _consumed.Clear();
            for (var i = 0; i < _matches.Count; i++)
                _consumed.Add(false);

            for (var h = 0; h < _matches.Count; h++)
            {
                if (_matches[h].Direction != Direction.Right)
                    continue;

                for (var v = 0; v < _matches.Count && !_consumed[h]; v++)
                {
                    if (_matches[v].Direction != Direction.Up || _consumed[v])
                        continue;

                    if (!TryFindCrossing(_matches[h], _matches[v], out var crossing))
                        continue;

                    if (!TryChooseShapeCell(board, _matches[h], _matches[v], crossing, swapFirst, swapSecond, out var shapeCell))
                        continue;

                    _creations.Add(new Creation(shapeCell, _matches[h].Color, PieceKind.Diagonal));
                    _protected.Add(shapeCell);
                    _consumed[h] = true;
                    _consumed[v] = true;
                }
            }

            for (var i = 0; i < _matches.Count; i++)
            {
                var match = _matches[i];
                if (_consumed[i] || match.Length != 4)
                    continue;

                if (!TryChooseCreationCell(board, match, swapFirst, swapSecond, out var cell))
                    continue;

                var kind = match.Direction == Direction.Right ? PieceKind.LineHorizontal : PieceKind.LineVertical;
                _creations.Add(new Creation(cell, match.Color, kind));
                _protected.Add(cell);
            }
        }

        // A horizontal and a vertical run cross when the vertical run's column falls inside the horizontal run and
        // the horizontal run's row falls inside the vertical run. The crossing cell is where they meet.
        private static bool TryFindCrossing(Match horizontal, Match vertical, out GridPosition crossing)
        {
            var columnInside = vertical.Start.X >= horizontal.Start.X && vertical.Start.X < horizontal.Start.X + horizontal.Length;
            var rowInside = horizontal.Start.Y >= vertical.Start.Y && horizontal.Start.Y < vertical.Start.Y + vertical.Length;

            crossing = new GridPosition(vertical.Start.X, horizontal.Start.Y);
            return columnInside && rowInside;
        }

        // Where the player moved if that cell is in the shape, else the crossing, else the first free cell of the shape.
        private bool TryChooseShapeCell(
            BoardModel board, Match horizontal, Match vertical, GridPosition crossing,
            GridPosition? swapFirst, GridPosition? swapSecond, out GridPosition cell)
        {
            if (swapFirst.HasValue && CanCreateInShape(board, horizontal, vertical, swapFirst.Value))
            {
                cell = swapFirst.Value;
                return true;
            }

            if (swapSecond.HasValue && CanCreateInShape(board, horizontal, vertical, swapSecond.Value))
            {
                cell = swapSecond.Value;
                return true;
            }

            if (CanCreateInShape(board, horizontal, vertical, crossing))
            {
                cell = crossing;
                return true;
            }

            for (var i = 0; i < horizontal.Length; i++)
            {
                if (CanCreateInShape(board, horizontal, vertical, horizontal.GetPosition(i)))
                {
                    cell = horizontal.GetPosition(i);
                    return true;
                }
            }

            for (var i = 0; i < vertical.Length; i++)
            {
                if (CanCreateInShape(board, horizontal, vertical, vertical.GetPosition(i)))
                {
                    cell = vertical.GetPosition(i);
                    return true;
                }
            }

            cell = default;
            return false;
        }

        private bool CanCreateInShape(BoardModel board, Match horizontal, Match vertical, GridPosition cell)
        {
            if (board.GetKind(cell) != PieceKind.Normal || _protected.Contains(cell))
                return false;

            return RunContains(horizontal, cell) || RunContains(vertical, cell);
        }

        private static bool RunContains(Match match, GridPosition cell)
        {
            for (var i = 0; i < match.Length; i++)
            {
                if (match.GetPosition(i) == cell)
                    return true;
            }

            return false;
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
        private int FireSpecials(BoardModel board, GridPosition? comboFirst, GridPosition? comboSecond)
        {
            _queue.Clear();
            _fired.Clear();

            var fired = 0;

            if (comboFirst.HasValue && comboSecond.HasValue)
            {
                // A combo uses both gems up: they clear but do not fire their own effects. What it clears depends on
                // the pair, always centred on the cell the player moved to (the second one):
                //   line + line         the row and the column
                //   line + diagonal     the row, the column and both diagonals (a star)
                //   diagonal + diagonal both diagonals through each of the two swapped cells
                var first = comboFirst.Value;
                var center = comboSecond.Value;
                _fired.Add(first);
                _fired.Add(center);
                _cells.Add(first);
                _cells.Add(center);

                var firstIsDiagonal = board.GetKind(first) == PieceKind.Diagonal;
                var secondIsDiagonal = board.GetKind(center) == PieceKind.Diagonal;

                if (firstIsDiagonal && secondIsDiagonal)
                {
                    AddDiagonals(board, first);
                    AddDiagonals(board, center);
                }
                else
                {
                    AddRow(board, center.Y);
                    AddColumn(board, center.X);

                    if (firstIsDiagonal || secondIsDiagonal)
                        AddDiagonals(board, center);
                }

                fired++;
            }

            foreach (var cell in _cells)
            {
                if (board.GetKind(cell) != PieceKind.Normal)
                    _queue.Add(cell);
            }

            for (var i = 0; i < _queue.Count; i++)
            {
                var cell = _queue[i];
                if (!_fired.Add(cell))
                    continue;

                fired++;
                var kind = board.GetKind(cell);
                if (kind == PieceKind.LineHorizontal)
                    AddRow(board, cell.Y);
                else if (kind == PieceKind.LineVertical)
                    AddColumn(board, cell.X);
                else if (kind == PieceKind.Diagonal)
                    AddDiagonals(board, cell);
            }

            return fired;
        }

        private void AddRow(BoardModel board, int y)
        {
            for (var x = 0; x < board.Width; x++)
                AddToClear(board, new GridPosition(x, y));
        }

        private void AddColumn(BoardModel board, int x)
        {
            for (var y = 0; y < board.Height; y++)
                AddToClear(board, new GridPosition(x, y));
        }

        // Both diagonals through a cell: walk outward in each of the four diagonal directions, staying on the board.
        private void AddDiagonals(BoardModel board, GridPosition center)
        {
            var reach = Math.Max(board.Width, board.Height);
            for (var d = -reach; d <= reach; d++)
            {
                var up = new GridPosition(center.X + d, center.Y + d);
                if (up.IsInside(board.Width, board.Height))
                    AddToClear(board, up);

                var down = new GridPosition(center.X + d, center.Y - d);
                if (down.IsInside(board.Width, board.Height))
                    AddToClear(board, down);
            }
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
