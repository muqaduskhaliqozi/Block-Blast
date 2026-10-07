using UnityEngine;

namespace BlockBlast.Core
{
    [CreateAssetMenu(fileName = "Level_01", menuName = "Block Blast/Level Data")]
    public class LevelData : ScriptableObject
    {
        public int levelNumber = 1;
        [Header("Intro Text")]
        [TextArea(2, 4)] public string description = "";

        [Header("Board")]
        [Range(3, 10)] public int boardWidth = 4;
        [Range(3, 10)] public int boardHeight = 4;

        [Header("Win Condition")]
        [Tooltip("Is score par pahunchna zaroori. 0 = score ki shart nahi")]
        public int targetScore = 60;

        [Tooltip("Itni rows/columns clear karna zaroori. 0 = line ki shart nahi")]
        public int requiredLinesCleared = 1;

        [Header("Tray Blocks")]
        [Tooltip("Ek block mein zyada se zyada kitne boxes ho sakte hain. 0 = koi had nahi")]
        public int maxCellsPerBlock = 0;
    }
}