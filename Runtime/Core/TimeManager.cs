using UnityEngine;
using System;

namespace ASK.Core
{
    [RequireComponent(typeof(Timescaler))]
    public class TimeManager : MonoBehaviour
    {
        // public virtual float TimeScale => _timescaler.Working;

        public virtual float GetTimeScale() => _timescaler.Working;

        public float DeltaTime { get; private set; }
        public float FixedDeltaTime { get; private set; }
        public float Time { get; private set; } = 0;
        
        private int _frameCount;
        
        [SerializeField] private int stepFrames;
        public bool DebugBreak;
        
        public delegate void ResetNFOAction();
        public event ResetNFOAction ResetNextFrameOffset;
        public event Action OnTimeScaleChange;

        private Timescaler _timescaler;

        private void Awake()
        {
            _timescaler = GetComponent<Timescaler>();
            _timescaler.ApplyTimescale(1, -1);
        }

        private void Update()
        {
            DeltaTime = Game.Instance.IsPaused ? 0 : UnityEngine.Time.deltaTime * GetTimeScale();
            Time += DeltaTime;
        }
        
        private void FixedUpdate()
        {
            if (ResetNextFrameOffset != null) ResetNextFrameOffset();

            float fixedDeltaTime = UnityEngine.Time.fixedDeltaTime * GetTimeScale();
            FixedDeltaTime = Game.Instance.IsPaused ? 0 : fixedDeltaTime;
            
            if (DebugBreak && _frameCount % stepFrames == 0) Debug.Break();
            _frameCount = (_frameCount + 1) % 10000;

            if (Physics2D.simulationMode == SimulationMode2D.Script) Physics2D.Simulate(fixedDeltaTime);
        }

        public virtual Timescaler.TimeScale ApplyTimescale(float f, int priority)
        {
            OnTimeScaleChange?.Invoke();
            return _timescaler.ApplyTimescale(f, priority);
        }

        public virtual void RemoveTimescale(Timescaler.TimeScale t)
        {
            OnTimeScaleChange?.Invoke();
            _timescaler.RemoveTimescale(t);
        }
    }
}