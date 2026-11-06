using System.Collections.Generic;
using NUnit.Framework;

namespace Game.Tests.EditMode.Study
{
    // Study sample (week 2, Monday): Command pattern. An action becomes an object that knows
    // how to do itself and how to undo itself, and a Stack keeps the history.
    public sealed class ScoreModel
    {
        public int Value { get; private set; }

        public void Add(int points)
        {
            Value += points;
        }
    }

    public sealed class AddScoreCommand
    {
        private readonly ScoreModel _score;
        private readonly int _points;

        public AddScoreCommand(ScoreModel score, int points)
        {
            _score = score;
            _points = points;
        }

        public void Execute()
        {
            _score.Add(_points);
        }

        public void Undo()
        {
            _score.Add(-_points);
        }
    }

    public sealed class CommandTests
    {
        [Test]
        public void Execute_AddsPointsToScore()
        {
            var score = new ScoreModel();

            new AddScoreCommand(score, 10).Execute();

            Assert.That(score.Value, Is.EqualTo(10));
        }

        [Test]
        public void Undo_AfterExecute_RestoresScore()
        {
            var score = new ScoreModel();
            var command = new AddScoreCommand(score, 10);
            command.Execute();

            command.Undo();

            Assert.That(score.Value, Is.EqualTo(0));
        }

        [Test]
        public void History_TwoCommandsUndoOnce_UndoesNewestFirst()
        {
            var score = new ScoreModel();
            var history = new Stack<AddScoreCommand>();

            var first = new AddScoreCommand(score, 10);
            first.Execute();
            history.Push(first);

            var second = new AddScoreCommand(score, 5);
            second.Execute();
            history.Push(second);

            history.Pop().Undo();

            Assert.That(score.Value, Is.EqualTo(10));
        }
    }
}
