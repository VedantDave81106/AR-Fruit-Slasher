using UnityEngine;

namespace ARFruitSlasher.UI
{
    public class FloatingScore : MonoBehaviour
    {
        private TextMesh textMesh;
        private float lifetime = 0.85f;
        private float timer = 0f;
        private Vector3 startPos;
        private Camera mainCamera;

        public void Initialize(string text, Color color)
        {
            textMesh = gameObject.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = 48;
            textMesh.characterSize = 0.05f;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.color = color;

            startPos = transform.position;
            mainCamera = Camera.main;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float progress = timer / lifetime;

            if (mainCamera != null)
            {
                // Face the AR camera
                transform.forward = mainCamera.transform.forward;
            }

            // Float upward smoothly
            transform.position = startPos + Vector3.up * (progress * 0.5f);

            // Scale and fade
            float scale = Mathf.Sin(progress * Mathf.PI) * 1.2f;
            transform.localScale = Vector3.one * Mathf.Max(scale, 0.01f);

            if (textMesh != null)
            {
                Color c = textMesh.color;
                c.a = 1f - progress;
                textMesh.color = c;
            }

            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
