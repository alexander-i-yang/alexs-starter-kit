using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace ASK.Core
{
    public abstract class Particle : MonoBehaviour
    {
        public abstract bool IsActive();

        public void SetActive(bool b)
        {
            gameObject.SetActive(b);
        }
    }

    public class ParticlePool : MonoBehaviour
    {
        private Dictionary<Type, Pool> _pools = new();

        public T ReceiveParticle<T>(Func<T> createParticle, Action<T> initParticle) where T : Particle
        {
            System.Type t = typeof(T);
            
            if (!_pools.ContainsKey(t))
            {
                _pools.Add(t, new Pool());
            }
            
            return _pools[typeof(T)].Spawn<T>(createParticle, initParticle);
        }
        
        public class Pool
        {
            private Queue<Particle> _particles = new();
        
            public T Spawn<T>(Func<T> createParticle, Action<T> initParticle) where T : Particle
            {
                T ret;
                
                /*var actives = _particles.Where(p => p.IsActive());
                var numActive = actives.Count();

                if (numActive > 10)
                {
                    actives.ForEach(p => p.SetActive(false));
                }*/
                
                foreach (var part in _particles)
                {
                    if (part == null) continue;
                    if (part.IsActive()) continue; 
                    ret = (T)part;
                    initParticle(ret);
                    return ret;
                }
                
                ret = createParticle();
                _particles.Enqueue(ret);
                return ret;
            }
        }
    }
    
    
}