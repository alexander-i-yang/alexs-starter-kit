using System;
using System.Linq;
using ASK.Core;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Modules
{
    [Serializable]
    public class GravityModule : IPhysModule
    {
        [SerializeField] protected float GravityDown;
        [SerializeField] protected float GravityUp;
        [SerializeField] protected float MaxFall;

        [SerializeField] private bool _grounded;

        public override PhysState ProcessSurroundings(PhysState p, PhysObj[] surroundings, Vector2 direction)
        {
            if (direction.y >= 0) return p;

            p.Grounded = ComputeGrounded(surroundings);
            if (!_grounded) p.velocity.y = Fall(p.velocity.y);
            return p;
        }

        public bool ComputeGrounded(PhysObj[] surroundings)
        {
            var grounds = surroundings.Select(x => x.GetComponent<Wall>());
            return grounds.Any(w => w != null);
        }
        
        public virtual float Fall(float vy) { 
            return Math.Max(MaxFall, vy + EffectiveGravity(vy) * Game.TimeManager.FixedDeltaTime);
        }

        protected float EffectiveGravity(float velocityY) => (velocityY > 0 ? GravityUp : GravityDown);

        public void FlipGravity()
        {
            throw new NotImplementedException();
            GravityDown *= -1;
            GravityUp *= -1;
        }
    }
}