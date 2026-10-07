using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Board;

namespace BlockBlast.Blocks
{
    public static class BlockShapeLibrary
    {
        private static List<BlockShapeData> _allShapes;

        public static List<BlockShapeData> GetAllShapes()
        {
            if (_allShapes != null) return _allShapes;

            _allShapes = new List<BlockShapeData>();

            // 1. Single Dot (1x1)
            _allShapes.Add(new BlockShapeData("Dot_1x1", new BoardCoordinate(0, 0)));

            // 2. Horizontal & Vertical Bars (2x1 to 5x1)
            _allShapes.Add(new BlockShapeData("Bar_2x1_H", new BoardCoordinate(0, 0), new BoardCoordinate(1, 0)));
            _allShapes.Add(new BlockShapeData("Bar_2x1_V", new BoardCoordinate(0, 0), new BoardCoordinate(0, 1)));

            _allShapes.Add(new BlockShapeData("Bar_3x1_H", new BoardCoordinate(0, 0), new BoardCoordinate(1, 0), new BoardCoordinate(2, 0)));
            _allShapes.Add(new BlockShapeData("Bar_3x1_V", new BoardCoordinate(0, 0), new BoardCoordinate(0, 1), new BoardCoordinate(0, 2)));

            _allShapes.Add(new BlockShapeData("Bar_4x1_H", new BoardCoordinate(0, 0), new BoardCoordinate(1, 0), new BoardCoordinate(2, 0), new BoardCoordinate(3, 0)));
            _allShapes.Add(new BlockShapeData("Bar_4x1_V", new BoardCoordinate(0, 0), new BoardCoordinate(0, 1), new BoardCoordinate(0, 2), new BoardCoordinate(0, 3)));

            _allShapes.Add(new BlockShapeData("Bar_5x1_H", new BoardCoordinate(0, 0), new BoardCoordinate(1, 0), new BoardCoordinate(2, 0), new BoardCoordinate(3, 0), new BoardCoordinate(4, 0)));
            _allShapes.Add(new BlockShapeData("Bar_5x1_V", new BoardCoordinate(0, 0), new BoardCoordinate(0, 1), new BoardCoordinate(0, 2), new BoardCoordinate(0, 3), new BoardCoordinate(0, 4)));

            // 3. Squares (2x2, 3x3)
            _allShapes.Add(new BlockShapeData("Square_2x2", 
                new BoardCoordinate(0, 0), new BoardCoordinate(1, 0), 
                new BoardCoordinate(0, 1), new BoardCoordinate(1, 1)));

            _allShapes.Add(new BlockShapeData("Square_3x3",
                new BoardCoordinate(0, 0), new BoardCoordinate(1, 0), new BoardCoordinate(2, 0),
                new BoardCoordinate(0, 1), new BoardCoordinate(1, 1), new BoardCoordinate(2, 1),
                new BoardCoordinate(0, 2), new BoardCoordinate(1, 2), new BoardCoordinate(2, 2)));

            // 4. Small Corners / L-shapes (2x2)
            _allShapes.Add(new BlockShapeData("Corner_2x2_BL", new BoardCoordinate(0, 0), new BoardCoordinate(1, 0), new BoardCoordinate(0, 1)));
            _allShapes.Add(new BlockShapeData("Corner_2x2_TL", new BoardCoordinate(0, 0), new BoardCoordinate(0, 1), new BoardCoordinate(1, 1)));
            _allShapes.Add(new BlockShapeData("Corner_2x2_TR", new BoardCoordinate(1, 0), new BoardCoordinate(0, 1), new BoardCoordinate(1, 1)));
            _allShapes.Add(new BlockShapeData("Corner_2x2_BR", new BoardCoordinate(0, 0), new BoardCoordinate(1, 0), new BoardCoordinate(1, 1)));

            // 5. Large L-shapes (3x3)
            _allShapes.Add(new BlockShapeData("L_3x3_Normal",
                new BoardCoordinate(0, 0), new BoardCoordinate(0, 1), new BoardCoordinate(0, 2),
                new BoardCoordinate(1, 0), new BoardCoordinate(2, 0)));

            _allShapes.Add(new BlockShapeData("L_3x3_Rot1",
                new BoardCoordinate(0, 2), new BoardCoordinate(1, 2), new BoardCoordinate(2, 2),
                new BoardCoordinate(0, 0), new BoardCoordinate(0, 1)));

            _allShapes.Add(new BlockShapeData("L_3x3_Rot2",
                new BoardCoordinate(0, 2), new BoardCoordinate(1, 2), new BoardCoordinate(2, 2),
                new BoardCoordinate(2, 0), new BoardCoordinate(2, 1)));

            _allShapes.Add(new BlockShapeData("L_3x3_Rot3",
                new BoardCoordinate(2, 0), new BoardCoordinate(2, 1), new BoardCoordinate(2, 2),
                new BoardCoordinate(0, 0), new BoardCoordinate(1, 0)));

            // 6. T-shapes
            _allShapes.Add(new BlockShapeData("T_Down", new BoardCoordinate(0, 1), new BoardCoordinate(1, 1), new BoardCoordinate(2, 1), new BoardCoordinate(1, 0)));
            _allShapes.Add(new BlockShapeData("T_Up", new BoardCoordinate(0, 0), new BoardCoordinate(1, 0), new BoardCoordinate(2, 0), new BoardCoordinate(1, 1)));
            _allShapes.Add(new BlockShapeData("T_Left", new BoardCoordinate(1, 0), new BoardCoordinate(1, 1), new BoardCoordinate(1, 2), new BoardCoordinate(0, 1)));
            _allShapes.Add(new BlockShapeData("T_Right", new BoardCoordinate(0, 0), new BoardCoordinate(0, 1), new BoardCoordinate(0, 2), new BoardCoordinate(1, 1)));

            // 7. Z-shape & S-shape
            _allShapes.Add(new BlockShapeData("Z_H", new BoardCoordinate(0, 1), new BoardCoordinate(1, 1), new BoardCoordinate(1, 0), new BoardCoordinate(2, 0)));
            _allShapes.Add(new BlockShapeData("S_H", new BoardCoordinate(0, 0), new BoardCoordinate(1, 0), new BoardCoordinate(1, 1), new BoardCoordinate(2, 1)));
            _allShapes.Add(new BlockShapeData("Z_V", new BoardCoordinate(0, 0), new BoardCoordinate(0, 1), new BoardCoordinate(1, 1), new BoardCoordinate(1, 2)));
            _allShapes.Add(new BlockShapeData("S_V", new BoardCoordinate(1, 0), new BoardCoordinate(1, 1), new BoardCoordinate(0, 1), new BoardCoordinate(0, 2)));

            // 8. Plus-shape
            _allShapes.Add(new BlockShapeData("Plus",
                new BoardCoordinate(1, 0),
                new BoardCoordinate(0, 1), new BoardCoordinate(1, 1), new BoardCoordinate(2, 1),
                new BoardCoordinate(1, 2)));

            return _allShapes;
        }

        public static BlockShapeData GetRandomShape()
        {
            var shapes = GetAllShapes();
            int idx = Random.Range(0, shapes.Count);
            return shapes[idx];
        }
    }
}
