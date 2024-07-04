using UnityEngine;

namespace ASK.Runtime.Phys2D.Behaviors
{
    public class WallCollisionBehavior : ICollisionBehavior
    {
        public PhysState OnCollide(PhysState physState, PhysObj physObj, Vector2 direction)
        {
            Wall wall = physObj.GetComponent<Wall>();
            if (wall != null) physState.Collided = true;
            return physState;
        }
    }
}