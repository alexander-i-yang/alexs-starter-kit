using UnityEngine;

namespace ASK.Runtime.Phys2D.Behaviors
{
    public interface ISquishBehavior
    {
        public bool Squish(PhysObj physObj, Vector2 direction);
    }
}