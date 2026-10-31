using Game.Core.Level;
using NUnit.Framework;

namespace Game.Tests.EditMode.Level
{
    public sealed class MovesModelTests
    {
        [Test]
        public void Consume_LastMove_RaisesOutOfMoves()
        {
            var model = new MovesModel(1);
            var raised = false;
            model.OutOfMoves += () => raised = true;

            model.Consume();

            Assert.That(model.Remaining, Is.EqualTo(0));
            Assert.That(raised, Is.True);
        }

        [Test]
        public void Consume_AtZero_DoesNotGoNegative()
        {
            var model = new MovesModel(1);
            model.Consume();

            model.Consume();

            Assert.That(model.Remaining, Is.EqualTo(0));
        }

        [Test]
        public void Consume_TwoMovesLeft_RaisesMovesChangedWithNewNumber()
        {
            var model = new MovesModel(2);
            var lastNumber = -1;
            model.MovesChanged += remaining => lastNumber = remaining;

            model.Consume();

            Assert.That(lastNumber, Is.EqualTo(1));
        }

        [Test]
        public void Add_PositiveAmount_IncreasesRemaining()
        {
            var model = new MovesModel(5);

            model.Add(3);

            Assert.That(model.Remaining, Is.EqualTo(8));
        }

        [Test]
        public void Add_ZeroAmount_DoesNothing()
        {
            var model = new MovesModel(5);

            model.Add(0);

            Assert.That(model.Remaining, Is.EqualTo(5));
        }

        [Test]
        public void Add_PositiveAmount_RaisesMovesChangedWithNewNumber()
        {
            var model = new MovesModel(5);
            var lastNumber = -1;
            model.MovesChanged += remaining => lastNumber = remaining;

            model.Add(3);

            Assert.That(lastNumber, Is.EqualTo(8));
        }
    }
}
