using Game.Core.UI;
using UnityEngine;

namespace Game.Runtime.UI
{
    /// <summary>
    /// The scene side of one full-screen page: it knows which screen it is and switches its objects on when the screen
    /// shows and off when it hides. Switching objects off (not just covering them) means a hidden board or HUD costs
    /// nothing to draw or to rebuild. It holds no rules.
    /// </summary>
    public sealed class ScreenView : MonoBehaviour
    {
        [Tooltip("Which screen this is.")]
        [SerializeField] private AppScreen _screen = AppScreen.MainMenu;

        [Tooltip("Everything that belongs to this screen: switched on when it shows, off when it hides.")]
        [SerializeField] private GameObject[] _roots = new GameObject[0];

        public AppScreen Screen => _screen;

        public void Show()
        {
            SetRootsActive(true);
        }

        public void Hide()
        {
            SetRootsActive(false);
        }

        private void SetRootsActive(bool active)
        {
            for (var i = 0; i < _roots.Length; i++)
            {
                if (_roots[i] != null)
                    _roots[i].SetActive(active);
            }
        }
    }
}
