using System;
using System.Collections.Generic;
using ASK.Core;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Behaviors
{
    
    public class FrictionBehavior : IPhysBehavior
    {
        public int GroundedFriction;
        public int AirResistance;
        public PhysState ProcessSurroundings(PhysState p, Dictionary<Direction, PhysObj[]> surroundings)
        {
            Vector2 targetV = new Vector2(0, p.velocity.y);
            p.velocity = Vector2.MoveTowards(p.velocity, targetV, FrictionStep(p.grounded));
            return p;
        }

        public float FrictionStep(bool grounded) => Friction(grounded) * Game.TimeManager.FixedDeltaTime;
        
        public int Friction(bool grounded) => grounded ? GroundedFriction : AirResistance;
    }
}