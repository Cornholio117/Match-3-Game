using System;
using System.Collections.Generic;

namespace Game.Core.UI
{
    /// <summary>
    /// Which popups are open, in the order they were opened; the last one is on top. Pure logic: the scene side
    /// listens to Opened and Closed and shows or hides the matching view.
    /// </summary>
    public sealed class PopupStack
    {
        private readonly List<PopupId> _open = new List<PopupId>();

        public event Action<PopupId> Opened;
        public event Action<PopupId> Closed;

        public int Count => _open.Count;

        /// <summary>The popup on top, or None when nothing is open.</summary>
        public PopupId Top => _open.Count == 0 ? PopupId.None : _open[_open.Count - 1];

        public bool IsOpen(PopupId id)
        {
            return _open.Contains(id);
        }

        /// <summary>Opens a popup on top. Returns false, changing nothing, when it is already open.</summary>
        public bool Open(PopupId id)
        {
            if (id == PopupId.None)
                throw new ArgumentException("None is not a popup.", nameof(id));

            if (_open.Contains(id))
                return false;

            _open.Add(id);
            Opened?.Invoke(id);
            return true;
        }

        /// <summary>Closes the popup on top. Returns false when nothing is open.</summary>
        public bool CloseTop()
        {
            if (_open.Count == 0)
                return false;

            return Close(_open[_open.Count - 1]);
        }

        /// <summary>
        /// What the Android back button does: closes the top popup, except the win and lose popups, which the player
        /// must answer with their own buttons. Returns false when nothing was closed.
        /// </summary>
        public bool CloseTopOnBack()
        {
            var top = Top;
            if (top == PopupId.None || top == PopupId.Win || top == PopupId.Lose)
                return false;

            return Close(top);
        }

        /// <summary>Closes one specific popup, wherever it is in the stack. Returns false when it was not open.</summary>
        public bool Close(PopupId id)
        {
            if (!_open.Remove(id))
                return false;

            Closed?.Invoke(id);
            return true;
        }
    }
}
