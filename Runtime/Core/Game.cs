using ASK.ScreenShake;
using UnityEngine;
using MyBox;

namespace ASK.Core
{
    [RequireComponent(typeof(CameraController), typeof(TimeManager))]
    public class Game : Singleton<Game>
    {
        [SerializeField] private string musicName;
        public bool IsPaused;
        
        public Vector2 ScreenSize = new Vector2(256, 144);

        public int FakeControlsArrows = -2;

        public delegate void OnDebug();
        public event OnDebug OnDebugEvent;

        // public static float Time => Instance._timeManager.Time;
        private TimeManager _timeManager;
        public static TimeManager TimeManager => Instance == null ? null : Instance._timeManager;

        private bool _isDebug;
        public bool IsDebug
        {
            get => _isDebug;
            set
            {
                _isDebug = value;
                if (value)
                {
                    OnDebugEvent?.Invoke();
                }
            }
        }

        public CameraController CamController;
        
        // public AudioClip music;

        void Awake()
        {
            Application.targetFrameRate = 60;
            InitializeSingleton();

            // FMODUnity.RuntimeManager.LoadBank("Master", true);
            CamController = GetComponent<CameraController>();
            _timeManager = GetComponent<TimeManager>();
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}