using System;
using Game.Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Runtime.UI
{
    /// <summary>The main menu on screen: it only reports the Play click.</summary>
    public sealed class MainMenuView : MonoBehaviour, IMainMenuView
    {
        [Header("References")]
        [SerializeField] private Button _playButton;

        public event Action PlayClicked;

        private void Awake()
        {
            if (_playButton == null)
            {
                GameLog.Error(nameof(MainMenuView), $"Missing Play button on {name}");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_playButton != null)
                _playButton.onClick.AddListener(HandlePlayButton);
        }

        private void OnDisable()
        {
            if (_playButton != null)
                _playButton.onClick.RemoveListener(HandlePlayButton);
        }

        private void HandlePlayButton()
        {
            PlayClicked?.Invoke();
        }
    }
}
