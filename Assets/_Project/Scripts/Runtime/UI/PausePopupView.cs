using System;
using Game.Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Runtime.UI
{
    /// <summary>The pause popup on screen: it reports the Resume click and lets the presenter decide what it means.</summary>
    public sealed class PausePopupView : PopupView, IPausePopupView
    {
        [Header("References")]
        [SerializeField] private Button _resumeButton;

        public event Action ResumeClicked;

        private void Awake()
        {
            if (_resumeButton == null)
            {
                GameLog.Error(nameof(PausePopupView), $"Missing Resume button on {name}");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(HandleResumeButton);
        }

        private void OnDisable()
        {
            if (_resumeButton != null)
                _resumeButton.onClick.RemoveListener(HandleResumeButton);
        }

        private void HandleResumeButton()
        {
            ResumeClicked?.Invoke();
        }
    }
}
