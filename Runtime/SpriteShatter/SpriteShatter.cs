using ASK.Runtime.Helpers;
using UnityEngine;
using MyBox;
using TriangleNet.Topology;

namespace ASK.Runtime.SpriteShatter
{
    public class SpriteShatter : MonoBehaviour
    {
        [SerializeField] private GameObject clone;

        private SpriteRenderer _sprite;

        public Vector2[] Mesh;

        [SerializeField] private float MaxTriangleArea = 2;

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
        }

        public void Shatter()
        {
            var triangles = Triangulator.DenseTriangulate(Mesh, MaxTriangleArea);
            triangles.ForEach(CreatePiece);
        }

        public void CreatePiece(Triangle t)
        {
            var clon = Instantiate(clone, transform.position, Quaternion.identity);

            //TODO: look into Sprite.PhysicsShape

            clon.GetComponent<SpriteShatterPiece>().Init(_sprite.sprite, t);
        }
    }
}