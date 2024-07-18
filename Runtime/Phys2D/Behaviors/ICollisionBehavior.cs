using UnityEngine;

namespace ASK.Runtime.Phys2D.Behaviors
{
    public abstract class CollisionBehavior : MonoBehaviour
    {
        public abstract PhysState OnCollide(PhysState physState, PhysObj physObj, Vector2 direction);
    }
}