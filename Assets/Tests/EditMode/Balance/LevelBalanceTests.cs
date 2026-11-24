using Game.Runtime.Config;
using NUnit.Framework;
using UnityEditor;

namespace Game.Tests.EditMode.Balance
{
    // A design guard: two virtual players play each real level on seeded boards, and the win rates must stay inside
    // the designed band. A weak player (any valid swap) should win rarely on later levels; a good player (biggest
    // match) should still win most of the time on early ones. Change a level or the score rules and these tell you
    // whether the level got too easy or too hard. Same seeds every run, so the numbers are stable.
    public sealed class LevelBalanceTests
    {
        private const int Games = 120;

        private static LevelConfig Load(int number)
        {
            return AssetDatabase.LoadAssetAtPath<LevelConfig>($"Assets/_Project/ScriptableObjects/SO_Level_0{number}.asset");
        }

        private static int WinPercent(LevelConfig level, PlayerKind kind, out int stuck)
        {
            var wins = 0;
            stuck = 0;
            for (var seed = 1; seed <= Games; seed++)
            {
                var result = PlaySimulator.Play(level, kind, seed);
                if (result.Won)
                    wins++;

                if (result.Stuck)
                    stuck++;
            }

            return wins * 100 / Games;
        }

        [TestCase(1, 30, 65, 88, 100)]
        [TestCase(2, 20, 52, 80, 100)]
        [TestCase(3, 10, 40, 80, 100)]
        [TestCase(4, 3, 28, 62, 90)]
        [TestCase(5, 0, 16, 38, 66)]
        public void WinRates_StayInsideTheDesignedBand(int number, int randomMin, int randomMax, int greedyMin, int greedyMax)
        {
            var level = Load(number);
            Assert.That(level, Is.Not.Null, $"SO_Level_0{number} is missing");

            var random = WinPercent(level, PlayerKind.Random, out var randomStuck);
            var greedy = WinPercent(level, PlayerKind.Greedy, out var greedyStuck);
            var summary = $"level {number}: random wins {random}% (want {randomMin}-{randomMax}), greedy wins {greedy}% (want {greedyMin}-{greedyMax})";

            Assert.That(random, Is.InRange(randomMin, randomMax), summary);
            Assert.That(greedy, Is.InRange(greedyMin, greedyMax), summary);
            Assert.That(randomStuck + greedyStuck, Is.EqualTo(0), $"level {number} reached a board with no valid swap; the reshuffle feature is needed");
        }

        [Test]
        public void WinRates_GetHarderFromLevelOneToLevelFive_ForBothPlayers()
        {
            var previousRandom = 101;
            var previousGreedy = 101;

            for (var number = 1; number <= 5; number++)
            {
                var level = Load(number);
                var random = WinPercent(level, PlayerKind.Random, out _);
                var greedy = WinPercent(level, PlayerKind.Greedy, out _);

                // A small tolerance (5 points) allows for the noise of 120 games.
                Assert.That(random, Is.LessThanOrEqualTo(previousRandom + 5), $"level {number} is easier than the level before for a random player");
                Assert.That(greedy, Is.LessThanOrEqualTo(previousGreedy + 5), $"level {number} is easier than the level before for a greedy player");
                previousRandom = random;
                previousGreedy = greedy;
            }
        }
    }
}
