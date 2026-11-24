using System;

namespace Game.Core.UI
{
    /// <summary>What the menu screens can ask the app to do.</summary>
    public interface IMenuFlow
    {
        void OpenLevelSelect();

        /// <summary>Starts the level with this index and shows the gameplay screen.</summary>
        void PlayLevel(int levelIndex);

        /// <summary>Goes back one screen.</summary>
        void GoBack();
    }

    /// <summary>What the presenter needs from the main menu screen: the Play button.</summary>
    public interface IMainMenuView
    {
        event Action PlayClicked;
    }

    /// <summary>Decides what the main menu does: Play leads to the level select screen.</summary>
    public sealed class MainMenuPresenter
    {
        private readonly IMainMenuView _view;
        private readonly IMenuFlow _flow;
        private bool _enabled;

        public MainMenuPresenter(IMainMenuView view, IMenuFlow flow)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        }

        public void Enable()
        {
            if (_enabled)
                return;

            _enabled = true;
            _view.PlayClicked += HandlePlayClicked;
        }

        public void Disable()
        {
            if (!_enabled)
                return;

            _enabled = false;
            _view.PlayClicked -= HandlePlayClicked;
        }

        private void HandlePlayClicked()
        {
            _flow.OpenLevelSelect();
        }
    }
}
