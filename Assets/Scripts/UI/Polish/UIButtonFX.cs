using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlockBlast.UI
{
    /// <summary>Squash on press, springy bounce on release. Add to any Button.</summary>
    public class UIButtonFX : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private float pressScale = 0.9f;

        private Vector3 baseScale = Vector3.one;
        private bool cached;
        private bool pressed;
        private Coroutine routine;
        private Selectable selectable;

        private void Awake()
        {
            Cache();
        }

        private void Cache()
        {
            if (cached) return;
            // Rest scale is always 1 so popup / entrance animations on parents can't corrupt it
            baseScale = Vector3.one;
            selectable = GetComponent<Selectable>();
            cached = true;
        }

        private void OnDisable()
        {
            if (cached) transform.localScale = baseScale;
            pressed = false;
            routine = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Cache();
            if (selectable != null && !selectable.IsInteractable()) return;
            pressed = true;
            Animate(baseScale * pressScale, 0.08f, false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!pressed) return;
            pressed = false;
            Animate(baseScale, 0.4f, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!pressed) return;
            pressed = false;
            Animate(baseScale, 0.3f, true);
        }

        private void Animate(Vector3 target, float duration, bool springy)
        {
            if (!isActiveAndEnabled) return;
            if (routine != null) StopCoroutine(routine);
            Vector3 from = transform.localScale;
            routine = StartCoroutine(Tween.Run(duration, t =>
            {
                float e = springy ? Ease.OutBack(t, 3.2f) : Ease.OutCubic(t);
                transform.localScale = Vector3.LerpUnclamped(from, target, e);
            }));
        }
    }
}
