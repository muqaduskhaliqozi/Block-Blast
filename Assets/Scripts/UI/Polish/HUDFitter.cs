using UnityEngine;
using UnityEngine.UI;

namespace BlockBlast.UI
{
    /// <summary>
    /// Sizes the gameplay header so it exactly fills the space between the top of the screen
    /// and the top of the board, on any aspect ratio.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class HUDFitter : MonoBehaviour
    {
        [SerializeField] private float boardTopWorldY = 5.05f;
        [SerializeField] private float minHeight = 330f;

        private RectTransform rt;
        private RectTransform canvasRect;

        private void Awake()
        {
            rt = (RectTransform)transform;
            Canvas c = GetComponentInParent<Canvas>();
            if (c != null) canvasRect = (RectTransform)c.rootCanvas.transform;
        }

        private void LateUpdate()
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            if (cam == null || canvasRect == null) return;

            float top = cam.orthographicSize; // camera rests at y = 0 (ignores shake offset)
            float frac = Mathf.Clamp01((top - boardTopWorldY) / (2f * cam.orthographicSize));
            float h = Mathf.Max(minHeight, frac * canvasRect.rect.height);
            if (!Mathf.Approximately(rt.sizeDelta.y, h))
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, h);
        }
    }
}
