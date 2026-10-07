using System;
using System.Collections;
using UnityEngine;

namespace BlockBlast.UI
{
    /// <summary>Easing curves used by all UI / FX animations.</summary>
    public static class Ease
    {
        public static float Linear(float t) => t;
        public static float OutCubic(float t) { t -= 1f; return t * t * t + 1f; }
        public static float InCubic(float t) => t * t * t;
        public static float InOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
        public static float OutQuad(float t) => 1f - (1f - t) * (1f - t);

        public static float OutBack(float t, float s = 1.70158f)
        {
            t -= 1f;
            return t * t * ((s + 1f) * t + s) + 1f;
        }

        public static float InBack(float t, float s = 1.70158f)
        {
            return (s + 1f) * t * t * t - s * t * t;
        }

        public static float OutElastic(float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            const float c4 = (2f * Mathf.PI) / 3f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }
    }

    /// <summary>Tiny coroutine based tween helper (no external packages needed).</summary>
    public static class Tween
    {
        public static IEnumerator Run(float duration, Action<float> step, float delay = 0f, bool unscaled = true)
        {
            if (delay > 0f)
            {
                float d = 0f;
                while (d < delay)
                {
                    d += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                    yield return null;
                }
            }

            if (duration <= 0f)
            {
                step(1f);
                yield break;
            }

            float t = 0f;
            step(0f);
            while (t < duration)
            {
                t += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                step(Mathf.Clamp01(t / duration));
                yield return null;
            }
            step(1f);
        }
    }
}
