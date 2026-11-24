using System;
using System.Collections.Generic;
using Game.Runtime.Config;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests.EditMode.Config
{
    public sealed class LevelCatalogTests
    {
        private const string CatalogPath = "Assets/_Project/ScriptableObjects/SO_LevelCatalog.asset";

        private static LevelCatalog Load()
        {
            return AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
        }

        [Test]
        public void Catalog_Exists_AndListsAtLeastFiveLevels()
        {
            var catalog = Load();

            Assert.That(catalog, Is.Not.Null, "SO_LevelCatalog is missing");
            Assert.That(catalog.Count, Is.GreaterThanOrEqualTo(5));
        }

        [Test]
        public void Catalog_HasNoEmptySlotsAndNoDuplicates()
        {
            var catalog = Load();
            var seen = new HashSet<LevelConfig>();

            for (var i = 0; i < catalog.Count; i++)
            {
                var level = catalog.Get(i);
                Assert.That(level, Is.Not.Null, $"slot {i} is empty");
                Assert.That(seen.Add(level), Is.True, $"slot {i} ({level.name}) is listed twice");
            }
        }

        // A level asset that is not in the catalog could never be played, so this catches a forgotten new level.
        [Test]
        public void Catalog_ListsEveryLevelAssetInTheProject()
        {
            var catalog = Load();
            var listed = new HashSet<LevelConfig>();
            for (var i = 0; i < catalog.Count; i++)
                listed.Add(catalog.Get(i));

            foreach (var guid in AssetDatabase.FindAssets("t:LevelConfig"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var level = AssetDatabase.LoadAssetAtPath<LevelConfig>(path);
                Assert.That(listed.Contains(level), Is.True, $"{path} is not in the catalog");
            }
        }

        [Test]
        public void Catalog_IsInOrderOfTheLevelNames()
        {
            var catalog = Load();

            for (var i = 1; i < catalog.Count; i++)
            {
                Assert.That(string.CompareOrdinal(catalog.Get(i - 1).name, catalog.Get(i).name), Is.LessThan(0),
                    $"{catalog.Get(i).name} comes after {catalog.Get(i - 1).name} in the catalog but sorts before it");
            }
        }

        [Test]
        public void Get_OutOfRange_Throws()
        {
            var catalog = Load();

            Assert.Throws<ArgumentOutOfRangeException>(() => catalog.Get(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => catalog.Get(catalog.Count));
        }
    }
}
