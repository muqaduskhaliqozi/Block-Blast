using UnityEngine;
using UnityEngine.UI;

namespace BlockBlast.UI
{
    /// <summary>Keeps a RectTransform inside the device safe area (notches, rounded corners).</summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private Rect lastSafe;
        private Vector2Int lastScreen;

        private void OnEnable() => Apply();
        private void Update()
        {
            if (Screen.safeArea != lastSafe || lastScreen.x != Screen.width || lastScreen.y != Screen.height) Apply();
        }

        private void Apply()
        {
            lastSafe = Screen.safeArea;
            lastScreen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0) return;

            RectTransform rt = (RectTransform)transform;
            Vector2 min = lastSafe.position;
            Vector2 max = lastSafe.position + lastSafe.size;
            min.x /= Screen.width; min.y /= Screen.height;
            max.x /= Screen.width; max.y /= Screen.height;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
