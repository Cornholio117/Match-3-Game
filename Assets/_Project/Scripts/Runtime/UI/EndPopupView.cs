using System;
using Game.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Runtime.UI
{
    /// <summary>A win or lose popup on screen: it shows the result and reports the Retry click.</summary>
    public sealed class EndPopupView : PopupView, IEndPopupView
    {
        [Header("References")]
        [SerializeField] private TMP_Text _resultLabel;
        [SerializeField] private Button _retryButton;

        public event Action RetryClicked;

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
        }

        private void OnDisable()
        {
            if (_retryButton != null)
                _retryButton.onClick.RemoveListener(HandleRetryButton);
        }

        // SetText with number arguments writes into the text buffer without creating a string (R5.1).
        public void ShowResult(int score, int goalProgress, int goalAmount)
        {
            _resultLabel.SetText("Score: {0:0}\nGoal: {1:0} / {2:0}", score, goalProgress, goalAmount);
        }

        private void HandleRetryButton()
        {
            RetryClicked?.Invoke();
        }
    }
}
