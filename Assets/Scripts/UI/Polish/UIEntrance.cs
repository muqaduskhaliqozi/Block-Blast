using UnityEngine;

namespace BlockBlast.UI
{
    /// <summary>Plays an entrance animation (slide / scale / fade) whenever the object is enabled.</summary>
    public class UIEntrance : MonoBehaviour
    {
        public enum Curve { OutBack, OutCubic, OutElastic }

        public Vector2 fromOffset = new Vector2(0f, -80f);
        public float fromScale = 1f;
        [Range(0f, 1f)] public float fromAlpha = 0f;
        public float delay = 0f;
        public float duration = 0.5f;
        public Curve curve = Curve.OutBack;
        public bool playOnEnable = true;

        private RectTransform rt;
        private CanvasGroup cg;
        private Vector2 basePos;
        private Vector3 baseScale;
        private bool cached;

        public void Setup(Vector2 offset, float scale, float alpha, float delaySeconds, float durationSeconds, Curve c)
        {
            fromOffset = offset;
            fromScale = scale;
            fromAlpha = alpha;
            delay = delaySeconds;
            duration = durationSeconds;
            curve = c;
        }

        private void Cache()
        {
            if (cached) return;
            rt = transform as RectTransform;
            basePos = rt.anchoredPosition;
            baseScale = rt.localScale;
            cg = GetComponent<CanvasGroup>();
            if (cg == null && fromAlpha < 1f) cg = gameObject.AddComponent<CanvasGroup>();
            cached = true;
        }

        private void OnEnable()
        {
            Cache();
            if (playOnEnable) Play();
        }

        private void OnDisable()
        {
            if (cached) Apply(1f);
        }

        public void Play()
        {
            Cache();
            StopAllCoroutines();
            Apply(0f);
            StartCoroutine(Tween.Run(duration, Apply, delay));
        }

        private void Apply(float t)
        {
            float e;
            switch (curve)
            {
                case Curve.OutElastic: e = Ease.OutElastic(t); break;
                case Curve.OutCubic: e = Ease.OutCubic(t); break;
                default: e = Ease.OutBack(t); break;
            }

            rt.anchoredPosition = Vector2.LerpUnclamped(basePos + fromOffset, basePos, e);
            rt.localScale = Vector3.LerpUnclamped(baseScale * fromScale, baseScale, e);
            if (cg != null) cg.alpha = Mathf.Lerp(fromAlpha, 1f, Mathf.Clamp01(t * 2.5f));
        }
    }
}
