using System;
using System.Collections.Generic;
using Game.Core.Common;

namespace Game.Core.Board
{
    /// <summary>What a resolved turn did: how many clear waves ran, how many gems were cleared, and whether it was cut short.</summary>
    public readonly struct TurnResult
    {
        public int Cascades { get; }
        public int CellsCleared { get; }
        public bool HitLimit { get; }

        public TurnResult(int cascades, int cellsCleared, bool hitLimit)
        {
            Cascades = cascades;
            CellsCleared = cellsCleared;
            HitLimit = hitLimit;
        }
    }

    /// <summary>Runs the clear, fall, refill loop until the board has no runs left, or the cascade limit is reached.</summary>
    public sealed class TurnResolver
    {
        public const int DefaultMaxCascades = 50;

        private readonly int _maxCascades;
        private readonly List<Match> _matches = new List<Match>();
        private readonly HashSet<GridPosition> _cells = new HashSet<GridPosition>();

        public TurnResolver(int maxCascades = DefaultMaxCascades)
        {
            if (maxCascades < 1)
                throw new ArgumentOutOfRangeException(nameof(maxCascades));

            _maxCascades = maxCascades;
        }

        /// <summary>
        /// Resolves everything on the board, appending what happened to steps (cleared first, then falls, then spawns,
        /// once per wave). The limit stops a pathological refill from looping forever.
        /// </summary>
        public TurnResult Resolve(BoardModel board, IRandom random, int colorCount, List<BoardStep> steps)
        {
            var cascades = 0;
            var cleared = 0;

            while (true)
            {
                MatchFinder.Find(board, _matches);
                if (_matches.Count == 0)
                    return new TurnResult(cascades, cleared, false);

                if (cascades >= _maxCascades)
                    return new TurnResult(cascades, cleared, true);

                cleared += ClearMatchedCells(board, steps);
                BoardGravity.Apply(board, steps);
                BoardRefill.Apply(board, random, colorCount, steps);
                cascades++;
            }
        }

        // A gem in an L or T belongs to two runs, so collect the distinct cells first and clear each one once.
        private int ClearMatchedCells(BoardModel board, List<BoardStep> steps)
        {
            _cells.Clear();
            foreach (var match in _matches)
            {
                for (var i = 0; i < match.Length; i++)
                    _cells.Add(match.GetPosition(i));
            }

            foreach (var cell in _cells)
            {
                steps.Add(BoardStep.Clear(board.GetPiece(cell), cell));
                board.SetPiece(cell, PieceColor.None);
            }

            return _cells.Count;
        }
    }
}
