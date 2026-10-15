using System;
using UnityEngine;
using ARFruitSlasher.Slicing;

namespace ARFruitSlasher.Spawner
{
    [Serializable]
    public class FruitTypeConfig
    {
        public string name = "Watermelon";
        public string icon = "🍉";
        public ObjectType type = ObjectType.Watermelon;
        public int points = 10;
        public Color outerColor = new Color(0.1f, 0.6f, 0.15f); // Green rind
        public Color fleshColor = new Color(0.95f, 0.15f, 0.2f); // Red flesh
        public float scale = 0.35f;
        public float weight = 1.0f; // Relative spawn probability
    }
}
