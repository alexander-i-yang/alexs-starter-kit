using UnityEngine;
using System;

namespace ASK.Core
{
    public class TimeManager : MonoBehaviour
    {
        private float _timeScale;
        public float TimeScale
        {
            get { return _timeScale; }
            public set
            {
                _timeScale = value;
                OnTimeScaleChange?.Invoke();
            }
        };
        
        public event Action OnTimeScaleChange;
        

        public float DeltaTime { get; private set; }
        public float FixedDeltaTime { get; private set; }
        public float Time { get; private set; } = 0;
        
        private int _frameCount;
        
        [SerializeField] private int stepFrames;
        public bool DebugBreak;
        
        public delegate void ResetNFOAction();
        public event ResetNFOAction ResetNextFrameOffset;

        private void Update()
        {
            DeltaTime = Game.Instance.IsPaused ? 0 : UnityEngine.Time.deltaTime * TimeScale;
            Time += DeltaTime;
        }
        
        private void FixedUpdate()
        {
            if (ResetNextFrameOffset != null) ResetNextFrameOffset();
            FixedDeltaTime = Game.Instance.IsPaused ? 0 : UnityEngine.Time.fixedDeltaTime * TimeScale;
            
            if (DebugBreak && _frameCount % stepFrames == 0) Debug.Break();
            _frameCount = (_frameCount + 1) % 10000;
        }
    }
}