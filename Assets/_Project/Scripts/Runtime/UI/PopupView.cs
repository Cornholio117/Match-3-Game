using Game.Core.UI;
using UnityEngine;

namespace Game.Runtime.UI
{
    /// <summary>
    /// The scene side of one popup: it knows which popup it is and can be shown or hidden. It holds no rules.
    /// Designed as a base for popups that add their own buttons (one level of inheritance, R1.4).
    /// </summary>
    public class PopupView : MonoBehaviour
    {
        [Tooltip("Which popup this is. Must be unique in the scene.")]
        [SerializeField] private PopupId _id = PopupId.Pause;

        public PopupId Id => _id;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
