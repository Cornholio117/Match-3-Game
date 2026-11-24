using System;
using Game.Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Runtime.UI
{
    /// <summary>The pause popup on screen: it reports its button clicks and lets the presenter decide what they mean.</summary>
    public sealed class PausePopupView : PopupView, IPausePopupView
    {
        [Header("References")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _levelSelectButton;

        public event Action ResumeClicked;
        public event Action LevelSelectClicked;

        private void Awake()
        {
            if (_resumeButton == null || _levelSelectButton == null)
            {
                GameLog.Error(nameof(PausePopupView), $"Missing Resume or Level Select button on {name}");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(HandleResumeButton);

            if (_levelSelectButton != null)
                _levelSelectButton.onClick.AddListener(HandleLevelSelectButton);
        }

        private void OnDisable()
        {
            if (_resumeButton != null)
                _resumeButton.onClick.RemoveListener(HandleResumeButton);

            if (_levelSelectButton != null)
                _levelSelectButton.onClick.RemoveListener(HandleLevelSelectButton);
        }

        private void HandleResumeButton()
        {
            ResumeClicked?.Invoke();
        }

        private void HandleLevelSelectButton()
        {
            LevelSelectClicked?.Invoke();
        }
    }
}
