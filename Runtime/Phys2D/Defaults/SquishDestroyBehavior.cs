using UnityEngine;

namespace ASK.Runtime.Phys2D.Modules
{
    public class SquishDestroyBehavior : MonoBehaviour, ISquishBehavior
    {
        public bool Squish(PhysObj physObj, Vector2 direction)
        {
            Destroy(gameObject);
            return true;
        }
    }
}