using System;
using Game.Core.UI;
using NUnit.Framework;

namespace Game.Tests.EditMode.UI
{
    public sealed class ScreenNavigatorTests
    {
        [Test]
        public void NewNavigator_StartsOnTheGivenScreenWithNoHistory()
        {
            var navigator = new ScreenNavigator(AppScreen.MainMenu);

            Assert.That(navigator.Current, Is.EqualTo(AppScreen.MainMenu));
            Assert.That(navigator.Depth, Is.EqualTo(0));
        }

        [Test]
        public void Go_NewScreen_BecomesCurrentAndRemembersWhereYouCameFrom()
        {
            var navigator = new ScreenNavigator(AppScreen.MainMenu);

            navigator.Go(AppScreen.LevelSelect);

            Assert.That(navigator.Current, Is.EqualTo(AppScreen.LevelSelect));
            Assert.That(navigator.Depth, Is.EqualTo(1));
        }

        [Test]
        public void Back_AfterTwoSteps_WalksBackOneScreenAtATime()
        {
            var navigator = new ScreenNavigator(AppScreen.MainMenu);
            navigator.Go(AppScreen.LevelSelect);
            navigator.Go(AppScreen.Gameplay);

            Assert.That(navigator.Back(), Is.True);
            Assert.That(navigator.Current, Is.EqualTo(AppScreen.LevelSelect));
            Assert.That(navigator.Back(), Is.True);
            Assert.That(navigator.Current, Is.EqualTo(AppScreen.MainMenu));
        }

        [Test]
        public void Back_OnTheFirstScreen_ReturnsFalseAndStaysPut()
        {
            var navigator = new ScreenNavigator(AppScreen.MainMenu);

            Assert.That(navigator.Back(), Is.False);
            Assert.That(navigator.Current, Is.EqualTo(AppScreen.MainMenu));
        }

        [Test]
        public void Go_ToTheCurrentScreen_DoesNothing()
        {
            var navigator = new ScreenNavigator(AppScreen.MainMenu);
            var raised = 0;
            navigator.ScreenChanged += (_, _) => raised++;

            navigator.Go(AppScreen.MainMenu);

            Assert.That(raised, Is.EqualTo(0));
            Assert.That(navigator.Depth, Is.EqualTo(0));
        }

        [Test]
        public void Go_ToAScreenAlreadyInTheHistory_UnwindsInsteadOfStackingCopies()
        {
            // After a win the player taps "Level Select": the stack must not grow, it goes back to that screen.
            var navigator = new ScreenNavigator(AppScreen.MainMenu);
            navigator.Go(AppScreen.LevelSelect);
            navigator.Go(AppScreen.Gameplay);

            navigator.Go(AppScreen.LevelSelect);

            Assert.That(navigator.Current, Is.EqualTo(AppScreen.LevelSelect));
            Assert.That(navigator.Depth, Is.EqualTo(1));
            Assert.That(navigator.Back(), Is.True);
            Assert.That(navigator.Current, Is.EqualTo(AppScreen.MainMenu));
        }

        [Test]
        public void ScreenChanged_Go_ReportsFromAndTo()
        {
            var navigator = new ScreenNavigator(AppScreen.MainMenu);
            var from = AppScreen.None;
            var to = AppScreen.None;
            navigator.ScreenChanged += (a, b) =>
            {
                from = a;
                to = b;
            };

            navigator.Go(AppScreen.LevelSelect);

            Assert.That(from, Is.EqualTo(AppScreen.MainMenu));
            Assert.That(to, Is.EqualTo(AppScreen.LevelSelect));
        }

        [Test]
        public void ScreenChanged_Back_ReportsFromAndTo()
        {
            var navigator = new ScreenNavigator(AppScreen.MainMenu);
            navigator.Go(AppScreen.LevelSelect);
            var from = AppScreen.None;
            var to = AppScreen.None;
            navigator.ScreenChanged += (a, b) =>
            {
                from = a;
                to = b;
            };

            navigator.Back();

            Assert.That(from, Is.EqualTo(AppScreen.LevelSelect));
            Assert.That(to, Is.EqualTo(AppScreen.MainMenu));
        }

        [Test]
        public void NoneIsNotAScreen_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ScreenNavigator(AppScreen.None));
            Assert.Throws<ArgumentException>(() => new ScreenNavigator(AppScreen.MainMenu).Go(AppScreen.None));
        }
    }
}
