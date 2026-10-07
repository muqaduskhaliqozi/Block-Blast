using System.Collections.Generic;
using BlockBlast.Board;
using BlockBlast.Blocks;

namespace BlockBlast.Placement
{
    public static class PlacementValidator
    {
        public static bool CanPlaceBlock(BoardManager board, BlockShapeData shape, BoardCoordinate origin)
        {
            if (board == null || shape == null) return false;
            return board.CanPlaceShape(shape.offsets, origin);
        }

        public static bool FindAnyValidPlacement(BoardManager board, BlockShapeData shape, out BoardCoordinate validOrigin)
        {
            validOrigin = new BoardCoordinate(0, 0);
            if (board == null || shape == null) return false;

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    BoardCoordinate candidate = new BoardCoordinate(x, y);
                    if (board.CanPlaceShape(shape.offsets, candidate))
                    {
                        validOrigin = candidate;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
