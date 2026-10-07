using UnityEngine;
using BlockBlast.Core;

namespace BlockBlast.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        private UnityEngine.Camera cam;

        private void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
            ConfigureCamera();
        }

        public void ConfigureCamera()
        {
            if (config == null) config = GameConfig.Default;
            if (cam == null) cam = GetComponent<UnityEngine.Camera>();

            cam.orthographic = true;
            cam.backgroundColor = config.backgroundColor;

            // The layout (header + 8x8 board + tray) is designed for a 9.5 x 17.2 world-unit area.
            // Fit whichever side is the limiting one so nothing is ever cut off.
            float currentAspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            cam.orthographicSize = Mathf.Max(designHeight * 0.5f, designWidth / currentAspect * 0.5f);
            lastScreen = new Vector2Int(Screen.width, Screen.height);
        }

        [SerializeField] private float designWidth = 9.5f;
        [SerializeField] private float designHeight = 17.2f;
        private Vector2Int lastScreen;

        private void Update()
        {
            if (Screen.width != lastScreen.x || Screen.height != lastScreen.y) ConfigureCamera();
        }
    }
}
