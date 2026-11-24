using System;
using Game.Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Runtime.UI
{
    /// <summary>The level select screen on screen: a button per level and a Back button. It only reports taps.</summary>
    public sealed class LevelSelectView : MonoBehaviour, ILevelSelectView
    {
        [Header("References")]
        [Tooltip("One button per level, in level order.")]
        [SerializeField] private LevelButtonView[] _levelButtons = new LevelButtonView[0];
        [SerializeField] private Button _backButton;

        public event Action<int> LevelClicked;
        public event Action BackClicked;

        private void Awake()
        {
            if (_backButton == null || _levelButtons.Length == 0)
            {
                GameLog.Error(nameof(LevelSelectView), $"Missing Back button or level buttons on {name}");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            for (var i = 0; i < _levelButtons.Length; i++)
                _levelButtons[i].Clicked += HandleLevelButton;

            if (_backButton != null)
                _backButton.onClick.AddListener(HandleBackButton);
        }

        private void OnDisable()
        {
            for (var i = 0; i < _levelButtons.Length; i++)
                _levelButtons[i].Clicked -= HandleLevelButton;

            if (_backButton != null)
                _backButton.onClick.RemoveListener(HandleBackButton);
        }

        public void ShowLevel(int levelIndex, int stars, bool unlocked)
        {
            if (levelIndex >= 0 && levelIndex < _levelButtons.Length)
                _levelButtons[levelIndex].Show(levelIndex + 1, stars, unlocked);
        }

        private void HandleLevelButton(LevelButtonView button)
        {
            var index = Array.IndexOf(_levelButtons, button);
            if (index >= 0)
                LevelClicked?.Invoke(index);
        }

        private void HandleBackButton()
        {
            BackClicked?.Invoke();
        }
    }
}
