using System.Collections.Generic;
using System.Linq;
using ASK.Runtime.Phys2D.Behaviors;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Defaults
{
    public class DefaultRideBehavior : IPhysBehavior
    {
        /**
         * When there was a floor but now there's not
         */
        // protected bool JumpedOff(PhysState p) => p.prevRidingOn != null && p.ridingOn == null;

        /**
         * When the floor was moving but now it's not
         */
        // protected bool FloorStopped(PhysState p) => p.prevRidingV != Vector2.zero && p.ridingOn != null && p.ridingOn.velocity == Vector2.zero;
        
        public PhysState ProcessSurroundings(PhysState p, Dictionary<Vector2, PhysObj[]> surroundings)
        {
            p.ridingOn = Riding(surroundings[Vector2.down]);
            return p;
        }

        public PhysObj[] Riding(PhysObj[] floors) =>
            floors.Where(p => p.GetComponent<Ground>() != null).ToArray();
    }
}