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

            Assert.That(config.Moves, Is.EqualTo(20));
            Assert.That(config.TargetScore, Is.EqualTo(1000));

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

                Assert.That(config.Moves, Is.GreaterThan(0), path);
                Assert.That(config.TargetScore, Is.GreaterThan(0), path);
            }
        }
    }
}
