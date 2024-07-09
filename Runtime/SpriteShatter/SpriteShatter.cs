using System;
using System.Linq;
using ASK.Editor.Standalone;
using ASK.Runtime.Helpers;
using UnityEngine;
using MyBox;
using TriangleNet.Topology;
using UnityEngine.Serialization;

namespace ASK.Runtime.SpriteShatter
{
    public class SpriteShatter : MonoBehaviour
    {
        [SerializeField] private GameObject clone;

        private SpriteRenderer _sprite;

        public Vector2[] Mesh;

        [SerializeField] private float MaxTriangleArea = 2;

        [SerializeField] private Vector2 testForce;
        [SerializeField] private float d_ForceMagnitude;
        [SerializeField] private Vector2 testForcePos;

        [SerializeField]
        [Tooltip("Debug only.")]
        private Triangle[] d_triangles;

        [SerializeReference]
        [ChilrdenClassesDropdown(typeof(ISpriteShatterVBehavior))]
        private ISpriteShatterVBehavior spriteShatterVBehavior;
        
        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
        }

        public void Shatter()
        {
            _sprite.enabled = false;
            var triangulated = Triangulator.TriangulateAndNormalize(Mesh, MaxTriangleArea).ToArray();
            var pieces = triangulated.Select(CreatePiece).ToArray();
            d_triangles = triangulated;
            for (int i = 0; i < triangulated.Length; ++i)
            {
                pieces[i].ApplyForce(ResolveForces(Vector2.zero, (Vector2)transform.position + testForcePos, triangulated[i]));
            }
        }

        public IShatterPiece CreatePiece(Triangle t)
        {
            var clon = Instantiate(clone, transform.position, Quaternion.identity);

            //TODO: look into Sprite.PhysicsShape

            var piece = clon.GetComponent<IShatterPiece>();
            piece.Init(_sprite.sprite, t);

            return piece;
        }
        
        /// <summary>
        /// Triangles are normalized according to sprite coordinates.
        /// </summary>
        /// <param name="force"></param>
        /// <param name="forcePos"></param>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public Vector2 ResolveForces(Vector2 force, Vector2 forcePos, Triangle triangle)
        {
            Vector2 normalizedPos = SpriteShatterPiece.Normalize(_sprite.sprite, (Vector2)transform.position - forcePos);
            Vector2 trianglePos = triangle.Center();
            return d_ForceMagnitude*(normalizedPos - trianglePos);
        }
    }
}