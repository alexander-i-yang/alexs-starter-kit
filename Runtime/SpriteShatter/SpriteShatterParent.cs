using System.Linq;
using ASK.Runtime.Helpers;
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

        private Rigidbody2D _rb;
        
        public virtual void Init(Sprite sprite, Triangle[] triangles)
        {
            _rb = GetComponent<Rigidbody2D>();
            _triangles = triangles;
            foreach (var triangle in triangles)
            {
                var newObj =
                    Instantiate(spriteShatterPiece, transform.position, Quaternion.identity, transform);
                
                var col = newObj.GetComponent<PolygonCollider2D>();
                col.points = triangle.Points().ToArray();
            
                var sr = newObj.GetComponent<SpriteRenderer>();
                sr.sprite = sprite;
                var normTriangle = Triangulator.Normalize(sprite, triangle);
                sr.material.SetVector(A, normTriangle.A.ToVector2());
                sr.material.SetVector(B, normTriangle.B.ToVector2());
                sr.material.SetVector(C, normTriangle.C.ToVector2());
            }
        }

        private void FixedUpdate()
        {
            if (_rb == null) return;
            
            if (_rb.velocity.magnitude < 0.0001f)
                _rb.velocity = Vector2.zero;
        }

        public void ApplyForce(Vector2 force)
        {
            _rb.AddForce(force, ForceMode2D.Impulse);
        }

        public Triangle[] GetTriangles() => _triangles;
    }
}