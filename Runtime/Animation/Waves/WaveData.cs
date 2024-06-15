using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace ASK.Animation
{
    public enum WaveType
    {
        Sine,
        Cosine,
        Square,
        Custom,
    };
    
    [Serializable]
    public class WaveData
    {
        public float Amplitude = 1;
        public float Frequency = 1;
        [InspectorName("WaveType")] public WaveType WaveType = WaveType.Sine;
        
        public string Formula = "sin(x)";

        private Func<float, float> waveFunc => (t) =>
        {
            if (this.WaveType == WaveType.Sine) return Mathf.Sin(t);
            if (this.WaveType == WaveType.Cosine) return Mathf.Cos(t);
            if (this.WaveType == WaveType.Square) return Mathf.Sign(t % 2 - 1);
            if (this.WaveType == WaveType.Custom)
            {
                ExpressionEvaluator.Evaluate(Formula.Replace("x", $"({t.ToString("0.000")})"), out float result);
                return result;
            };
            return Mathf.Sin(t);
        };

        public Vector3[] GetPoints(int start, float end, Vector2 offset = default, Vector2 scale = default)
        {
            if (offset == default) offset = Vector2.zero;
            if (scale == default) scale = Vector2.one;
            if (end < start || float.IsNaN(end) || float.IsNaN(start)) return new [] {Vector3.zero};
            try
            {
                return Enumerable.Range(start, (int)end)
                    .Select(x => new Vector2(x, Evaluate(x, scale, offset)))
                    .Select(x => (Vector3)x)
                    .ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return new [] {Vector3.zero};
            }
        }

        /// <summary>
        /// Gets the last point less than thresh that is still a whole number of cycles.
        /// </summary>
        /// <param name="thresh">Cycle should be under this number.</param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public float GetWholeCyclesUntil(float thresh, float scale) {
            float a = Frequency / scale / Mathf.PI / 2;
            return Mathf.Floor(thresh * a) / a;
        }

        public float Evaluate(float time) => Evaluate(time, Vector2.one, Vector2.zero);

        /// <summary>
        /// Evaluate the output of the wave at t=time.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="scale"></param>
        /// <param name="offset">Applied after scale.</param>
        /// <returns></returns>
        public float Evaluate(float time, Vector2 scale, Vector2 offset)
        {
            return this.waveFunc(time * Frequency / scale.x) * Amplitude * scale.y + offset.y;
        }
    }
}