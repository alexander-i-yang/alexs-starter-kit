using ASK.Runtime.Phys2D.Behaviors;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Defaults
{
    public class WallCollisionBehavior : CollisionBehavior
    {
        public override PhysState OnCollide(PhysState physState, PhysObj physObj, Vector2 direction)
        {
            Ground ground = physObj.GetProperty<Ground>();
            if (ground != null) physState.collided = true;
            return physState;
        }
    }
}