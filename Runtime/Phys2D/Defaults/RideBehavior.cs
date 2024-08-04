using System.Collections.Generic;
using System.Linq;
using ASK.Runtime.Core;
using ASK.Runtime.Phys2D.Behaviors;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Defaults
{
    public class RideBehavior : IPhysBehavior
    {
        /**
         * When there was a floor but now there's not
         */
        // protected bool JumpedOff(PhysState p) => p.prevRidingOn != null && p.ridingOn == null;

        /**
         * When the floor was moving but now it's not
         */
        // protected bool FloorStopped(PhysState p) => p.prevRidingV != Vector2.zero && p.ridingOn != null && p.ridingOn.velocity == Vector2.zero;
        
        public PhysState ProcessSurroundings(PhysState p, Dictionary<Direction, PhysObj[]> surroundings)
        {
            p.ridingOn = Riding(surroundings[Direction.Down]);
            return p;
        }

        public PhysObj[] Riding(PhysObj[] floors) =>
            floors.Where(p => p.GetProperty<Ridable>() != null).ToArray();
    }
}