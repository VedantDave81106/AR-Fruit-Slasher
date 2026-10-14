using UnityEngine;

namespace ARFruitSlasher.Procedural
{
    public static class ProceduralAudioGenerator
    {
        private const int SampleRate = 44100;

        public static AudioClip CreateSwishClip()
        {
            float duration = 0.15f;
            int samples = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Sin(Mathf.PI * (t / duration));
                float noise = (Random.value * 2f - 1f) * 0.4f;
                float freq = Mathf.Lerp(600f, 200f, t / duration);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t);
                data[i] = (noise + tone * 0.6f) * envelope;
            }

            AudioClip clip = AudioClip.Create("SwishSound", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateSliceClip()
        {
            float duration = 0.22f;
            int samples = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Exp(-t * 18f);
                float freq = Mathf.Lerp(1200f, 150f, Mathf.Pow(t / duration, 0.5f));
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t);
                float crunch = (Random.value * 2f - 1f) * Mathf.Exp(-t * 28f) * 0.6f;
                data[i] = (tone * 0.7f + crunch) * envelope;
            }

            AudioClip clip = AudioClip.Create("SliceSound", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateAlertClip()
        {
            // Rapid double radar ping
            float duration = 0.35f;
            int samples = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float subT = t % 0.17f;
                float envelope = Mathf.Exp(-subT * 22f);
                float tone = Mathf.Sin(2f * Mathf.PI * 1320f * t);
                data[i] = tone * envelope * 0.85f;
            }

            AudioClip clip = AudioClip.Create("AlertWarningSound", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateExplosionClip()
        {
            float duration = 0.6f;
            int samples = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Exp(-t * 7f);
                float noise = (Random.value * 2f - 1f);
                float subBass = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.8f;
                data[i] = (noise * 0.6f + subBass) * envelope;
            }

            AudioClip clip = AudioClip.Create("ExplosionSound", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateCheckpointClip()
        {
            // Arpeggio chime
            float duration = 0.5f;
            int samples = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[samples];
            float[] freqs = { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                int noteIndex = Mathf.Clamp(Mathf.FloorToInt((t / duration) * 4), 0, 3);
                float noteT = t - (noteIndex * (duration / 4f));
                float env = Mathf.Exp(-noteT * 12f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freqs[noteIndex] * t) * env * 0.7f;
            }

            AudioClip clip = AudioClip.Create("CheckpointFanfare", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateMissClip()
        {
            float duration = 0.28f;
            int samples = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 9f);
                float freq = Mathf.Lerp(250f, 90f, t / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.7f;
            }

            AudioClip clip = AudioClip.Create("MissSound", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
