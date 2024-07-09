using System.Linq;
using ASK.Runtime.Helpers;
using TriangleNet.Topology;
using UnityEngine;

namespace ASK.Runtime.SpriteShatter
{
    public class SpriteShatterPiece : MonoBehaviour
    {
        private static readonly int A = Shader.PropertyToID("_TriangleA");
        private static readonly int B = Shader.PropertyToID("_TriangleB");
        private static readonly int C = Shader.PropertyToID("_TriangleC");
        
        public virtual void Init(Sprite sprite, Triangle triangle)
        {
            var col = GetComponent<PolygonCollider2D>();
            col.points = triangle.Points().ToArray();
            
            var sr = GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            triangle = Normalize(sprite, triangle);
            sr.material.SetVector(A, triangle.A.ToVector2());
            sr.material.SetVector(B, triangle.B.ToVector2());
            sr.material.SetVector(C, triangle.C.ToVector2());
        }
        
        /// <summary>
        /// Transform the triangle's points so that they are from [0,1] with the origin at the bottom left of the sprite.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private Triangle Normalize(Sprite sprite, Triangle t)
        {
            Vector2 extents = sprite.bounds.extents;
            t.A = ((extents + t.A.ToVector2()) / (extents * 2)).ToVertex();
            t.B = ((extents + t.B.ToVector2()) / (extents * 2)).ToVertex();
            t.C = ((extents + t.C.ToVector2()) / (extents * 2)).ToVertex();
            return t;
        }
    }
}