using NUnit.Framework;

namespace Game.Tests.EditMode.Study
{
    // Study exercise: an interface is a promise of "I have these methods".
    // The promise: "I can give you a number below max."
    public interface IDice
    {
        int Roll(int maxExclusive);
    }

    // A fake dice for tests: always gives the same number.
    public sealed class FixedDice : IDice
    {
        private readonly int _value;

        public FixedDice(int value)
        {
            _value = value;
        }

        public int Roll(int maxExclusive) => _value;
    }

    // Game code: it only knows IDice, not which dice it got.
    public sealed class Player
    {
        private readonly IDice _dice;

        public Player(IDice dice)
        {
            _dice = dice;
        }

        public int TakeTurn() => _dice.Roll(6) + 1;  // a die shows 1 to 6
    }

    public sealed class InterfaceTests
    {
        [Test]
        public void TakeTurn_DiceAlwaysGivesThree_ReturnsFour()
        {
            var player = new Player(new FixedDice(3));

            var result = player.TakeTurn();

            Assert.That(result, Is.EqualTo(4));
        }

        [Test]
        public void TakeTurn_DiceGivesZero_ReturnsOne()
        {
            var player = new Player(new FixedDice(0));

            Assert.That(player.TakeTurn(), Is.EqualTo(1));
        }
    }
}
