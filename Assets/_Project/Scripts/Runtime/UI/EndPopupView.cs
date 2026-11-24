using System;
using Game.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Runtime.UI
{
    /// <summary>
    /// A win or lose popup on screen: it shows the result, the stars and, when offered, a Next Level button, and
    /// reports the button clicks. The star row and the Next Level button are optional, so the lose popup can leave them out.
    /// </summary>
    public sealed class EndPopupView : PopupView, IEndPopupView
    {
        [Header("References")]
        [SerializeField] private TMP_Text _resultLabel;
        [SerializeField] private Button _retryButton;

        [Header("Optional (win popup)")]
        [Tooltip("The button that starts the next level. Hidden when there is no next level to play.")]
        [SerializeField] private Button _nextLevelButton;

        [Tooltip("Three star images, left to right. Stars earned are tinted with the earned colour, the rest with the empty colour.")]
        [SerializeField] private Image[] _stars = new Image[0];

        [Header("Star colours")]
        [SerializeField] private Color _earnedColor = new Color(1f, 0.8f, 0.15f, 1f);
        [SerializeField] private Color _emptyColor = new Color(0.3f, 0.3f, 0.35f, 1f);

        public event Action RetryClicked;
        public event Action NextLevelClicked;

        private void Awake()
        {
            if (_resultLabel == null || _retryButton == null)
            {
                GameLog.Error(nameof(EndPopupView), $"Missing result label or Retry button on {name}");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_retryButton != null)
                _retryButton.onClick.AddListener(HandleRetryButton);

            if (_nextLevelButton != null)
                _nextLevelButton.onClick.AddListener(HandleNextLevelButton);
        }

        private void OnDisable()
        {
            if (_retryButton != null)
                _retryButton.onClick.RemoveListener(HandleRetryButton);

            if (_nextLevelButton != null)
                _nextLevelButton.onClick.RemoveListener(HandleNextLevelButton);
        }

        // SetText with number arguments writes into the text buffer without creating a string (R5.1).
        public void ShowResult(int score, int goalProgress, int goalAmount, int stars, bool canGoNext)
        {
            _resultLabel.SetText("Score: {0:0}\nGoal: {1:0} / {2:0}", score, goalProgress, goalAmount);

            for (var i = 0; i < _stars.Length; i++)
                _stars[i].color = i < stars ? _earnedColor : _emptyColor;

            if (_nextLevelButton != null)
                _nextLevelButton.gameObject.SetActive(canGoNext);
        }

        private void HandleRetryButton()
        {
            RetryClicked?.Invoke();
        }

        private void HandleNextLevelButton()
        {
            NextLevelClicked?.Invoke();
        }
    }
}
