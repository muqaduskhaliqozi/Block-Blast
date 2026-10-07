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

            // Compute ideal orthographic size based on aspect ratio
            float targetAspect = 9f / 16f;
            float currentAspect = (float)Screen.width / Screen.height;

            float baseOrthoSize = 7.5f;

            if (currentAspect < targetAspect)
            {
                // Screen is narrower than 9:16 (tall aspect ratios like 19.5:9, 20:9)
                cam.orthographicSize = baseOrthoSize * (targetAspect / currentAspect);
            }
            else
            {
                cam.orthographicSize = baseOrthoSize;
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            ConfigureCamera();
        }
    }
}
