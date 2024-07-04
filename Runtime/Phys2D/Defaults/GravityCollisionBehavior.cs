using ASK.Runtime.Phys2D.Behaviors;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Defaults
{
    public class GravityCollisionBehavior : ICollisionBehavior
    {
        public PhysState OnCollide(PhysState physState, PhysObj physObj, Vector2 direction)
        {
            if (direction.y >= 0) return physState;
            Wall g = physObj.GetComponent<Wall>();
            if (g != null)
            {
                physState.collided = true;
                physState.velocity.y = 0;
                physState.grounded = true;
            }

            return physState;
        }
    }
}