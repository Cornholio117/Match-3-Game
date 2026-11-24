using System;
using Game.Core.UI;
using NUnit.Framework;

namespace Game.Tests.EditMode.UI
{
    public sealed class MainMenuPresenterTests
    {
        private sealed class FakeMainMenuView : IMainMenuView
        {
            public event Action PlayClicked;

            public void PressPlay()
            {
                PlayClicked?.Invoke();
            }
        }

        private sealed class FakeMenuFlow : IMenuFlow
        {
            public int OpenLevelSelectCalls;
            public int PlayedLevel = -1;
            public int GoBackCalls;

            public void OpenLevelSelect()
            {
                OpenLevelSelectCalls++;
            }

            public void PlayLevel(int levelIndex)
            {
                PlayedLevel = levelIndex;
            }

            public void GoBack()
            {
                GoBackCalls++;
            }
        }

        [Test]
        public void PlayClicked_OpensTheLevelSelectScreenOnce()
        {
            var view = new FakeMainMenuView();
            var flow = new FakeMenuFlow();
            new MainMenuPresenter(view, flow).Enable();

            view.PressPlay();

            Assert.That(flow.OpenLevelSelectCalls, Is.EqualTo(1));
        }

        [Test]
        public void PlayClicked_AfterDisable_DoesNothing()
        {
            var view = new FakeMainMenuView();
            var flow = new FakeMenuFlow();
            var presenter = new MainMenuPresenter(view, flow);
            presenter.Enable();
            presenter.Disable();

            view.PressPlay();

            Assert.That(flow.OpenLevelSelectCalls, Is.EqualTo(0));
        }

        [Test]
        public void Enable_CalledTwice_StillReactsOnce()
        {
            var view = new FakeMainMenuView();
            var flow = new FakeMenuFlow();
            var presenter = new MainMenuPresenter(view, flow);
            presenter.Enable();
            presenter.Enable();

            view.PressPlay();

            Assert.That(flow.OpenLevelSelectCalls, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new MainMenuPresenter(null, new FakeMenuFlow()));
            Assert.Throws<ArgumentNullException>(() => new MainMenuPresenter(new FakeMainMenuView(), null));
        }
    }
}
