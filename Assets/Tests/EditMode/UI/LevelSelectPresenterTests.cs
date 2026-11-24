using System;
using Game.Core.Level;
using Game.Core.UI;
using NUnit.Framework;

namespace Game.Tests.EditMode.UI
{
    public sealed class LevelSelectPresenterTests
    {
        private sealed class FakeLevelSelectView : ILevelSelectView
        {
            public event Action<int> LevelClicked;
            public event Action BackClicked;
            public readonly int[] Stars = { -1, -1, -1, -1, -1 };
            public readonly bool[] Unlocked = new bool[5];
            public int ShowCalls;

            public void ShowLevel(int levelIndex, int stars, bool unlocked)
            {
                Stars[levelIndex] = stars;
                Unlocked[levelIndex] = unlocked;
                ShowCalls++;
            }

            public void PressLevel(int levelIndex)
            {
                LevelClicked?.Invoke(levelIndex);
            }

            public void PressBack()
            {
                BackClicked?.Invoke();
            }
        }

        private sealed class FakeMenuFlow : IMenuFlow
        {
            public int PlayCalls;
            public int PlayedLevel = -1;
            public int GoBackCalls;

            public void OpenLevelSelect()
            {
            }

            public void PlayLevel(int levelIndex)
            {
                PlayCalls++;
                PlayedLevel = levelIndex;
            }

            public void GoBack()
            {
                GoBackCalls++;
            }
        }

        [Test]
        public void Enable_ShowsEveryLevelWithItsStarsAndLock()
        {
            var player = new PlayerProgress(5);
            player.RecordResult(0, 2);
            var view = new FakeLevelSelectView();

            new LevelSelectPresenter(player, view, new FakeMenuFlow()).Enable();

            Assert.That(view.ShowCalls, Is.EqualTo(5));
            Assert.That(view.Stars[0], Is.EqualTo(2));
            Assert.That(view.Unlocked[0], Is.True);
            Assert.That(view.Unlocked[1], Is.True, "one star on level 1 unlocks level 2");
            Assert.That(view.Unlocked[2], Is.False);
            Assert.That(view.Unlocked[4], Is.False);
        }

        [Test]
        public void StarsChanged_RefreshesTheButtonsAndUnlocksTheNextLevel()
        {
            var player = new PlayerProgress(5);
            var view = new FakeLevelSelectView();
            new LevelSelectPresenter(player, view, new FakeMenuFlow()).Enable();
            Assert.That(view.Unlocked[1], Is.False);

            player.RecordResult(0, 3);

            Assert.That(view.Stars[0], Is.EqualTo(3));
            Assert.That(view.Unlocked[1], Is.True);
        }

        [Test]
        public void LevelClicked_UnlockedLevel_PlaysThatLevel()
        {
            var player = new PlayerProgress(5);
            player.RecordResult(0, 1);
            var view = new FakeLevelSelectView();
            var flow = new FakeMenuFlow();
            new LevelSelectPresenter(player, view, flow).Enable();

            view.PressLevel(1);

            Assert.That(flow.PlayCalls, Is.EqualTo(1));
            Assert.That(flow.PlayedLevel, Is.EqualTo(1));
        }

        [Test]
        public void LevelClicked_LockedLevel_DoesNothing()
        {
            var player = new PlayerProgress(5);
            var view = new FakeLevelSelectView();
            var flow = new FakeMenuFlow();
            new LevelSelectPresenter(player, view, flow).Enable();

            view.PressLevel(3);

            Assert.That(flow.PlayCalls, Is.EqualTo(0));
        }

        [TestCase(-1)]
        [TestCase(5)]
        [TestCase(99)]
        public void LevelClicked_IndexOutsideTheLevels_DoesNothingAndDoesNotThrow(int index)
        {
            var player = new PlayerProgress(5);
            var view = new FakeLevelSelectView();
            var flow = new FakeMenuFlow();
            new LevelSelectPresenter(player, view, flow).Enable();

            Assert.DoesNotThrow(() => view.PressLevel(index));
            Assert.That(flow.PlayCalls, Is.EqualTo(0));
        }

        [Test]
        public void BackClicked_GoesBack()
        {
            var view = new FakeLevelSelectView();
            var flow = new FakeMenuFlow();
            new LevelSelectPresenter(new PlayerProgress(5), view, flow).Enable();

            view.PressBack();

            Assert.That(flow.GoBackCalls, Is.EqualTo(1));
        }

        [Test]
        public void Disable_StopsClicksAndRefreshes()
        {
            var player = new PlayerProgress(5);
            var view = new FakeLevelSelectView();
            var flow = new FakeMenuFlow();
            var presenter = new LevelSelectPresenter(player, view, flow);
            presenter.Enable();
            presenter.Disable();
            var callsBefore = view.ShowCalls;

            player.RecordResult(0, 1);
            view.PressLevel(0);
            view.PressBack();

            Assert.That(view.ShowCalls, Is.EqualTo(callsBefore));
            Assert.That(flow.PlayCalls, Is.EqualTo(0));
            Assert.That(flow.GoBackCalls, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var player = new PlayerProgress(5);
            var view = new FakeLevelSelectView();
            var flow = new FakeMenuFlow();

            Assert.Throws<ArgumentNullException>(() => new LevelSelectPresenter(null, view, flow));
            Assert.Throws<ArgumentNullException>(() => new LevelSelectPresenter(player, null, flow));
            Assert.Throws<ArgumentNullException>(() => new LevelSelectPresenter(player, view, null));
        }
    }
}
