using UnityEngine;

namespace ASK.Runtime.Phys2D.Behaviors
{
    public interface ICollisionBehavior
    {
        public PhysState OnCollide(PhysState physState, PhysObj physObj, Vector2 direction);
    }
}