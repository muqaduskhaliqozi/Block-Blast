using UnityEngine;

namespace BlockBlast.Audio
{
    public static class AudioSynth
    {
        public static AudioClip CreatePickupClip()
        {
            int sampleRate = 44100;
            float duration = 0.08f;
            int samplesCount = (int)(sampleRate * duration);
            float[] samples = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(400f, 800f, t / duration);
                float env = 1.0f - (t / duration);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.5f;
            }

            AudioClip clip = AudioClip.Create("PickupSFX", samplesCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip CreatePlaceClip()
        {
            int sampleRate = 44100;
            float duration = 0.12f;
            int samplesCount = (int)(sampleRate * duration);
            float[] samples = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(300f, 120f, t / duration);
                float env = Mathf.Exp(-15f * t);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.7f;
            }

            AudioClip clip = AudioClip.Create("PlaceSFX", samplesCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip CreateLineClearClip()
        {
            int sampleRate = 44100;
            float duration = 0.35f;
            int samplesCount = (int)(sampleRate * duration);
            float[] samples = new float[samplesCount];

            float[] freqs = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5 chord

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1.0f - (t / duration);
                float sampleVal = 0f;

                int step = (int)(t / (duration / freqs.Length));
                step = Mathf.Clamp(step, 0, freqs.Length - 1);
                float freq = freqs[step];

                sampleVal = Mathf.Sin(2f * Mathf.PI * freq * t) * env;
                samples[i] = sampleVal * 0.4f;
            }

            AudioClip clip = AudioClip.Create("LineClearSFX", samplesCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip CreateComboClip()
        {
            int sampleRate = 44100;
            float duration = 0.45f;
            int samplesCount = (int)(sampleRate * duration);
            float[] samples = new float[samplesCount];

            float[] freqs = new float[] { 440f, 554.37f, 659.25f, 880f }; // A major triad

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1.0f - (t / duration);
                int step = (int)(t / (duration / freqs.Length));
                step = Mathf.Clamp(step, 0, freqs.Length - 1);
                float freq = freqs[step];

                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.5f;
            }

            AudioClip clip = AudioClip.Create("ComboSFX", samplesCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip CreateGameOverClip()
        {
            int sampleRate = 44100;
            float duration = 0.6f;
            int samplesCount = (int)(sampleRate * duration);
            float[] samples = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(440f, 150f, t / duration);
                float env = 1.0f - (t / duration);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.5f;
            }

            AudioClip clip = AudioClip.Create("GameOverSFX", samplesCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip CreateButtonClickClip()
        {
            int sampleRate = 44100;
            float duration = 0.05f;
            int samplesCount = (int)(sampleRate * duration);
            float[] samples = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1.0f - (t / duration);
                samples[i] = (Random.value * 2f - 1f) * env * 0.2f;
            }

            AudioClip clip = AudioClip.Create("ButtonClickSFX", samplesCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
