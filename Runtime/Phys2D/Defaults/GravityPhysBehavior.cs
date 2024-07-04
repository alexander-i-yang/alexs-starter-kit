using System;
using System.Collections.Generic;
using System.Linq;
using ASK.Core;
using ASK.Runtime.Phys2D.Behaviors;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Defaults
{
    [Serializable]
    public class GravityPhysBehavior : IPhysBehavior
    {
        [SerializeField] protected float GravityDown;
        [SerializeField] protected float GravityUp;
        [SerializeField] protected float MaxFall;

        public PhysState ProcessSurroundings(PhysState p, Dictionary<Vector2, PhysObj[]> surroundings)
        {
            var surroundingsDown = surroundings[Vector2.down];
            
            p.grounded = ComputeGrounded(surroundingsDown);
            if (!p.grounded) p.velocity.y = Fall(p.velocity.y);
            return p;
        }

        public bool ComputeGrounded(PhysObj[] surroundings)
        {
            var grounds = surroundings.Select(x => x.GetProperty<Ground>());
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