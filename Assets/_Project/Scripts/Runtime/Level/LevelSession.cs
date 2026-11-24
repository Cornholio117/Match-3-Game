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
    /// HUD, opens the win or lose popup when the level ends, and counts each finished turn from the board. It is a
    /// composition point, so it holds no rules of its own. Restart builds everything again for a fresh attempt.
    /// </summary>
    public sealed class LevelSession : MonoBehaviour, ILevelRestarter
    {
        [Header("References")]
        [SerializeField] private BoardController _board;
        [SerializeField] private HudView _hud;
        [SerializeField] private PopupManager _popups;
        [SerializeField] private EndPopupView _winPopup;
        [SerializeField] private EndPopupView _losePopup;

        [Header("Debug")]
        [SerializeField] private bool _debugMode;

        private LevelProgress _progress;
        private HudPresenter _hudPresenter;
        private LevelEndPresenter _levelEndPresenter;
        private EndPopupPresenter _winPresenter;
        private EndPopupPresenter _losePresenter;

        public LevelProgress Progress => _progress;

        private void Awake()
        {
            if (_board == null || _hud == null || _popups == null || _winPopup == null || _losePopup == null || _board.Level == null)
            {
                GameLog.Error(nameof(LevelSession), $"Missing board, HUD, popup or level reference on {name}");
                enabled = false;
                return;
            }

            BuildProgress();
        }

        private void OnEnable()
        {
            EnablePresenters();
            _board.TurnPlayed += HandleTurnPlayed;
        }

        private void OnDisable()
        {
            _board.TurnPlayed -= HandleTurnPlayed;
            DisablePresenters();
        }

        /// <summary>Starts the level over: new progress, new presenters and a fresh board.</summary>
        public void Restart()
        {
            DisablePresenters();
            BuildProgress();
            EnablePresenters();
            _board.Restart();

            if (_debugMode)
                GameLog.Info(nameof(LevelSession), "level restarted");
        }

        private void BuildProgress()
        {
            var level = _board.Level;
            var stack = _popups.Stack;

            _progress = new LevelProgress(level.GoalType, level.GoalAmount, level.GoalColor, level.Moves, new ScoreRules());
            _hudPresenter = new HudPresenter(_progress, _hud);
            _levelEndPresenter = new LevelEndPresenter(_progress, stack);
            _winPresenter = new EndPopupPresenter(_winPopup, stack, PopupId.Win, _progress, this);
            _losePresenter = new EndPopupPresenter(_losePopup, stack, PopupId.Lose, _progress, this);
        }

        private void EnablePresenters()
        {
            _hudPresenter.Enable();
            _levelEndPresenter.Enable();
            _winPresenter.Enable();
            _losePresenter.Enable();
            _progress.StateChanged += HandleStateChanged;
        }

        private void DisablePresenters()
        {
            _progress.StateChanged -= HandleStateChanged;
            _losePresenter.Disable();
            _winPresenter.Disable();
            _levelEndPresenter.Disable();
            _hudPresenter.Disable();
        }

        private void HandleTurnPlayed(List<BoardStep> steps)
        {
            _progress.ApplyTurn(steps);

            if (_debugMode)
                GameLog.Info(nameof(LevelSession), $"score {_progress.Score}, moves {_progress.Moves.Remaining}, goal {_progress.GoalProgress}/{_progress.GoalAmount}");
        }

        private void HandleStateChanged(LevelState state)
        {
            GameLog.Info(nameof(LevelSession), $"level {state}");
        }
    }
}
