using System;
using System.Collections.Generic;
using System.Linq;
using ASK.Core;
using ASK.Runtime.Phys2D.Behaviors;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

namespace ASK.Runtime.Phys2D.Defaults
{
    [Serializable]
    public class GravityPhysBehavior : IPhysBehavior
    {
        [SerializeField]
        [PositiveValueOnly]
        protected float gravityDown;
        
        [PositiveValueOnly]
        [SerializeField] protected float gravityUp;
        
        [PositiveValueOnly]
        [SerializeField]
        protected float maxFall;

        /*[SerializeField]
        protected Vector2 direction;*/

        public PhysState ProcessSurroundings(PhysState p, Dictionary<Direction, PhysObj[]> surroundings)
        {
            var surroundingsDown = surroundings[Direction.Down];
            
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
            return Math.Max(-maxFall, vy - EffectiveGravity(vy) * Game.TimeManager.FixedDeltaTime);
        }

        protected float EffectiveGravity(float velocityY) => (velocityY > 0 ? gravityUp : gravityDown);

        public void FlipGravity()
        {
            throw new NotImplementedException();
            gravityDown *= -1;
            gravityUp *= -1;
        }
    }
}