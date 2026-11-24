using System;
using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Common;
using Game.Runtime.Config;
using UnityEngine;

namespace Game.Runtime.Board
{
    /// <summary>
    /// Connects input, the board game and the board view. It owns the busy flag: while the view is animating a turn,
    /// new swipes are ignored, so two turns can never overlap.
    /// </summary>
    public sealed class BoardController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardView _boardView;
        [SerializeField] private BoardInputHandler _input;

        [Header("Game")]
        [Tooltip("How far a shockwave gem reaches, in cells: 2 clears a 5x5 square, 1 a 3x3 square.")]
        [SerializeField, Range(1, 4)] private int _shockwaveRadius = 2;

        [Tooltip("Board seed. 0 means a different board every run, taken from the clock; the seed is logged so a board can be replayed.")]
        [SerializeField] private int _seed;

        [Header("Debug")]
        [SerializeField] private bool _debugMode;

        private readonly List<BoardStep> _steps = new List<BoardStep>();
        private BoardGame _game;
        private LevelConfig _level;
        private bool _isBusy;

        /// <summary>The level being played; null until StartLevel has been called.</summary>
        public LevelConfig Level => _level;

        /// <summary>
        /// Raised after an accepted swap has finished animating, with the steps of that turn. The list is reused for
        /// the next turn, so listeners must use it during the call and not keep it.
        /// </summary>
        public event Action<List<BoardStep>> TurnPlayed;

        private void Awake()
        {
            if (_boardView == null || _input == null)
            {
                GameLog.Error(nameof(BoardController), $"Missing board view or input on {name}");
                enabled = false;
            }
        }

        /// <summary>
        /// Starts a level with a fresh board: its size, colours and shockwave radius decide the board. Nothing is built
        /// until this is called, so the game decides when and which level. Call it between turns (from a button or at
        /// start-up), never while a turn is animating. It must not run before the board view's own Awake, so call it
        /// from Start or later.
        /// </summary>
        public void StartLevel(LevelConfig level)
        {
            if (level == null)
            {
                GameLog.Error(nameof(BoardController), $"StartLevel called with no level on {name}");
                return;
            }

            _level = level;
            StartNewGame();
        }

        private void StartNewGame()
        {
            var seed = _seed != 0 ? _seed : Environment.TickCount;
            _game = BoardGame.CreateFilled(_level.Width, _level.Height, _level.ColorCount, new SeededRandom(seed),
                shockwaveRadius: _shockwaveRadius);
            _boardView.Build(_game.Board);
            GameLog.Info(nameof(BoardController), $"New board, seed {seed}");
        }

        private void OnEnable()
        {
            _input.SwapRequested += HandleSwapRequested;
        }

        private void OnDisable()
        {
            _input.SwapRequested -= HandleSwapRequested;
        }

        private void HandleSwapRequested(GridPosition a, GridPosition b)
        {
            if (_isBusy || _game == null)
                return;

            // A swipe off the edge of the board has no neighbour to swap with.
            if (!a.IsInside(_level.Width, _level.Height) || !b.IsInside(_level.Width, _level.Height))
                return;

            _ = ResolveSwapAsync(a, b);
        }

        private async Awaitable ResolveSwapAsync(GridPosition a, GridPosition b)
        {
            _isBusy = true;

            try
            {
                // The model resolves the whole turn instantly; the view then plays it back step by step.
                var outcome = _game.TrySwap(a, b, _steps);

                if (outcome.Accepted)
                {
                    await _boardView.PlaySwapAsync(a, b);
                    await _boardView.PlayStepsAsync(_steps);
                    TurnPlayed?.Invoke(_steps);

                    if (_debugMode)
                        GameLog.Info(nameof(BoardController), $"cascades {outcome.Turn.Cascades}, cleared {outcome.Turn.CellsCleared}, limit hit {outcome.Turn.HitLimit}");
                }
                else
                {
                    await _boardView.PlayRejectedSwapAsync(a, b);
                }
            }
            catch (OperationCanceledException)
            {
                // The scene is being torn down mid-animation; nothing to do.
            }
            catch (Exception exception)
            {
                GameLog.Error(nameof(BoardController), exception.ToString());
            }
            finally
            {
                _isBusy = false;
            }
        }
    }
}
