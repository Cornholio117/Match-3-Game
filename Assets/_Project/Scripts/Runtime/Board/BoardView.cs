using System.Collections.Generic;
using Game.Core.Board;
using Game.Runtime.Config;
using UnityEngine;

namespace Game.Runtime.Board
{
    /// <summary>
    /// Shows the board and plays back what happened to it. It holds no game rules: the model decides, this only
    /// animates the recorded steps (clear, fall, spawn) and the swap itself.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PiecePool _pool;
        [SerializeField] private PieceSet _pieceSet;

        [Header("Animation")]
        [Tooltip("Seconds for two gems to trade places.")]
        [SerializeField, Min(0.01f)] private float _swapSeconds = 0.15f;

        [Tooltip("Seconds for cleared gems to shrink away.")]
        [SerializeField, Min(0.01f)] private float _clearSeconds = 0.15f;

        [Tooltip("Seconds for gems to fall into gaps.")]
        [SerializeField, Min(0.01f)] private float _fallSeconds = 0.2f;

        [Tooltip("Seconds for new gems to drop in from above the board.")]
        [SerializeField, Min(0.01f)] private float _spawnSeconds = 0.25f;

        [Header("Debug")]
        [SerializeField] private bool _debugMode;

        // Reused every animation so playing a turn creates no garbage.
        private readonly List<PieceView> _moving = new List<PieceView>();
        private readonly List<Vector3> _from = new List<Vector3>();
        private readonly List<Vector3> _to = new List<Vector3>();

        private PieceView[,] _pieces;
        private int _width;
        private int _height;

        private void Awake()
        {
            if (_pool == null || _pieceSet == null)
            {
                GameLog.Error(nameof(BoardView), $"Missing pool or piece set on {name}");
                enabled = false;
            }
        }

        /// <summary>Shows a board instantly, with every gem in its cell.</summary>
        public void Build(BoardModel board)
        {
            ReleaseAll();

            _width = board.Width;
            _height = board.Height;
            _pieces = new PieceView[_width, _height];

            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    var position = new GridPosition(x, y);
                    _pieces[x, y] = CreatePiece(board.GetPiece(position), CellToWorld(position));
                }
            }
        }

        public Vector3 CellToWorld(GridPosition cell)
        {
            return transform.TransformPoint(cell.X, cell.Y, 0f);
        }

        /// <summary>Finds the board cell under a world position. False when the position is outside the board.</summary>
        public bool TryGetCell(Vector3 world, out GridPosition cell)
        {
            var local = transform.InverseTransformPoint(world);
            cell = new GridPosition(Mathf.RoundToInt(local.x), Mathf.RoundToInt(local.y));
            return cell.IsInside(_width, _height);
        }

        /// <summary>Slides two gems into each other's cell.</summary>
        public async Awaitable PlaySwapAsync(GridPosition a, GridPosition b)
        {
            var first = _pieces[a.X, a.Y];
            var second = _pieces[b.X, b.Y];
            _pieces[a.X, a.Y] = second;
            _pieces[b.X, b.Y] = first;

            BeginMoves();
            AddMove(first, CellToWorld(b));
            AddMove(second, CellToWorld(a));
            await RunMovesAsync(_swapSeconds);
        }

        /// <summary>Slides two gems toward each other and back, to show a swap that was not allowed.</summary>
        public async Awaitable PlayRejectedSwapAsync(GridPosition a, GridPosition b)
        {
            var first = _pieces[a.X, a.Y];
            var second = _pieces[b.X, b.Y];

            BeginMoves();
            AddMove(first, CellToWorld(b));
            AddMove(second, CellToWorld(a));
            await RunMovesAsync(_swapSeconds * 0.5f);

            BeginMoves();
            AddMove(first, CellToWorld(a));
            AddMove(second, CellToWorld(b));
            await RunMovesAsync(_swapSeconds * 0.5f);
        }

        /// <summary>Plays the recorded steps in order; steps of the same kind that follow each other play together.</summary>
        public async Awaitable PlayStepsAsync(List<BoardStep> steps)
        {
            var start = 0;
            while (start < steps.Count)
            {
                var kind = steps[start].Kind;
                var end = start;
                while (end < steps.Count && steps[end].Kind == kind)
                    end++;

                switch (kind)
                {
                    case BoardStepKind.Clear:
                        await PlayClearsAsync(steps, start, end);
                        break;
                    case BoardStepKind.Fall:
                        await PlayFallsAsync(steps, start, end);
                        break;
                    case BoardStepKind.Spawn:
                        await PlaySpawnsAsync(steps, start, end);
                        break;
                }

                start = end;
            }
        }

        private async Awaitable PlayClearsAsync(List<BoardStep> steps, int start, int end)
        {
            _moving.Clear();
            for (var i = start; i < end; i++)
            {
                var cell = steps[i].From;
                _moving.Add(_pieces[cell.X, cell.Y]);
                _pieces[cell.X, cell.Y] = null;
            }

            var elapsed = 0f;
            while (elapsed < _clearSeconds)
            {
                await Awaitable.NextFrameAsync(destroyCancellationToken);
                elapsed += Time.deltaTime;
                var scale = 1f - Mathf.Clamp01(elapsed / _clearSeconds);
                for (var i = 0; i < _moving.Count; i++)
                    _moving[i].transform.localScale = new Vector3(scale, scale, 1f);
            }

            for (var i = 0; i < _moving.Count; i++)
                _pool.Release(_moving[i]);
        }

        private async Awaitable PlayFallsAsync(List<BoardStep> steps, int start, int end)
        {
            BeginMoves();
            for (var i = start; i < end; i++)
            {
                var from = steps[i].From;
                var to = steps[i].To;
                var piece = _pieces[from.X, from.Y];
                _pieces[from.X, from.Y] = null;
                _pieces[to.X, to.Y] = piece;
                AddMove(piece, CellToWorld(to));
            }

            await RunMovesAsync(_fallSeconds);
        }

        private async Awaitable PlaySpawnsAsync(List<BoardStep> steps, int start, int end)
        {
            BeginMoves();
            for (var i = start; i < end; i++)
            {
                var cell = steps[i].From;

                // Start one board-height above the target so new gems drop in from above and keep their spacing.
                var target = CellToWorld(cell);
                var piece = CreatePiece(steps[i].Color, target + Vector3.up * _height);
                _pieces[cell.X, cell.Y] = piece;
                AddMove(piece, target);
            }

            await RunMovesAsync(_spawnSeconds);
        }

        private PieceView CreatePiece(PieceColor color, Vector3 position)
        {
            var piece = _pool.Get();
            piece.SetPiece(color, _pieceSet.GetSprite(color));
            piece.transform.position = position;
            return piece;
        }

        private void ReleaseAll()
        {
            if (_pieces == null)
                return;

            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    if (_pieces[x, y] != null)
                        _pool.Release(_pieces[x, y]);
                }
            }
        }

        private void BeginMoves()
        {
            _moving.Clear();
            _from.Clear();
            _to.Clear();
        }

        private void AddMove(PieceView piece, Vector3 target)
        {
            _moving.Add(piece);
            _from.Add(piece.transform.position);
            _to.Add(target);
        }

        // Moves every queued gem from where it is to its target together, easing out so they settle softly.
        private async Awaitable RunMovesAsync(float seconds)
        {
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                await Awaitable.NextFrameAsync(destroyCancellationToken);
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / seconds);
                var eased = 1f - (1f - t) * (1f - t);
                for (var i = 0; i < _moving.Count; i++)
                    _moving[i].transform.position = Vector3.Lerp(_from[i], _to[i], eased);
            }

            for (var i = 0; i < _moving.Count; i++)
                _moving[i].transform.position = _to[i];

            if (_debugMode)
                GameLog.Info(nameof(BoardView), $"moved {_moving.Count} gems in {seconds:F2}s");
        }

        private void OnDrawGizmos()
        {
            if (!_debugMode || _width == 0) return;

            Gizmos.color = Color.green;
            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                    Gizmos.DrawWireCube(CellToWorld(new GridPosition(x, y)), new Vector3(1f, 1f, 0f));
            }
        }
    }
}
