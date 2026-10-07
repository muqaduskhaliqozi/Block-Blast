using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlockBlast.UI
{
    /// <summary>Slowly drifting, rotating translucent blocks used as a living background.</summary>
    [RequireComponent(typeof(RectTransform))]
    public class BackgroundFloaters : MonoBehaviour
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private int count = 14;
        [SerializeField] private Vector2 sizeRange = new Vector2(40f, 120f);
        [SerializeField] private Vector2 speedRange = new Vector2(25f, 70f);
        [SerializeField] private Vector2 alphaRange = new Vector2(0.08f, 0.22f);
        [SerializeField] private Color[] colors =
        {
            new Color(1.00f, 0.29f, 0.36f), new Color(0.00f, 0.82f, 1.00f), new Color(1.00f, 0.82f, 0.40f),
            new Color(0.02f, 0.84f, 0.63f), new Color(0.51f, 0.22f, 0.93f), new Color(1.00f, 0.62f, 0.11f),
            new Color(1.00f, 0.00f, 0.50f)
        };

        private class Floater { public RectTransform rt; public float speed; public float spin; public float sway; public float phase; }
        private readonly List<Floater> floaters = new List<Floater>();
        private RectTransform area;

        public void Setup(Sprite s, int n, float alphaMax)
        {
            sprite = s;
            count = n;
            alphaRange = new Vector2(alphaRange.x, alphaMax);
        }

        private void Start()
        {
            area = (RectTransform)transform;
            for (int i = 0; i < count; i++)
            {
                GameObject go = new GameObject("Floater", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                Image img = go.GetComponent<Image>();
                img.sprite = sprite;
                img.raycastTarget = false;
                Color c = colors[Random.Range(0, colors.Length)];
                c.a = Random.Range(alphaRange.x, alphaRange.y);
                img.color = c;

                Floater f = new Floater
                {
                    rt = (RectTransform)go.transform,
                    speed = Random.Range(speedRange.x, speedRange.y),
                    spin = Random.Range(-40f, 40f),
                    sway = Random.Range(10f, 40f),
                    phase = Random.Range(0f, 6.28f)
                };
                float size = Random.Range(sizeRange.x, sizeRange.y);
                f.rt.sizeDelta = new Vector2(size, size);
                f.rt.anchorMin = f.rt.anchorMax = new Vector2(0.5f, 0.5f);
                Rect r = area.rect;
                f.rt.anchoredPosition = new Vector2(Random.Range(r.xMin, r.xMax), Random.Range(r.yMin, r.yMax));
                f.rt.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                floaters.Add(f);
            }
        }

        private void Update()
        {
            Rect r = area.rect;
            float dt = Time.unscaledDeltaTime;
            float time = Time.unscaledTime;
            foreach (Floater f in floaters)
            {
                Vector2 p = f.rt.anchoredPosition;
                p.y += f.speed * dt;
                p.x += Mathf.Sin(time * 0.6f + f.phase) * f.sway * dt;
                float half = f.rt.sizeDelta.y;
                if (p.y > r.yMax + half)
                {
                    p.y = r.yMin - half;
                    p.x = Random.Range(r.xMin, r.xMax);
                }
                f.rt.anchoredPosition = p;
                f.rt.Rotate(0f, 0f, f.spin * dt);
            }
        }
    }
}
