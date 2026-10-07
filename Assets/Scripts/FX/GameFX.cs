using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using BlockBlast.Board;
using BlockBlast.Core;
using BlockBlast.UI;

namespace BlockBlast.FX
{
    /// <summary>
    /// Gameplay juice: particle bursts on line clears, line flashes, camera shake,
    /// floating "+score" popups and the grey-out wave on game over.
    /// </summary>
    public class GameFX : MonoBehaviour
    {
        public static GameFX Instance { get; private set; }

        [Header("Sprites")]
        [SerializeField] private Sprite tileSprite;
        [SerializeField] private Sprite sparkleSprite;
        [SerializeField] private Sprite glowSprite;

        [Header("Score Popups (UI)")]
        [SerializeField] private RectTransform popupLayer;
        [SerializeField] private TMP_FontAsset popupFont;
        [SerializeField] private Material popupMaterial;

        [Header("Settings")]
        [SerializeField] private int sortingOrder = 70;
        [SerializeField] private int maxParticles = 300;

        private class Particle
        {
            public Transform t;
            public SpriteRenderer sr;
            public Vector3 velocity;
            public float gravity;
            public float age;
            public float life;
            public float spin;
            public Vector3 startScale;
            public Vector3 endScale;
            public Color color;
            public bool active;
        }

        private readonly List<Particle> particles = new List<Particle>();
        private Transform particleRoot;

        private UnityEngine.Camera cam;
        private Vector3 camOffset;
        private float shakeTime;
        private float shakeDuration;
        private float shakeAmp;

        public void Setup(Sprite tile, Sprite sparkle, Sprite glow, RectTransform layer, TMP_FontAsset font, Material mat)
        {
            tileSprite = tile;
            sparkleSprite = sparkle;
            glowSprite = glow;
            popupLayer = layer;
            popupFont = font;
            popupMaterial = mat;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            particleRoot = new GameObject("FX_Particles").transform;
            particleRoot.SetParent(transform, false);
        }

        private void Start()
        {
            cam = UnityEngine.Camera.main;
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
            if (Instance == this) Instance = null;
        }

        // ------------------------------------------------------------------
        // Particles
        // ------------------------------------------------------------------

        private Particle Get()
        {
            foreach (Particle p in particles)
                if (!p.active) return p;

            if (particles.Count >= maxParticles) return null;

            GameObject go = new GameObject("p");
            go.transform.SetParent(particleRoot, false);
            Particle np = new Particle { t = go.transform, sr = go.AddComponent<SpriteRenderer>() };
            np.sr.sortingOrder = sortingOrder;
            particles.Add(np);
            return np;
        }

        private Particle Emit(Sprite sprite, Vector3 pos, Vector3 vel, float gravity, float life, float spin,
                          Vector3 startScale, Vector3 endScale, Color color, int orderOffset = 0)
        {
            if (sprite == null) return null;
            Particle p = Get();
            if (p == null) return null;
            p.active = true;
            p.t.gameObject.SetActive(true);
            p.t.position = pos;
            p.t.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            p.velocity = vel;
            p.gravity = gravity;
            p.age = 0f;
            p.life = life;
            p.spin = spin;
            p.startScale = startScale;
            p.endScale = endScale;
            p.color = color;
            p.sr.sprite = sprite;
            p.sr.color = color;
            p.sr.sortingOrder = sortingOrder + orderOffset;
            p.t.localScale = startScale;
            return p;
        }

        /// <summary>Colourful burst for one cleared cell.</summary>
        public void CellBurst(Vector3 pos, Color color)
        {
            Color bright = Color.Lerp(color, Color.white, 0.25f);

            // glow pop
            Emit(glowSprite, pos, Vector3.zero, 0f, 0.35f, 0f, Vector3.one * 0.3f, Vector3.one * 0.9f,
                 new Color(bright.r, bright.g, bright.b, 0.9f), -1);

            // mini tiles flying out
            for (int i = 0; i < 5; i++)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                Vector3 v = new Vector3(dir.x, dir.y, 0f) * Random.Range(2.5f, 5.5f) + Vector3.up * Random.Range(2f, 4f);
                float s = Random.Range(0.22f, 0.36f);
                Emit(tileSprite, pos, v, -16f, Random.Range(0.55f, 0.85f), Random.Range(-540f, 540f),
                     Vector3.one * s, Vector3.one * s * 0.3f, color, 2);
            }

            // sparkles
            for (int i = 0; i < 2; i++)
            {
                Vector2 off = Random.insideUnitCircle * 0.35f;
                Emit(sparkleSprite, pos + (Vector3)off, new Vector3(0f, Random.Range(0.5f, 1.5f), 0f), 0f,
                     Random.Range(0.35f, 0.55f), Random.Range(-180f, 180f),
                     Vector3.one * Random.Range(0.35f, 0.55f), Vector3.zero, Color.white, 3);
            }
        }

        /// <summary>Bright streak along a cleared row or column.</summary>
        public void LineFlash(Vector3 center, bool horizontal, float length)
        {
            if (glowSprite == null) return;
            float unit = glowSprite.bounds.size.x;
            float along = (length + 1.6f) / unit;
            float across = 1.4f / unit;
            Vector3 start = horizontal ? new Vector3(along * 0.6f, across, 1f) : new Vector3(across, along * 0.6f, 1f);
            Vector3 end = horizontal ? new Vector3(along * 1.1f, across * 0.2f, 1f) : new Vector3(across * 0.2f, along * 1.1f, 1f);
            Particle flash = Emit(glowSprite, center, Vector3.zero, 0f, 0.45f, 0f, start, end, new Color(1f, 1f, 1f, 0.95f), 1);
            if (flash != null) flash.t.rotation = Quaternion.identity;
        }

        public void Shake(float amplitude, float duration)
        {
            shakeAmp = Mathf.Max(shakeAmp * (shakeTime > 0f ? 1f : 0f), amplitude);
            shakeDuration = duration;
            shakeTime = duration;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            foreach (Particle p in particles)
            {
                if (!p.active) continue;
                p.age += dt;
                float k = p.age / p.life;
                if (k >= 1f)
                {
                    p.active = false;
                    p.t.gameObject.SetActive(false);
                    continue;
                }
                p.velocity.y += p.gravity * dt;
                p.t.position += p.velocity * dt;
                if (p.spin != 0f) p.t.Rotate(0f, 0f, p.spin * dt);
                p.t.localScale = Vector3.Lerp(p.startScale, p.endScale, Ease.OutCubic(k));
                Color c = p.color;
                c.a *= k < 0.55f ? 1f : 1f - (k - 0.55f) / 0.45f;
                p.sr.color = c;
            }
        }

        private void LateUpdate()
        {
            if (cam == null) return;
            // remove last frame's offset, then apply a new one
            cam.transform.position -= camOffset;
            camOffset = Vector3.zero;
            if (shakeTime > 0f)
            {
                shakeTime -= Time.deltaTime;
                float strength = shakeAmp * Mathf.Clamp01(shakeTime / Mathf.Max(0.01f, shakeDuration));
                Vector2 r = Random.insideUnitCircle * strength;
                camOffset = new Vector3(r.x, r.y, 0f);
                cam.transform.position += camOffset;
            }
        }

        // ------------------------------------------------------------------
        // Score popup
        // ------------------------------------------------------------------

        public void ShowScorePopup(Vector3 worldPos, int amount, bool big)
        {
            if (amount <= 0 || popupLayer == null || cam == null) return;

            Vector3 screen = cam.WorldToScreenPoint(worldPos);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(popupLayer, screen, null, out Vector2 local)) return;

            GameObject go = new GameObject("ScorePopup", typeof(RectTransform));
            go.transform.SetParent(popupLayer, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(400f, 150f);
            rt.anchoredPosition = local;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            if (popupFont != null) tmp.font = popupFont;
            if (popupMaterial != null) tmp.fontSharedMaterial = popupMaterial;
            tmp.text = "+" + amount;
            tmp.fontSize = big ? 96f : 66f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.enableVertexGradient = true;
            tmp.colorGradient = big
                ? new VertexGradient(new Color(1f, 0.97f, 0.6f), new Color(1f, 0.97f, 0.6f), new Color(1f, 0.6f, 0.1f), new Color(1f, 0.6f, 0.1f))
                : new VertexGradient(Color.white, Color.white, new Color(0.75f, 0.9f, 1f), new Color(0.75f, 0.9f, 1f));

            StartCoroutine(PopupRoutine(rt, tmp));
        }

        private IEnumerator PopupRoutine(RectTransform rt, TextMeshProUGUI tmp)
        {
            Vector2 start = rt.anchoredPosition;
            float dur = 0.9f;
            float t = 0f;
            while (t < dur && rt != null)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                float pop = k < 0.25f ? Ease.OutBack(k / 0.25f, 3f) : 1f;
                rt.localScale = Vector3.one * pop;
                rt.anchoredPosition = start + new Vector2(0f, 150f * Ease.OutCubic(k));
                tmp.alpha = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;
                yield return null;
            }
            if (rt != null) Destroy(rt.gameObject);
        }

        // ------------------------------------------------------------------
        // Game over wave
        // ------------------------------------------------------------------

        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            if (newState == GameState.GameOver) StartCoroutine(GameOverWave());
        }

        private IEnumerator GameOverWave()
        {
            BoardManager board = BoardManager.Instance;
            if (board == null) yield break;
            Shake(0.12f, 0.3f);
            for (int y = board.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    BoardCell cell = board.GetCell(new BoardCoordinate(x, y));
                    if (cell != null && cell.IsOccupied) cell.SetDimmed();
                }
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}
