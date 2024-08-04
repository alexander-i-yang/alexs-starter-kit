using System.Collections.Generic;
using System.Linq;
using ASK.Core;
using ASK.Runtime.Helpers;
using Clipper2Lib;
using MyBox;
using TriangleNet.Topology;
using UnityEngine;
using UnityEngine.Serialization;

namespace ASK.Runtime.SpriteShatter
{
    public abstract class IShatterPiece : Particle
    {
        public abstract void Init(Sprite sprite, SpriteShatterGroup groupData);
        public abstract void ApplyForce(Vector2 force);
        public abstract Triangle[] GetTriangles();
    }

    public class SpriteShatterParent : IShatterPiece
    {
        private static readonly int A = Shader.PropertyToID("_TriangleA");
        private static readonly int B = Shader.PropertyToID("_TriangleB");
        private static readonly int C = Shader.PropertyToID("_TriangleC");

        public GameObject spriteShatterPiece;

        private Triangle[] _triangles;

        private Rigidbody2D _rb;

        [SerializeField]
        private float delayFade;
        
        [FormerlySerializedAs("delayTime")] [SerializeField]
        private float fadeTime;

        private float awakeTime;
        private List<SpriteRenderer> _srs;
        
        public override void Init(Sprite sprite, SpriteShatterGroup groupData)
        {
            _rb = GetComponent<Rigidbody2D>();
            _triangles = groupData.Triangles.ToArray();
            gameObject.SetActive(true);
            //Paths64 paths = new Paths64();
            _srs = new List<SpriteRenderer>();

            foreach (var triangle in _triangles)
            {
                var newObj =
                    Instantiate(spriteShatterPiece, transform.position, Quaternion.identity, transform);
                
                var col = newObj.GetComponent<PolygonCollider2D>();
                col.points = triangle.Points().ToArray();
            
                var sr = newObj.GetComponent<SpriteRenderer>();
                _srs.Add(sr);
                sr.sprite = sprite;

                var normaA = Triangulator.Normalize(sprite, triangle.A).ToVector2();
                var normaB = Triangulator.Normalize(sprite, triangle.B).ToVector2();
                var normaC = Triangulator.Normalize(sprite, triangle.C).ToVector2();
                
                sr.material.SetVector(A, normaA);
                sr.material.SetVector(B, normaB);
                sr.material.SetVector(C, normaC);
            }

            ApplyForce(groupData.velocity);
            
            awakeTime = Game.TimeManager.Time;
        }

        private void FixedUpdate()
        {
            if (_rb == null) return;
            
            if (_rb.velocity.magnitude < 0.0001f)
                _rb.velocity = Vector2.zero;
        }

        void Update()
        {
            var t = ASK.Core.Game.TimeManager.Time;
            if (t - awakeTime > delayFade)
            {
                float i = ((t - awakeTime) - delayFade)/fadeTime;
                if (i > 1)
                {
                    //Destroy(gameObject);
                    _srs.ForEach(sr => sr.gameObject.SetActive(false));
                    gameObject.SetActive(false);
                }
                else
                {
                    _srs.ForEach(sr => sr.SetAlpha(1 - i));
                }
            }
        }

        public override void ApplyForce(Vector2 force)
        {
            _rb.AddForce(force, ForceMode2D.Impulse);
        }

        public override Triangle[] GetTriangles() => _triangles;
        public override bool IsActive() => gameObject.activeSelf;
    }
}