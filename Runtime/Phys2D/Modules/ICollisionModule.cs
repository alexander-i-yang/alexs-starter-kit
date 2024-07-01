using UnityEngine;

namespace ASK.Runtime.Phys2D.Modules
{
    public abstract class ICollisionModule : MonoBehaviour
    {
        public abstract PhysState OnCollide(PhysState physState, PhysObj physObj, Vector2 direction);
    }
}