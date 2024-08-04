using System;
using UnityEngine;

namespace ASK.Runtime.Core
{
    public enum Direction
    {
        Down,
        Up,
        Left,
        Right,
    }
    
    public static class DirectionHelper {
        public static Vector2Int ToV(this Direction d)
        {
            switch (d)
            {
                case Direction.Down: return Vector2Int.down;
                case Direction.Up: return Vector2Int.up;
                case Direction.Right: return Vector2Int.right;
                case Direction.Left: return Vector2Int.left;
            }

            throw new ArgumentException($"Unknown direction {d}");
        }
    }
}