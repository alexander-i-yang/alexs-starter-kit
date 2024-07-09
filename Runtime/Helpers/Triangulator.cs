using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using UnityEngine;

namespace ASK.Runtime.Helpers
{
    public static class Triangulator
    {
        public static ICollection<Triangle> DenseTriangulate(Vector2[] mesh, float maxTriangleArea)
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

        public static Vector2 ToVector2(this Vertex v) => new ((float)v.x, (float)v.y);
        
        public static Vertex ToVertex(this Vector2 v) => new (v.x, v.y);
        
        public static IEnumerable<Vector2> Points(this Triangle t) =>
            t.vertices.Select(v => v.ToVector2());

        public static Vector2 A(this Triangle t) => t.GetVertex(0).ToVector2();
        public static Vector2 B(this Triangle t) => t.GetVertex(1).ToVector2();
        public static Vector2 C(this Triangle t) => t.GetVertex(2).ToVector2();
    }
}