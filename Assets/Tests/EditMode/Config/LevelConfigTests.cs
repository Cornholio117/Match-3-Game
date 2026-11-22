using Game.Core.Board;
using Game.Core.Level;
using Game.Runtime.Config;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests.EditMode.Config
{
    public sealed class LevelConfigTests
    {
        [Test]
        public void CreateInstance_NewConfig_HasDefaultValues()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();

            Assert.That(config.Width, Is.EqualTo(8));
            Assert.That(config.Height, Is.EqualTo(8));
            Assert.That(config.ColorCount, Is.EqualTo(5));
            Assert.That(config.Moves, Is.EqualTo(20));
            Assert.That(config.GoalType, Is.EqualTo(LevelGoalType.Score));
            Assert.That(config.GoalAmount, Is.EqualTo(1000));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void MovesModel_BuiltFromConfig_StartsWithConfiguredMoves()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();

            var model = new MovesModel(config.Moves);

            Assert.That(model.Remaining, Is.EqualTo(config.Moves));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void BoardGame_BuiltFromConfig_UsesTheConfiguredSizeAndColours()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();

            var game = BoardGame.CreateFilled(config.Width, config.Height, config.ColorCount, new Game.Core.Common.SeededRandom(1));

            Assert.That(game.Board.Width, Is.EqualTo(config.Width));
            Assert.That(game.Board.Height, Is.EqualTo(config.Height));

            Object.DestroyImmediate(config);
        }

        // Data validation: catches a bad value in any level asset, with the asset path in the message.
        [Test]
        public void AllLevelConfigs_HaveValidValues()
        {
            var guids = AssetDatabase.FindAssets("t:LevelConfig");
            Assert.That(guids.Length, Is.GreaterThan(0), "No LevelConfig assets found");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<LevelConfig>(path);

                Assert.That(config.Width, Is.GreaterThanOrEqualTo(3), path);
                Assert.That(config.Height, Is.GreaterThanOrEqualTo(3), path);
                Assert.That(config.ColorCount, Is.InRange(BoardModel.MinColorCount, BoardModel.MaxColorCount), path);
                Assert.That(config.Moves, Is.GreaterThan(0), path);
                Assert.That(config.GoalAmount, Is.GreaterThan(0), path);

                if (config.GoalType == LevelGoalType.CollectColor)
                {
                    Assert.That(config.GoalColor, Is.Not.EqualTo(PieceColor.None), path);
                    Assert.That((int)config.GoalColor, Is.LessThanOrEqualTo(config.ColorCount), path + " collects a colour the level never spawns");
                }
            }
        }

        [Test]
        public void AllLevelConfigs_AtLeastFiveLevelsExist()
        {
            var guids = AssetDatabase.FindAssets("t:LevelConfig");

            Assert.That(guids.Length, Is.GreaterThanOrEqualTo(5));
        }
    }
}
