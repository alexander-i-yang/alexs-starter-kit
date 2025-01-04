using System;
using System.Net.NetworkInformation;
using ASK.Runtime.Phys2D.Behaviors;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Defaults
{
    public class BonkHeadBehavior : ICollisionBehavior
    {
        public int BonkHeadVelocity = 0;
        
        public PhysState OnCollide(PhysState physState, PhysObj physObj, Vector2 direction)
        {
            if (direction.y <= 0) return physState;
            if (physObj.GetProperty<Ground>() != null)
                physState.velocity.y = Math.Min(physState.velocity.y, BonkHeadVelocity);
            return physState;
        }
    }
}