using UnityEngine;
using ARFruitSlasher.AR;
using ARFruitSlasher.Core;
using ARFruitSlasher.Slicing;
using ARFruitSlasher.Spawner;
using ARFruitSlasher.UI;

namespace ARFruitSlasher.Core
{
    /// <summary>
    /// Ensures that all managers and systems are initialized even if starting in an empty test scene
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            EnsureManager<GameManager>("GameManager");
            EnsureManager<DifficultyManager>("DifficultyManager");
            EnsureManager<SoundManager>("SoundManager");
            EnsureManager<ARPlacementManager>("ARPlacementManager");
            EnsureManager<ARWarningIndicator>("ARWarningIndicator");
            EnsureManager<FruitSpawner>("FruitSpawner");
            EnsureManager<ARFaceScanManager>("ARFaceScanManager");

            // Ensure BladeController is on Camera or blade object
            Camera cam = Camera.main;
            if (cam != null && cam.GetComponent<BladeController>() == null)
            {
                cam.gameObject.AddComponent<BladeController>();
            }

            // Ensure UI exists
            if (FindObjectOfType<UIManager>() == null)
            {
                GameObject uiGo = new GameObject("UICanvas");
                uiGo.AddComponent<UIManager>();
                uiGo.AddComponent<RearAlertUI>();
            }
        }

        private static T EnsureManager<T>(string name) where T : Component
        {
            T existing = FindObjectOfType<T>();
            if (existing == null)
            {
                GameObject go = new GameObject(name);
                existing = go.AddComponent<T>();
            }
            return existing;
        }
    }
}
