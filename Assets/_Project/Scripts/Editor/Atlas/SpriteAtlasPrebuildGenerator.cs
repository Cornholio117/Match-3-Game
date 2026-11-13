// [UNITY-SKILL:SPRITEATLAS]
// [TYPE:PREBUILD]
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.Editor.Atlas
{
    /// <summary>Rebuilds the gem sprite atlas before every build, so it always matches the sprites in the gem folder.</summary>
    public sealed class SpriteAtlasPrebuildGenerator : IPreprocessBuildWithReport
    {
        private const string GemFolder = "Assets/_Project/Art/Gems";
        private const string AtlasPath = "Assets/_Project/Art/Atlases/SPR_Atlas_Gems.spriteatlasv2";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Generate();
        }

        public static void Generate()
        {
            // Packing is Disabled by default: an atlas would still import and look fine, but never pack.
            EditorSettings.spritePackerMode = SpritePackerMode.SpriteAtlasV2;

            // Scoped to the gem folder so package sprites can never end up in the atlas.
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { GemFolder });
            var sprites = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
                .Where(sprite => sprite != null)
                .Cast<UnityEngine.Object>()
                .ToArray();

            if (sprites.Length == 0)
            {
                Debug.LogWarning($"[SpriteAtlas] No sprites found in {GemFolder}");
                return;
            }

            SpriteAtlasAsset atlasAsset;
            if (File.Exists(AtlasPath))
            {
                atlasAsset = SpriteAtlasAsset.Load(AtlasPath);
                var existing = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
                atlasAsset.Remove(existing.GetPackables());
            }
            else
            {
                atlasAsset = new SpriteAtlasAsset();
                Directory.CreateDirectory(Path.GetDirectoryName(AtlasPath));
            }

            atlasAsset.Add(sprites);
            SpriteAtlasAsset.Save(atlasAsset, AtlasPath);
            AssetDatabase.ImportAsset(AtlasPath);

            ConfigureImporter();
            Debug.Log($"[SpriteAtlas] {AtlasPath} now holds {sprites.Length} sprites");
        }

        private static void ConfigureImporter()
        {
            var importer = AssetImporter.GetAtPath(AtlasPath) as SpriteAtlasImporter;
            if (importer == null) return;

            var textureSettings = importer.textureSettings;
            textureSettings.filterMode = FilterMode.Bilinear;
            textureSettings.generateMipMaps = false;
            importer.textureSettings = textureSettings;

            // Padding and alpha dilation stop neighbouring gems bleeding colour into each other.
            var packingSettings = importer.packingSettings;
            packingSettings.padding = 4;
            packingSettings.enableAlphaDilation = true;
            packingSettings.enableRotation = false;
            packingSettings.enableTightPacking = false;
            importer.packingSettings = packingSettings;

            SetPlatformFormat(importer, "Android", TextureImporterFormat.ASTC_6x6);
            SetPlatformFormat(importer, "iPhone", TextureImporterFormat.ASTC_4x4);

            // Gems are needed in every level, so the atlas ships inside the build.
            importer.includeInBuild = true;
            importer.SaveAndReimport();
        }

        private static void SetPlatformFormat(SpriteAtlasImporter importer, string platformName, TextureImporterFormat format)
        {
            var settings = new TextureImporterPlatformSettings
            {
                name = platformName,
                overridden = true,
                maxTextureSize = 2048,
                format = format
            };
            importer.SetPlatformSettings(settings);
        }
    }
}
