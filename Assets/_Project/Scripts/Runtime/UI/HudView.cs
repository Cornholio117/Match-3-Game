using Game.Core.Board;
using Game.Core.Level;
using Game.Core.UI;
using TMPro;
using UnityEngine;

namespace Game.Runtime.UI
{
    /// <summary>The HUD labels on screen. It only formats and displays what the presenter tells it.</summary>
    public sealed class HudView : MonoBehaviour, IHudView
    {
        [Header("References")]
        [SerializeField] private TMP_Text _movesLabel;
        [SerializeField] private TMP_Text _scoreLabel;
        [SerializeField] private TMP_Text _goalLabel;

        private string _goalFormat;
        private LevelGoalType _formatType;
        private PieceColor _formatColor;

        private void Awake()
        {
            if (_movesLabel == null || _scoreLabel == null || _goalLabel == null)
            {
                GameLog.Error(nameof(HudView), $"Missing label on {name}");
                enabled = false;
            }
        }

        // SetText with number arguments writes straight into the text buffer, so updating a label allocates no
        // string (R5.1). The {0:0} format shows whole numbers.
        public void ShowMoves(int remaining)
        {
            _movesLabel.SetText("Moves: {0:0}", remaining);
        }

        public void ShowScore(int score)
        {
            _scoreLabel.SetText("Score: {0:0}", score);
        }

        public void ShowGoal(LevelGoalType type, PieceColor color, int current, int target)
        {
            // The text before the numbers only changes when the goal itself changes, so it is built once, not per update.
            if (_goalFormat == null || type != _formatType || color != _formatColor)
            {
                _formatType = type;
                _formatColor = color;
                _goalFormat = type == LevelGoalType.Score
                    ? "Goal: {0:0} / {1:0}"
                    : "Collect " + color + ": {0:0} / {1:0}";
            }

            _goalLabel.SetText(_goalFormat, current, target);
        }
    }
}
