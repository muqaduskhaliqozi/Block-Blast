using System;
using UnityEngine;

namespace BlockBlast.Board
{
    [System.Serializable]
    public struct BoardCoordinate : IEquatable<BoardCoordinate>
    {
        public int x;
        public int y;

        public BoardCoordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(BoardCoordinate other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is BoardCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public static bool operator ==(BoardCoordinate left, BoardCoordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoardCoordinate left, BoardCoordinate right)
        {
            return !left.Equals(right);
        }

        public static BoardCoordinate operator +(BoardCoordinate a, BoardCoordinate b)
        {
            return new BoardCoordinate(a.x + b.x, a.y + b.y);
        }

        public static BoardCoordinate operator -(BoardCoordinate a, BoardCoordinate b)
        {
            return new BoardCoordinate(a.x - b.x, a.y - b.y);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}
