using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ASK.Helpers;
using ASK.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace ASK.Runtime.Helpers
{
    public static class TexToPoly
    {
        /// <summary>
        /// Generates a polygon from the given texture using alpha cutoffs.
        /// Does not work with holes/islands.
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="alphaCutoff"></param>
        /// <returns></returns>
        /// <exception cref="ConstraintException"></exception>
        public static Vector2[] GetPolygon(Texture2D tex, float alphaCutoff = 0.01f)
        {
            if (!tex.isReadable)
            {
                throw new ConstraintException("Tex isn't readable. Set Read/Write to true in asset menu.");
            }

            Vector2 texSize = new Vector2(tex.width, tex.height);
            
            var matrix = GetMatrix(tex, alphaCutoff);

            for (int y = 0; y < matrix.GetLength(0)-1; ++y)
            {
                for (int x = 0; x < matrix.GetLength(1)-1; ++x)
                {
                    var pts = March(matrix, x, y);
                    if (pts.Length != 0)
                    {
                        return pts.Offset(-texSize/2).ToArray();
                    }
                }
            }

            return Array.Empty<Vector2>();
        }

        /// <summary>
        /// Returns matrix of booleans indicating whether the pixel at that point is false or true.
        /// Also includes a surrounding offset of empty pixels.
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="alphaCutoff"></param>
        /// <returns></returns>
        public static bool[,] GetMatrix(Texture2D tex, float alphaCutoff)
        {
            bool[,] matrix = new bool[tex.height + 2, tex.width + 2];
            for (int y = 0; y < tex.height; ++y)
            {
                for (int x = 0; x < tex.width; ++x)
                {
                    matrix[y + 1, x + 1] = tex.GetPixel(x, y).a >= alphaCutoff;
                }
            }

            return matrix;
        }

        public static Vector2[] March(bool[,] matrix, int x, int y)
        {
            var cell = GetCell(matrix, x, y);
            if (CellToInt(cell) == 0) return new Vector2[0];
            Vector2Int cur = new Vector2Int(x, y);
            Vector2Int origCur = new Vector2Int(x, y);
            List<Vector2> ret = new List<Vector2>();
            for (int i = 0; i < 1000000; ++i)
            {
                ret.Add(cur);
                cell = GetCell(matrix, cur.x, cur.y);
                Direction d = GetDirection(cell);
                cur += d.ToV();
                if (origCur == cur)
                {
                    ret.Add(cur);
                    break;
                }
            }

            return ret.ToArray();
        }

        public static Direction GetDirection(bool[] cell)
        {
            int c = CellToInt(cell);
            if (codes.ContainsKey(c)) return codes[c];
            throw new Exception("Error: codes does not contain cell");
        }

        public static bool[] GetCell(bool[,] matrix, int c, int r)
        {
            return new[]
            {
                matrix[r, c],
                matrix[r, c + 1],
                matrix[r + 1, c],
                matrix[r + 1, c + 1],
            };
        }

        public static int CellToInt(bool[] cell)
        {
            int[] celli = cell.Select(b => b ? 1 : 0).ToArray();
            return
                celli[0] +
                celli[1] * 2 +
                celli[2] * 4 +
                celli[3] * 8;
        }
        
        public static Dictionary<int, Direction> codes = new()
        {
            {CellToInt(new [] {false, false, false, false}), Direction.Down},
            {CellToInt(new [] {false, false, false, true}), Direction.Right},
            {CellToInt(new [] {false, false, true, false}), Direction.Up},
            {CellToInt(new [] {false, false, true, true}), Direction.Right},
            {CellToInt(new [] {false, true, false, false}), Direction.Down},
            {CellToInt(new [] {false, true, false, true}), Direction.Down},
            // {CellToInt(new [] {false, true, true, false}), Direction.Down},
            {CellToInt(new [] {false, true, true, true}), Direction.Down},
            {CellToInt(new [] {true, false, false, false}), Direction.Left},
            //{CellToInt(new [] {true, false, false, true}), Direction.Down},
            {CellToInt(new [] {true, false, true, false}), Direction.Up},
            {CellToInt(new [] {true, false, true, true}), Direction.Right},
            {CellToInt(new [] {true, true, false, false}), Direction.Left},
            {CellToInt(new [] {true, true, false, true}), Direction.Left},
            {CellToInt(new [] {true, true, true, false}), Direction.Up},
            {CellToInt(new [] {true, true, true, true}), Direction.Down},
        };
    }
}