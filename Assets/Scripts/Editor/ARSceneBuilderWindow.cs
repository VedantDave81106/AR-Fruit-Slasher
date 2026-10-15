#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ARFruitSlasher.AR;
using ARFruitSlasher.Core;
using ARFruitSlasher.Slicing;
using ARFruitSlasher.Spawner;
using ARFruitSlasher.UI;

namespace ARFruitSlasher.Editor
{
    public class ARSceneBuilderWindow : EditorWindow
    {
        [MenuItem("Tools/AR Fruit Slasher/Setup Complete AR Scene", false, 0)]
        public static void SetupScene()
        {
            if (!EditorUtility.DisplayDialog("Setup AR Fruit Slasher Scene",
                "This will generate and configure all game systems, camera, blade controller, UI canvas, and spawner into the current scene.\n\nProceed?",
                "Yes, Setup Scene", "Cancel"))
            {
                return;
            }

            // Create or verify Directional Light
            Light sun = Object.FindObjectOfType<Light>();
            if (sun == null)
            {
                GameObject lightGO = new GameObject("Directional Light");
                sun = lightGO.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.intensity = 1.2f;
                sun.color = new Color(1.0f, 0.95f, 0.88f);
                lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            // Camera & Blade
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camGO = new GameObject("Main Camera");
                cam = camGO.AddComponent<Camera>();
                camGO.tag = "MainCamera";
                camGO.AddComponent<AudioListener>();
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.15f, 1f); // Dark AR passthrough fallback
            cam.transform.position = new Vector3(0f, 1.4f, 0f);
            cam.transform.rotation = Quaternion.identity;

            if (cam.GetComponent<BladeController>() == null)
            {
                cam.gameObject.AddComponent<BladeController>();
            }

            // Game Managers Root
            GameObject managersRoot = GameObject.Find("--- GAME MANAGERS ---");
            if (managersRoot == null)
            {
                managersRoot = new GameObject("--- GAME MANAGERS ---");
            }

            EnsureComponent<GameBootstrap>(managersRoot);
            EnsureComponent<GameManager>(managersRoot);
            EnsureComponent<DifficultyManager>(managersRoot);
            EnsureComponent<SoundManager>(managersRoot);
            EnsureComponent<FruitSpawner>(managersRoot);
            EnsureComponent<ARPlacementManager>(managersRoot);
            EnsureComponent<ARWarningIndicator>(managersRoot);
            EnsureComponent<ARFaceScanManager>(managersRoot);
            EnsureComponent<ARCameraHandTracker>(managersRoot);

            // UI Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("UI_Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);

                canvasGO.AddComponent<GraphicRaycaster>();
                canvasGO.AddComponent<UIManager>();
                canvasGO.AddComponent<RearAlertUI>();
            }

            // Save Scene
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("<color=green><b>[AR Fruit Slasher]</b> Scene successfully built and configured!</color>");
            EditorUtility.DisplayDialog("Setup Complete!",
                "All AR Fruit Slasher systems have been generated!\n\n• GameManager & Checkpoint Scaling (100° -> 180° -> 270° -> 360°)\n• Blade Controller with touch/mouse swipe\n• Rear Threat Warning Indicator\n• Spawner with procedural fruit halves & sound effects\n• Complete UI HUD\n\nYou can press PLAY right now in the Editor to test it with your mouse, or build it to Android ARCore!",
                "Awesome!");
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            T comp = target.GetComponent<T>();
            if (comp == null)
            {
                comp = target.AddComponent<T>();
            }
            return comp;
        }
    }
}
#endif
