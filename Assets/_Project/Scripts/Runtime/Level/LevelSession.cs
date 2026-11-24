using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Level;
using Game.Core.UI;
using Game.Runtime.Board;
using Game.Runtime.UI;
using UnityEngine;

namespace Game.Runtime.Level
{
    /// <summary>
    /// Puts one level together in the scene: it builds the level's progress from its LevelConfig, shows it on the
    /// HUD through a presenter, and counts each finished turn from the board. It is a composition point, so it holds
    /// no rules of its own.
    /// </summary>
    public sealed class LevelSession : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardController _board;
        [SerializeField] private HudView _hud;

        [Header("Debug")]
        [SerializeField] private bool _debugMode;

        private LevelProgress _progress;
        private HudPresenter _hudPresenter;

        public LevelProgress Progress => _progress;

        private void Awake()
        {
            if (_board == null || _hud == null || _board.Level == null)
            {
                GameLog.Error(nameof(LevelSession), $"Missing board, HUD or level on {name}");
                enabled = false;
                return;
            }

            var level = _board.Level;
            _progress = new LevelProgress(level.GoalType, level.GoalAmount, level.GoalColor, level.Moves, new ScoreRules());
            _hudPresenter = new HudPresenter(_progress, _hud);
        }

        private void OnEnable()
        {
            _hudPresenter.Enable();
            _progress.StateChanged += HandleStateChanged;
            _board.TurnPlayed += HandleTurnPlayed;
        }

        private void OnDisable()
        {
            _board.TurnPlayed -= HandleTurnPlayed;
            _progress.StateChanged -= HandleStateChanged;
            _hudPresenter.Disable();
        }

        private void HandleTurnPlayed(List<BoardStep> steps)
        {
            _progress.ApplyTurn(steps);

            if (_debugMode)
                GameLog.Info(nameof(LevelSession), $"score {_progress.Score}, moves {_progress.Moves.Remaining}, goal {_progress.GoalProgress}/{_progress.GoalAmount}");
        }

        // The win and lose popups are the next step; for now the result is only logged.
        private void HandleStateChanged(LevelState state)
        {
            GameLog.Info(nameof(LevelSession), $"level {state}");
        }
    }
}
