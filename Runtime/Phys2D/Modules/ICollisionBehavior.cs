using UnityEngine;

namespace ASK.Runtime.Phys2D.Modules
{
    public abstract class ICollisionBehavior : MonoBehaviour
    {
        public abstract PhysState OnCollide(PhysState physState, PhysObj physObj, Vector2 direction);
    }
}