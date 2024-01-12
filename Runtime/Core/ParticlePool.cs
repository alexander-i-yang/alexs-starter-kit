using UnityEngine;

namespace ASK.Core
{
    public class ParticlePool : MonoBehaviour
    {
        private Transform _child;
        
        void Awake()
        {
            _child = new GameObject("Particle Pool").transform;
            _child.parent = transform;
        }

        public void ReceiveParticle(Transform t)
        {
            t.parent = _child;
        } 
    }
}