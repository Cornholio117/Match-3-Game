using System;
using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Level;
using NUnit.Framework;

namespace Game.Tests.EditMode.Level
{
    public sealed class ScoreRulesTests
    {
        private static void AddClears(List<BoardStep> steps, int count)
        {
            for (var x = 0; x < count; x++)
                steps.Add(BoardStep.Clear(PieceColor.Green, new GridPosition(x, 0)));
        }

        private static void AddRefill(List<BoardStep> steps, int count)
        {
            for (var x = 0; x < count; x++)
                steps.Add(BoardStep.Spawn(PieceColor.Red, new GridPosition(x, 2)));
        }

        [Test]
        public void Calculate_NoSteps_ReturnsZero()
        {
            var rules = new ScoreRules();

            Assert.That(rules.Calculate(new List<BoardStep>()), Is.EqualTo(0));
        }

        [Test]
        public void Calculate_OneWaveOfThreeGems_ReturnsThirtyWithDefaults()
        {
            var steps = new List<BoardStep>();
            AddClears(steps, 3);
            AddRefill(steps, 3);

            Assert.That(new ScoreRules().Calculate(steps), Is.EqualTo(30));
        }

        [Test]
        public void Calculate_SecondWave_CountsDoubleAndThirdTriple()
        {
            var steps = new List<BoardStep>();
            AddClears(steps, 3);
            AddRefill(steps, 3);
            AddClears(steps, 3);
            AddRefill(steps, 3);
            AddClears(steps, 3);
            AddRefill(steps, 3);

            // 3 gems x 10 x 1, then x 2, then x 3.
            Assert.That(new ScoreRules().Calculate(steps), Is.EqualTo(30 + 60 + 90));
        }

        [Test]
        public void Calculate_SpecialGemCreated_AddsTheBonus()
        {
            var steps = new List<BoardStep>();
            AddClears(steps, 3);
            steps.Add(BoardStep.Create(PieceColor.Green, PieceKind.LineHorizontal, new GridPosition(3, 0)));
            AddRefill(steps, 3);

            // 3 gems x 10, plus 50 for the new special gem.
            Assert.That(new ScoreRules().Calculate(steps), Is.EqualTo(80));
        }

        [Test]
        public void Calculate_FallsAndSpawnsAlone_ScoreNothing()
        {
            var steps = new List<BoardStep>
            {
                BoardStep.Fall(PieceColor.Blue, new GridPosition(0, 1), new GridPosition(0, 0)),
                BoardStep.Spawn(PieceColor.Red, new GridPosition(0, 1))
            };

            Assert.That(new ScoreRules().Calculate(steps), Is.EqualTo(0));
        }

        [Test]
        public void Calculate_CustomValues_AreUsed()
        {
            var steps = new List<BoardStep>();
            AddClears(steps, 3);

            Assert.That(new ScoreRules(pointsPerGem: 5, specialBonus: 0).Calculate(steps), Is.EqualTo(15));
        }

        [Test]
        public void Constructor_PointsPerGemNotPositive_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScoreRules(0, 50));
        }

        [Test]
        public void Constructor_NegativeSpecialBonus_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScoreRules(10, -1));
        }
    }
}
