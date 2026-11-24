using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Level;
using Game.Core.UI;
using Game.Runtime.Board;
using Game.Runtime.Config;
using Game.Runtime.UI;
using UnityEngine;

namespace Game.Runtime.Level
{
    /// <summary>
    /// Puts levels together in the scene and runs them one after another: it starts any level from the catalog,
    /// builds that level's progress, shows it on the HUD, opens the win or lose popup when the level ends, and counts
    /// each finished turn from the board. It remembers the player's stars. It is a composition point, so it holds no
    /// rules of its own.
    /// </summary>
    public sealed class LevelSession : MonoBehaviour, ILevelFlow
    {
        [Header("References")]
        [SerializeField] private BoardController _board;
        [SerializeField] private HudView _hud;
        [SerializeField] private PopupManager _popups;
        [SerializeField] private EndPopupView _winPopup;
        [SerializeField] private EndPopupView _losePopup;

        [Header("Levels")]
        [SerializeField] private LevelCatalog _catalog;

        [Tooltip("The level to start with, counting from 0.")]
        [SerializeField, Min(0)] private int _firstLevelIndex;

        [Header("Debug")]
        [SerializeField] private bool _debugMode;

        private PlayerProgress _player;
        private LevelProgress _progress;
        private int _levelIndex;
        private HudPresenter _hudPresenter;
        private LevelEndPresenter _levelEndPresenter;
        private EndPopupPresenter _winPresenter;
        private EndPopupPresenter _losePresenter;

        public LevelProgress Progress => _progress;

        /// <summary>The stars the player has earned so far.</summary>
        public PlayerProgress Player => _player;

        /// <summary>Which level is being played, counting from 0.</summary>
        public int LevelIndex => _levelIndex;

        /// <summary>True when there is a next level and the player has unlocked it.</summary>
        public bool CanStartNextLevel => _player != null && _player.HasNextLevel(_levelIndex) && _player.IsUnlocked(_levelIndex + 1);

        private void Awake()
        {
            if (_board == null || _hud == null || _popups == null || _winPopup == null || _losePopup == null || _catalog == null || _catalog.Count == 0)
            {
                GameLog.Error(nameof(LevelSession), $"Missing board, HUD, popup or level catalog reference on {name}");
                enabled = false;
                return;
            }

            _player = new PlayerProgress(_catalog.Count);
        }

        private void OnEnable()
        {
            _board.TurnPlayed += HandleTurnPlayed;
        }

        private void OnDisable()
        {
            _board.TurnPlayed -= HandleTurnPlayed;
            DisablePresenters();
        }

        // Start, not Awake: the board view must have finished its own Awake before a board is built.
        private void Start()
        {
            StartLevel(Mathf.Clamp(_firstLevelIndex, 0, _catalog.Count - 1));
        }

        /// <summary>Starts a level from the catalog: a fresh board, new progress and new presenters.</summary>
        public void StartLevel(int index)
        {
            DisablePresenters();

            // A new level never starts with the last level's result popups still open (the buttons close them first,
            // but a restart from code or a back-navigation must be safe too).
            _popups.Stack.Close(PopupId.Win);
            _popups.Stack.Close(PopupId.Lose);

            _levelIndex = index;
            var level = _catalog.Get(index);
            _board.StartLevel(level);
            BuildProgress(level);
            EnablePresenters();

            if (_debugMode)
                GameLog.Info(nameof(LevelSession), $"started level {index + 1} ({level.name})");
        }

        /// <summary>Starts the current level over.</summary>
        public void Restart()
        {
            StartLevel(_levelIndex);
        }

        /// <summary>Starts the next level; does nothing when there is none or it is still locked.</summary>
        public void StartNextLevel()
        {
            if (CanStartNextLevel)
                StartLevel(_levelIndex + 1);
        }

        private void BuildProgress(LevelConfig level)
        {
            var stack = _popups.Stack;

            _progress = new LevelProgress(level.GoalType, level.GoalAmount, level.GoalColor, level.Moves, new ScoreRules());
            _hudPresenter = new HudPresenter(_progress, _hud);
            _levelEndPresenter = new LevelEndPresenter(_progress, stack, _player, _levelIndex);
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

        // Safe to call before any level has started: there is nothing to disable yet.
        private void DisablePresenters()
        {
            if (_progress == null)
                return;

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
            GameLog.Info(nameof(LevelSession), $"level {state}, total stars {_player.TotalStars}");
        }
    }
}
