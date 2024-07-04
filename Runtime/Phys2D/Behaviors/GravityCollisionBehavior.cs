using UnityEngine;

namespace ASK.Runtime.Phys2D.Behaviors
{
    public class GravityCollisionBehavior : ICollisionBehavior
    {
        public override PhysState OnCollide(PhysState physState, PhysObj physObj, Vector2 direction)
        {
            if (direction.y >= 0) return physState;
            Wall g = physObj.GetComponent<Wall>();
            if (g != null)
            {
                physState.Collided = true;
                physState.velocity.y = 0;
                physState.Grounded = true;
            }

            return physState;
        }
    }
}