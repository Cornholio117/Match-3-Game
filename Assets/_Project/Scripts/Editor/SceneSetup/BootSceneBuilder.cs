using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Game.Editor.SceneSetup
{
    /// <summary>Creates the Boot scene with the mandatory hierarchy from RULES.md R4.</summary>
    public static class BootSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Boot.unity";
        private const string SeparatorTag = "EditorOnly";

        [MenuItem("Tools/Match3/Create Boot Scene")]
        public static void CreateBootScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                // Never overwrite a scene silently; the user decides whether to delete it.
                Debug.LogError($"[{nameof(BootSceneBuilder)}] {ScenePath} already exists. Delete or rename it first.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Separator("General");
            CreateEmpty("Bootstrap");

            Separator("Camera");
            CreateCamera();

            Separator("Board");
            CreateEmpty("BoardView");
            CreateEmpty("PiecePool");

            Separator("VFX");
            CreateEmpty("VfxPool");

            Separator("Audio");
            CreateMusicSource();
            CreateEmpty("SfxPool");

            Separator("UI");
            CreateCanvas("HUD Canvas", raycaster: true);
            CreateCanvas("Popup Canvas", raycaster: true);
            CreateCanvas("Static Canvas", raycaster: false);
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();
            Debug.Log($"[{nameof(BootSceneBuilder)}] Created {ScenePath}");
        }

        // Separators carry no components and use EditorOnly so Unity strips them from builds (R4.1).
        private static void Separator(string section)
        {
            var go = new GameObject($"=========={section}==========") { tag = SeparatorTag };
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private static GameObject CreateEmpty(string objectName) => new GameObject(objectName);

        private static void CreateCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            go.transform.position = new Vector3(0f, 0f, -10f);
            go.AddComponent<AudioListener>();
        }

        private static void CreateMusicSource()
        {
            var go = new GameObject("MusicSource");
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
        }

        private static void CreateCanvas(string canvasName, bool raycaster)
        {
            var go = new GameObject(canvasName);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Static canvas holds no interactive elements, so skip the raycaster (R5.6).
            if (raycaster)
                go.AddComponent<GraphicRaycaster>();
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        private static void AddToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == ScenePath)
                    return;
            }

            var updated = new EditorBuildSettingsScene[scenes.Length + 1];
            scenes.CopyTo(updated, 0);
            updated[scenes.Length] = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
