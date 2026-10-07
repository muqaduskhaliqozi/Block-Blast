using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Board;

namespace BlockBlast.Blocks
{
    [System.Serializable]
    public class BlockShapeData
    {
        public string shapeName;
        public List<BoardCoordinate> offsets;

        public BlockShapeData(string name, params BoardCoordinate[] coords)
        {
            shapeName = name;
            offsets = new List<BoardCoordinate>(coords);
        }

        public int CellCount => offsets != null ? offsets.Count : 0;

        public Vector2 GetCenterOffset()
        {
            if (offsets == null || offsets.Count == 0) return Vector2.zero;

            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;

            foreach (var c in offsets)
            {
                if (c.x < minX) minX = c.x;
                if (c.x > maxX) maxX = c.x;
                if (c.y < minY) minY = c.y;
                if (c.y > maxY) maxY = c.y;
            }

            float centerX = (minX + maxX) * 0.5f;
            float centerY = (minY + maxY) * 0.5f;

            return new Vector2(centerX, centerY);
        }
    }
}
