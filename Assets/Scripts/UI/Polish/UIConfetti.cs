using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlockBlast.UI
{
    /// <summary>Burst of falling confetti, played every time the object is enabled.</summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIConfetti : MonoBehaviour
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private int count = 70;
        [SerializeField] private float delay = 0.25f;

        private static readonly Color[] Palette =
        {
            new Color(1.00f, 0.29f, 0.36f), new Color(0.00f, 0.82f, 1.00f), new Color(1.00f, 0.82f, 0.40f),
            new Color(0.02f, 0.84f, 0.63f), new Color(0.51f, 0.22f, 0.93f), new Color(1.00f, 0.62f, 0.11f),
            new Color(1.00f, 0.00f, 0.50f), Color.white
        };

        private class Piece { public RectTransform rt; public Vector2 v; public float spin; public float life; public float flip; }
        private readonly List<Piece> pieces = new List<Piece>();
        private float startTimer;
        private bool spawned;

        public void Setup(Sprite s) { sprite = s; }

        private void OnEnable()
        {
            Clear();
            startTimer = delay;
            spawned = false;
        }

        private void OnDisable() => Clear();

        private void Clear()
        {
            foreach (Piece p in pieces) if (p.rt != null) Destroy(p.rt.gameObject);
            pieces.Clear();
        }

        private void Spawn()
        {
            RectTransform area = (RectTransform)transform;
            Rect r = area.rect;
            for (int i = 0; i < count; i++)
            {
                GameObject go = new GameObject("Confetti", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                Image img = go.GetComponent<Image>();
                img.sprite = sprite;
                img.raycastTarget = false;
                img.color = Palette[Random.Range(0, Palette.Length)];
                RectTransform rt = (RectTransform)go.transform;
                rt.sizeDelta = new Vector2(Random.Range(18f, 30f), Random.Range(26f, 42f));
                bool left = i % 2 == 0;
                rt.anchoredPosition = new Vector2(left ? r.xMin + 40f : r.xMax - 40f, r.center.y - r.height * 0.1f);
                Vector2 v = new Vector2((left ? 1f : -1f) * Random.Range(250f, 900f), Random.Range(900f, 2000f));
                pieces.Add(new Piece { rt = rt, v = v, spin = Random.Range(-720f, 720f), life = Random.Range(2.5f, 3.5f), flip = Random.Range(4f, 10f) });
            }
        }

        private void Update()
        {
            if (!spawned)
            {
                startTimer -= Time.unscaledDeltaTime;
                if (startTimer <= 0f) { Spawn(); spawned = true; }
                return;
            }

            float dt = Time.unscaledDeltaTime;
            for (int i = pieces.Count - 1; i >= 0; i--)
            {
                Piece p = pieces[i];
                p.life -= dt;
                p.v.y -= 1800f * dt;
                p.v *= 1f - 1.6f * dt;
                p.rt.anchoredPosition += p.v * dt;
                p.rt.Rotate(0f, 0f, p.spin * dt);
                Vector3 s = p.rt.localScale;
                s.x = Mathf.Sin(Time.unscaledTime * p.flip);
                p.rt.localScale = s;
                if (p.life <= 0f)
                {
                    Destroy(p.rt.gameObject);
                    pieces.RemoveAt(i);
                }
            }
        }
    }
}
