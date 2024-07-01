using UnityEngine;

namespace ASK.Runtime.Phys2D.Modules
{
    public class DefaultSquishBehavior : MonoBehaviour, ISquishBehavior
    {
        public bool Squish(PhysObj physObj, Vector2 direction) => false;
    }
}