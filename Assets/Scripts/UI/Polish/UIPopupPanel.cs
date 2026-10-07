using UnityEngine;
using UnityEngine.UI;

namespace BlockBlast.UI
{
    /// <summary>
    /// Modal popup animation: the dim overlay fades in, the box pops with an overshoot,
    /// then the listed items pop in one after another.
    /// Works with any script that simply calls SetActive(true) on the panel.
    /// </summary>
    public class UIPopupPanel : MonoBehaviour
    {
        [SerializeField] private Image dim;
        [SerializeField] private float dimAlpha = 0.78f;
        [SerializeField] private RectTransform box;
        [SerializeField] private RectTransform[] stagger;
        [SerializeField] private float staggerStart = 0.18f;
        [SerializeField] private float staggerStep = 0.07f;

        private Vector3[] staggerScales;

        public void Setup(Image dimImage, RectTransform boxRect, RectTransform[] items)
        {
            dim = dimImage;
            box = boxRect;
            stagger = items;
        }

        private void Awake()
        {
            if (stagger == null) stagger = new RectTransform[0];
            staggerScales = new Vector3[stagger.Length];
            for (int i = 0; i < stagger.Length; i++)
                staggerScales[i] = stagger[i] != null ? stagger[i].localScale : Vector3.one;
        }

        private void OnEnable()
        {
            StopAllCoroutines();

            if (dim != null)
            {
                Color c = dim.color;
                StartCoroutine(Tween.Run(0.25f, t =>
                {
                    c.a = Mathf.Lerp(0f, dimAlpha, t);
                    dim.color = c;
                }));
            }

            if (box != null)
            {
                StartCoroutine(Tween.Run(0.45f, t =>
                {
                    box.localScale = Vector3.one * Mathf.LerpUnclamped(0.55f, 1f, Ease.OutBack(t, 2.2f));
                }));
            }

            if (stagger == null) return;
            for (int i = 0; i < stagger.Length; i++)
            {
                RectTransform item = stagger[i];
                if (item == null) continue;
                Vector3 target = staggerScales != null && i < staggerScales.Length ? staggerScales[i] : Vector3.one;
                item.localScale = Vector3.zero;
                StartCoroutine(Tween.Run(0.38f, t =>
                {
                    item.localScale = target * Ease.OutBack(t, 2.4f);
                }, staggerStart + i * staggerStep));
            }
        }

        private void OnDisable()
        {
            if (box != null) box.localScale = Vector3.one;
            if (stagger == null || staggerScales == null) return;
            for (int i = 0; i < stagger.Length && i < staggerScales.Length; i++)
                if (stagger[i] != null) stagger[i].localScale = staggerScales[i];
        }
    }
}
