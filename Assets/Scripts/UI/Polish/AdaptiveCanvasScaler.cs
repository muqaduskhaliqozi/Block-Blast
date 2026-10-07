using UnityEngine;
using UnityEngine.UI;

namespace BlockBlast.UI
{
    /// <summary>
    /// Matches width on tall phones and height on wide screens/tablets, so the UI always lines up
    /// with the camera framing used by CameraController.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class AdaptiveCanvasScaler : MonoBehaviour
    {
        [SerializeField] private float designAspect = 9.5f / 17.2f;
        private CanvasScaler scaler;
        private float lastAspect = -1f;

        private void Awake()
        {
            scaler = GetComponent<CanvasScaler>();
            Apply();
        }

        private void Update()
        {
            Apply();
        }

        private void Apply()
        {
            if (Screen.height <= 0) return;
            float aspect = (float)Screen.width / Screen.height;
            if (Mathf.Approximately(aspect, lastAspect)) return;
            lastAspect = aspect;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = aspect < designAspect ? 0f : 1f;
        }
    }
}
