using System;
using Game.Core.Level;
using NUnit.Framework;

namespace Game.Tests.EditMode.Level
{
    public sealed class PlayerProgressTests
    {
        [Test]
        public void NewProgress_OnlyTheFirstLevelIsUnlocked()
        {
            var progress = new PlayerProgress(5);

            Assert.That(progress.IsUnlocked(0), Is.True);
            Assert.That(progress.IsUnlocked(1), Is.False);
            Assert.That(progress.IsUnlocked(4), Is.False);
            Assert.That(progress.TotalStars, Is.EqualTo(0));
        }

        [Test]
        public void RecordResult_OneStar_UnlocksTheNextLevel()
        {
            var progress = new PlayerProgress(5);

            progress.RecordResult(0, 1);

            Assert.That(progress.GetStars(0), Is.EqualTo(1));
            Assert.That(progress.IsUnlocked(1), Is.True);
            Assert.That(progress.IsUnlocked(2), Is.False);
        }

        [Test]
        public void RecordResult_ZeroStars_ChangesNothing()
        {
            var progress = new PlayerProgress(5);

            var improved = progress.RecordResult(0, 0);

            Assert.That(improved, Is.False);
            Assert.That(progress.IsUnlocked(1), Is.False);
        }

        [Test]
        public void RecordResult_WorseResult_KeepsTheBestStars()
        {
            var progress = new PlayerProgress(5);
            progress.RecordResult(0, 3);

            var improved = progress.RecordResult(0, 2);

            Assert.That(improved, Is.False);
            Assert.That(progress.GetStars(0), Is.EqualTo(3));
        }

        [Test]
        public void RecordResult_BetterResult_ReturnsTrueAndRaisesTheEvent()
        {
            var progress = new PlayerProgress(5);
            progress.RecordResult(0, 1);
            var changedLevel = -1;
            progress.LevelStarsChanged += index => changedLevel = index;

            var improved = progress.RecordResult(0, 2);

            Assert.That(improved, Is.True);
            Assert.That(changedLevel, Is.EqualTo(0));
        }

        [Test]
        public void RecordResult_SameStars_RaisesNoEvent()
        {
            var progress = new PlayerProgress(5);
            progress.RecordResult(0, 2);
            var raised = 0;
            progress.LevelStarsChanged += _ => raised++;

            progress.RecordResult(0, 2);

            Assert.That(raised, Is.EqualTo(0));
        }

        [Test]
        public void TotalStars_AddsUpTheBestOfEveryLevel()
        {
            var progress = new PlayerProgress(5);
            progress.RecordResult(0, 3);
            progress.RecordResult(1, 2);
            progress.RecordResult(1, 1);

            Assert.That(progress.TotalStars, Is.EqualTo(5));
        }

        [Test]
        public void HasNextLevel_LastLevel_IsFalse()
        {
            var progress = new PlayerProgress(3);

            Assert.That(progress.HasNextLevel(1), Is.True);
            Assert.That(progress.HasNextLevel(2), Is.False);
        }

        [Test]
        public void InvalidArguments_Throw()
        {
            var progress = new PlayerProgress(5);

            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerProgress(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => progress.GetStars(5));
            Assert.Throws<ArgumentOutOfRangeException>(() => progress.GetStars(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => progress.IsUnlocked(5));
            Assert.Throws<ArgumentOutOfRangeException>(() => progress.RecordResult(0, 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => progress.RecordResult(0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => progress.RecordResult(5, 1));
        }
    }
}
