using ASK.Runtime.Phys2D.Behaviors;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Defaults
{
    public class DefaultSquishBehavior : MonoBehaviour, ISquishBehavior
    {
        public bool Squish(PhysObj physObj, Vector2 direction) => false;
    }
}