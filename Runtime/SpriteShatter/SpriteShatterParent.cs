using System.Linq;
using ASK.Runtime.Helpers;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using UnityEngine;

namespace ASK.Runtime.SpriteShatter
{
    public interface IShatterPiece
    {
        void Init(Sprite sprite, Triangle[] triangles);
        void ApplyForce(Vector2 force);
        public Triangle[] GetTriangles();
    }

    public class SpriteShatterParent : MonoBehaviour, IShatterPiece
    {
        private static readonly int A = Shader.PropertyToID("_TriangleA");
        private static readonly int B = Shader.PropertyToID("_TriangleB");
        private static readonly int C = Shader.PropertyToID("_TriangleC");

        public GameObject spriteShatterPiece;

        private Triangle[] _triangles;
        
        public virtual void Init(Sprite sprite, Triangle[] triangles)
        {
            _triangles = triangles;
            foreach (var triangle in triangles)
            {
                var newObj =
                    Instantiate(spriteShatterPiece, transform.position, Quaternion.identity, transform);
                
                var col = newObj.GetComponent<PolygonCollider2D>();
                col.points = triangle.Points().ToArray();
            
                var sr = newObj.GetComponent<SpriteRenderer>();
                sr.sprite = sprite;
                var normTriangle = Normalize(sprite, triangle);
                sr.material.SetVector(A, normTriangle.A.ToVector2());
                sr.material.SetVector(B, normTriangle.B.ToVector2());
                sr.material.SetVector(C, normTriangle.C.ToVector2());
            }
        }
        
        /// <summary>
        /// Transform the triangle's points so that they are from [0,1] with the origin at the bottom left of the sprite.
        /// (Aka transform into the sprite's uv coordinates.)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private Triangle Normalize(Sprite sprite, Triangle t)
        {
            t.A = Normalize(sprite, t.A);
            t.B = Normalize(sprite, t.B);
            t.C = Normalize(sprite, t.C);
            return t;
        }
        
        public static Vector2 Normalize(Sprite sprite, Vector2 v)
        {
            Vector2 extents = sprite.bounds.extents;
            return (extents + v) / (extents * 2);
        }
        
        public static Vertex Normalize(Sprite sprite, Vertex v)
        {
            Vector2 extents = sprite.bounds.extents;
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

        public void ApplyForce(Vector2 force)
        {
            GetComponent<Rigidbody2D>().AddForce(force);
        }

        public Triangle[] GetTriangles() => _triangles;
    }
}