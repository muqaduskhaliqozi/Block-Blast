using UnityEngine;

namespace BlockBlast.UI
{
    /// <summary>Endless idle motion: breathing pulse, floating bob, wobble or spin.</summary>
    public class UIIdleAnim : MonoBehaviour
    {
        public enum Mode { Pulse, Bob, Wobble, Spin }

        [SerializeField] private Mode mode = Mode.Pulse;
        [SerializeField] private float amplitude = 0.05f;   // Pulse: scale %, Bob: pixels, Wobble: degrees, Spin: unused
        [SerializeField] private float speed = 2f;          // radians per second (Spin: degrees per second)
        [SerializeField] private bool randomPhase = false;

        private RectTransform rt;
        private Vector3 baseScale;
        private Vector2 basePos;
        private float baseRot;
        private float phase;

        public void Setup(Mode m, float amp, float spd, bool randomizePhase = false)
        {
            mode = m;
            amplitude = amp;
            speed = spd;
            randomPhase = randomizePhase;
        }

        private void Awake()
        {
            rt = transform as RectTransform;
            baseScale = transform.localScale;
            basePos = rt != null ? rt.anchoredPosition : (Vector2)transform.localPosition;
            baseRot = transform.localEulerAngles.z;
            phase = randomPhase ? Random.Range(0f, Mathf.PI * 2f) : 0f;
        }

        private void Update()
        {
            float s = Mathf.Sin(Time.unscaledTime * speed + phase);
            switch (mode)
            {
                case Mode.Pulse:
                    transform.localScale = baseScale * (1f + amplitude * s);
                    break;
                case Mode.Bob:
                    if (rt != null) rt.anchoredPosition = basePos + new Vector2(0f, amplitude * s);
                    break;
                case Mode.Wobble:
                    transform.localRotation = Quaternion.Euler(0f, 0f, baseRot + amplitude * s);
                    break;
                case Mode.Spin:
                    transform.Rotate(0f, 0f, speed * Time.unscaledDeltaTime);
                    break;
            }
        }
    }
}
