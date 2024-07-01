using System;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Modules
{
    [Serializable]
    public abstract class IPhysModule : MonoBehaviour
    {
        public abstract PhysState ProcessSurroundings(PhysState p, PhysObj[] surroundings, Vector2 direction);
    }
}