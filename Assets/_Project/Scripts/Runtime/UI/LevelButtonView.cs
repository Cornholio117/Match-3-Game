using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Runtime.UI
{
    /// <summary>One level button on the level select screen: its number, its stars, and a lock while it cannot be played.</summary>
    public sealed class LevelButtonView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _numberLabel;

        [Tooltip("Shown while the level is locked.")]
        [SerializeField] private GameObject _lockOverlay;

        [Tooltip("Three star images, left to right.")]
        [SerializeField] private Image[] _stars = new Image[0];

        [Header("Star colours")]
        [SerializeField] private Color _earnedColor = new Color(1f, 0.8f, 0.15f, 1f);
        [SerializeField] private Color _emptyColor = new Color(0.3f, 0.3f, 0.35f, 1f);

        /// <summary>Raised with this button when it is tapped.</summary>
        public event Action<LevelButtonView> Clicked;

        private void Awake()
        {
            if (_button == null || _numberLabel == null)
            {
                GameLog.Error(nameof(LevelButtonView), $"Missing button or number label on {name}");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_button != null)
                _button.onClick.AddListener(HandleButton);
        }

        private void OnDisable()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleButton);
        }

        /// <summary>levelNumber is the number the player sees (counting from 1).</summary>
        public void Show(int levelNumber, int stars, bool unlocked)
        {
            _numberLabel.SetText("{0:0}", levelNumber);

            for (var i = 0; i < _stars.Length; i++)
                _stars[i].color = i < stars ? _earnedColor : _emptyColor;

            if (_lockOverlay != null)
                _lockOverlay.SetActive(!unlocked);
        }

        private void HandleButton()
        {
            Clicked?.Invoke(this);
        }
    }
}
