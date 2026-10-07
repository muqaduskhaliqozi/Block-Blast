using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlockBlast.UI
{
    /// <summary>Stars pop in one by one (used on the level complete popup).</summary>
    public class UIStarsReveal : MonoBehaviour
    {
        [SerializeField] private RectTransform[] stars;
        [SerializeField] private float startDelay = 0.45f;
        [SerializeField] private float step = 0.22f;

        public void Setup(RectTransform[] s) { stars = s; }

        private void OnEnable()
        {
            if (stars == null) return;
            StopAllCoroutines();
            for (int i = 0; i < stars.Length; i++)
            {
                RectTransform star = stars[i];
                if (star == null) continue;
                star.localScale = Vector3.zero;
                int index = i;
                StartCoroutine(Tween.Run(0.45f, t =>
                {
                    star.localScale = Vector3.one * Ease.OutBack(t, 3f);
                    star.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(-90f, 0f, Ease.OutCubic(t)));
                    if (t <= 0f && Audio.AudioManager.Instance != null && index >= 0)
                        Audio.AudioManager.Instance.PlayComboSound(index + 2);
                }, startDelay + i * step));
            }
        }
    }
}
