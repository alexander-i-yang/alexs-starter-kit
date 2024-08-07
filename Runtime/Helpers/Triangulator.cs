using System.Collections.Generic;
using System.Linq;
using ASK.Helpers;
using ASK.Runtime.SpriteShatter;
using Clipper2Lib;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using UnityEditor;
using UnityEngine;
using Vertex = TriangleNet.Geometry.Vertex;

namespace ASK.Runtime.Helpers
{
    public static class Triangulator
    {
        /// <summary>
        /// Returns a list triangles with vertices normalized between (0,0) and (1,1).
        /// Normalization is a built-in feature of the underlying Triangle library.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="maxTriangleArea"></param>
        /// <returns></returns>
        public static ICollection<Triangle> Triangulate(Vector2[] mesh, float maxTriangleArea)
        {
            var poly = new Polygon();

            poly.Add(new Contour(mesh.Select(x => new Vertex(x.x, x.y))));
            var options = new ConstraintOptions() { ConformingDelaunay = true };

            var quality = new QualityOptions()
            {
                MaximumArea = maxTriangleArea
            };
            return poly.Triangulate(options, quality).Triangles;
        }

        public static void DrawGroups(SpriteShatterGroup[] groups, Vector3 offset = default, float[] colors = null)
        {
            for (var i = 0; i < groups.Length; i++)
            {
                var group = groups[i];
                var color = colors == null ? 0 : colors[i];
                var groupColors = group.Triangles.Select(_ => color).ToArray();
                DrawTriangles(group.Triangles.ToArray(), offset, groupColors);
            }
        }
        
        public static void DrawTriangles(Triangle[] triangles, Vector3 offset = default, float[] colors = null)
        {
            var tri = triangles.Select(t => t.vertices).ToArray();
            DrawTriangles(tri, offset, colors);
        }

        public static void DrawTriangles(Vertex[][] triangles, Vector3 offset = default, float[] colors = null)
        {
            if (triangles == null) return;

            if (offset == default) offset = Vector3.zero;
            #if UNITY_EDITOR
            for (var i = 0; i < triangles.Length; i++)
            {
                var pts = triangles[i];
                var drawPts = pts.Append(pts[0])
                    .Where(p => p != null)
                    .Select(v => v.ToVector2())
                    .Offset(offset)
                    .ToVector3()
                    .ToArray();
                Handles.color = new Color(1, 0, 0, colors == null ? 1 : colors[i]);
                Handles.DrawAAConvexPolygon(drawPts);
                Handles.color = Color.white;
                Handles.DrawAAPolyLine(drawPts);
            }
            #endif

        }

        public static Vector2 ToVector2(this Vertex v) => new((float)v.x, (float)v.y);

        public static Vertex ToVertex(this Vector2 v) => new(v.x, v.y);

        public static IEnumerable<Vector2> Points(this Triangle t) =>
            t.vertices.Select(v => v == null ? Vector2.zero : v.ToVector2());

        public static Vector2 Center(this Triangle t) => t.Points().Aggregate((cur, next) => cur + next) / 3;

        public static Vector2 Normalize(Sprite sprite, Vector2 v)
        {
            Vector2 extents = sprite.bounds.extents;
            return (extents + v) / (extents * 2);
        }

        public static Vertex Normalize(Sprite sprite, Vertex v)
        {
            return Normalize(sprite, v.ToVector2()).ToVertex();
        }

        public static Vertex[] DeNormalize(Sprite sprite, Triangle t)
        {
            return new[]
            {
                DeNormalize(sprite, t.A),
                DeNormalize(sprite, t.B),
                DeNormalize(sprite, t.C),
            };
        }

        public static Vertex DeNormalize(Sprite sprite, Vertex v) => DeNormalize(sprite, v.ToVector2()).ToVertex();

        public static Vector2 DeNormalize(Sprite sprite, Vector2 v)
        {
            Vector2 extents = sprite.bounds.extents;
            return (v - new Vector2(0.5f, 0.5f)) * extents * 2;
        }

        /// <summary>
        /// Transform the triangle's points so that they are from [0,1] with the origin at the bottom left of the sprite.
        /// (Aka transform into the sprite's uv coordinates.)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static void Normalize(Sprite sprite, Triangle t)
        {
            t.A = Normalize(sprite, t.A);
            t.B = Normalize(sprite, t.B);
            t.C = Normalize(sprite, t.C);
        }
        
        public static Point64 ToPoint64(this Vertex v)
        {
            return new Point64(v.x, v.y);
        }

        public static Vector2[] ToVectors(this Path64 p)
        {
            return p.Select(pt => new Vector2(pt.X, pt.Y)).ToArray();
        }
        
        public static Vector2 FindNearestPointOnLine(Vector2 origin, Vector2 direction, Vector2 point)
        {
            direction.Normalize();
            Vector2 lhs = point - origin;

            float dotP = Vector2.Dot(lhs, direction);
            return origin + direction * dotP;
        }

        public static float DistFromLine(Vector2 origin, Vector2 direction, Vector2 point)
        {
            Vector2 p = FindNearestPointOnLine(origin, direction, point);
            return (p - point).magnitude;
        }
    }
}