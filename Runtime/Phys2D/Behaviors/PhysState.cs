using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ASK.Runtime.Phys2D.Behaviors
{
    [Serializable]
    public class PhysState
    {
        public bool collided;
        public Vector2 velocity;
        public bool grounded;

        public PhysObj[] ridingOn;
        public float stun;
        public float inv;
    }
}