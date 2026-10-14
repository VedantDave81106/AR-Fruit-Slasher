using UnityEngine;
using ARFruitSlasher.Procedural;

namespace ARFruitSlasher.Core
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Clips (Optional - procedural fallback if unassigned)")]
        [SerializeField] private AudioClip swishClip;
        [SerializeField] private AudioClip sliceClip;
        [SerializeField] private AudioClip alertClip;
        [SerializeField] private AudioClip explosionClip;
        [SerializeField] private AudioClip checkpointClip;
        [SerializeField] private AudioClip missClip;

        private AudioSource sfxSource;
        private AudioSource alertSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            alertSource = gameObject.AddComponent<AudioSource>();
            alertSource.playOnAwake = false;

            // Generate procedural clips if missing
            if (swishClip == null) swishClip = ProceduralAudioGenerator.CreateSwishClip();
            if (sliceClip == null) sliceClip = ProceduralAudioGenerator.CreateSliceClip();
            if (alertClip == null) alertClip = ProceduralAudioGenerator.CreateAlertClip();
            if (explosionClip == null) explosionClip = ProceduralAudioGenerator.CreateExplosionClip();
            if (checkpointClip == null) checkpointClip = ProceduralAudioGenerator.CreateCheckpointClip();
            if (missClip == null) missClip = ProceduralAudioGenerator.CreateMissClip();
        }

        public void PlaySwish()
        {
            if (swishClip != null)
            {
                sfxSource.pitch = Random.Range(0.9f, 1.15f);
                sfxSource.PlayOneShot(swishClip, 0.6f);
            }
        }

        public void PlaySlice(Vector3 position)
        {
            if (sliceClip != null)
            {
                sfxSource.pitch = Random.Range(0.95f, 1.25f);
                sfxSource.PlayOneShot(sliceClip, 0.9f);
            }
        }

        public void PlayAlert()
        {
            if (alertClip != null && !alertSource.isPlaying)
            {
                alertSource.PlayOneShot(alertClip, 1.0f);
            }
        }

        public void PlayExplosion()
        {
            if (explosionClip != null)
            {
                sfxSource.pitch = 1.0f;
                sfxSource.PlayOneShot(explosionClip, 1.0f);
            }
        }

        public void PlayCheckpoint()
        {
            if (checkpointClip != null)
            {
                sfxSource.pitch = 1.0f;
                sfxSource.PlayOneShot(checkpointClip, 1.0f);
            }
        }

        public void PlayMiss()
        {
            if (missClip != null)
            {
                sfxSource.PlayOneShot(missClip, 0.7f);
            }
        }
    }
}
