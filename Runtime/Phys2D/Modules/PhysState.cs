using System;
using UnityEngine;

namespace ASK.Runtime.Phys2D.Modules
{
    [Serializable]
    public struct PhysState
    {
        public bool Collided;
        public Vector2 velocity;
        public bool Grounded;
    }
}