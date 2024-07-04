using ASK.Runtime.Phys2D.Behaviors;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Defaults
{
    public class WallCollisionBehavior : ICollisionBehavior
    {
        public PhysState OnCollide(PhysState physState, PhysObj physObj, Vector2 direction)
        {
            Wall wall = physObj.GetComponent<Wall>();
            if (wall != null) physState.collided = true;
            return physState;
        }
    }
}