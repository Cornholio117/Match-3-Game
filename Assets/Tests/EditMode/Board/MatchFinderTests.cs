using System.Collections.Generic;
using Game.Core.Board;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class MatchFinderTests
    {
        private static List<Match> FindAll(BoardModel board)
        {
            var results = new List<Match>();
            MatchFinder.Find(board, results);
            return results;
        }

        private static bool Contains(List<Match> matches, PieceColor color, GridPosition start, Direction direction, int length)
        {
            foreach (var match in matches)
            {
                if (match.Color == color && match.Start == start && match.Direction == direction && match.Length == length)
                    return true;
            }

            return false;
        }

        [Test]
        public void Find_NoRuns_ReturnsNothing()
        {
            var board = BoardBuilder.FromRows(
                "RBRB",
                "BRBR",
                "RBRB");

            Assert.That(FindAll(board), Is.Empty);
        }

        [Test]
        public void Find_RowOfThree_ReturnsOneRightwardMatch()
        {
            var board = BoardBuilder.FromRows(
                "YBYB",
                "GGGR");

            var matches = FindAll(board);

            Assert.That(matches.Count, Is.EqualTo(1));
            Assert.That(Contains(matches, PieceColor.Green, new GridPosition(0, 0), Direction.Right, 3), Is.True);
        }

        [Test]
        public void Find_ColumnOfThree_ReturnsOneUpwardMatch()
        {
            var board = BoardBuilder.FromRows(
                "RBY",
                "RYB",
                "RBY");

            var matches = FindAll(board);

            Assert.That(matches.Count, Is.EqualTo(1));
            Assert.That(Contains(matches, PieceColor.Red, new GridPosition(0, 0), Direction.Up, 3), Is.True);
        }

        [Test]
        public void Find_RowOfFive_ReturnsOneMatchOfLengthFive()
        {
            var board = BoardBuilder.FromRows(
                "BBBBB");

            var matches = FindAll(board);

            Assert.That(matches.Count, Is.EqualTo(1));
            Assert.That(matches[0].Length, Is.EqualTo(5));
        }

        [Test]
        public void Find_RunTouchingTheRightEdge_IsFound()
        {
            var board = BoardBuilder.FromRows(
                "RBYYY");

            Assert.That(Contains(FindAll(board), PieceColor.Yellow, new GridPosition(2, 0), Direction.Right, 3), Is.True);
        }

        [Test]
        public void Find_RunTouchingTheTopEdge_IsFound()
        {
            var board = BoardBuilder.FromRows(
                "G",
                "G",
                "G",
                "R");

            Assert.That(Contains(FindAll(board), PieceColor.Green, new GridPosition(0, 1), Direction.Up, 3), Is.True);
        }

        [Test]
        public void Find_TwoCellsOfOneColour_IsNotARun()
        {
            var board = BoardBuilder.FromRows(
                "GGRG");

            Assert.That(FindAll(board), Is.Empty);
        }

        [Test]
        public void Find_PictureOne_ReturnsTheBlueRowAndTheRedRow()
        {
            var board = BoardBuilder.FromRows(
                "RBBBR",
                "GGRGG",
                "BRRRB");

            var matches = FindAll(board);

            Assert.That(matches.Count, Is.EqualTo(2));
            Assert.That(Contains(matches, PieceColor.Blue, new GridPosition(1, 2), Direction.Right, 3), Is.True);
            Assert.That(Contains(matches, PieceColor.Red, new GridPosition(1, 0), Direction.Right, 3), Is.True);
        }

        [Test]
        public void Find_LShape_ReturnsTwoRunsThatShareOneCell()
        {
            var board = BoardBuilder.FromRows(
                "GRYRY",
                "GYRYR",
                "GGGRY",
                "RYRYR");

            var matches = FindAll(board);
            var cells = new HashSet<GridPosition>();
            foreach (var match in matches)
            {
                for (var i = 0; i < match.Length; i++)
                    cells.Add(match.GetPosition(i));
            }

            Assert.That(matches.Count, Is.EqualTo(2));
            Assert.That(cells.Count, Is.EqualTo(5));
        }

        [Test]
        public void Find_EmptyCells_AreNeverARun()
        {
            var board = BoardBuilder.FromRows(
                "...",
                "...");

            Assert.That(FindAll(board), Is.Empty);
        }

        [Test]
        public void Find_ReusedList_IsClearedFirst()
        {
            var results = new List<Match>();
            MatchFinder.Find(BoardBuilder.FromRows("GGG"), results);

            MatchFinder.Find(BoardBuilder.FromRows("GBG"), results);

            Assert.That(results, Is.Empty);
        }

        [TestCase(1, 0, true)]
        [TestCase(0, 0, true)]
        [TestCase(0, 1, false)]
        public void IsPartOfMatch_RowOfThreeOverMixedRow_OnlyTheRunCellsCount(int x, int y, bool expected)
        {
            var board = BoardBuilder.FromRows(
                "RBY",
                "GGG");

            Assert.That(MatchFinder.IsPartOfMatch(board, new GridPosition(x, y)), Is.EqualTo(expected));
        }
    }
}
