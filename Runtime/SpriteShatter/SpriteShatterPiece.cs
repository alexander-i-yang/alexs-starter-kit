using Haze;
using UnityEngine;

namespace ASK.Runtime.SpriteShatter
{
    public class SpriteShatterPiece : MonoBehaviour
    {
        private static readonly int A = Shader.PropertyToID("_TriangleA");
        private static readonly int B = Shader.PropertyToID("_TriangleB");
        private static readonly int C = Shader.PropertyToID("_TriangleC");
        
        public virtual void Init(Sprite sprite, Triangulator.Triangle triangle)
        {
            var col = GetComponent<PolygonCollider2D>();
            col.points = triangle.arr;
            
            var sr = GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            triangle = Normalize(sprite, triangle);
            sr.material.SetVector(A, triangle.a);
            sr.material.SetVector(B, triangle.b);
            sr.material.SetVector(C, triangle.c);
        }
        
        /// <summary>
        /// Transform the triangle's points so that they are from [0,1] with the origin at the bottom left of the sprite.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private Triangulator.Triangle Normalize(Sprite sprite, Triangulator.Triangle t)
        {
            Vector2 extents = sprite.bounds.extents;
            t.a = (extents + t.a) / (extents * 2);
            t.b = (extents + t.b) / (extents * 2);
            t.c = (extents + t.c) / (extents * 2);
            return t;
        }
    }
}