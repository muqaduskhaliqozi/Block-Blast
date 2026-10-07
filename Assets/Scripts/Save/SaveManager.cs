using UnityEngine;

namespace BlockBlast.Save
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string BEST_SCORE_KEY = "BlockBlast_BestScore";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public int GetBestScore()
        {
            return PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        }

        public void SaveBestScore(int score)
        {
            int currentBest = GetBestScore();
            if (score > currentBest)
            {
                PlayerPrefs.SetInt(BEST_SCORE_KEY, score);
                PlayerPrefs.Save();
            }
        }

        public void ClearSaveData()
        {
            PlayerPrefs.DeleteKey(BEST_SCORE_KEY);
            PlayerPrefs.Save();
        }
    }
}
