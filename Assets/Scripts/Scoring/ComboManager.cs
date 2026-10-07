using System;
using UnityEngine;

namespace BlockBlast.Scoring
{
    public class ComboManager : MonoBehaviour
    {
        public static ComboManager Instance { get; private set; }

        public int CurrentStreak { get; private set; } = 0;

        public event Action<int, string> OnComboTriggered;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void RegisterLineClear(int linesCleared)
        {
            CurrentStreak++;

            string msg = "GOOD!";
            if (linesCleared >= 3) msg = "UNSTOPPABLE!";
            else if (linesCleared == 2) msg = "GREAT!";
            else if (CurrentStreak >= 3) msg = $"COMBO x{CurrentStreak}!";
            else if (CurrentStreak == 2) msg = "DOUBLE COMBO!";

            OnComboTriggered?.Invoke(CurrentStreak, msg);

            if (Audio.AudioManager.Instance != null && CurrentStreak > 1)
            {
                Audio.AudioManager.Instance.PlayComboSound(CurrentStreak);
            }
        }

        public void ResetCombo()
        {
            CurrentStreak = 0;
        }
    }
}
