using System;
using System.Collections.Generic;
using ASK.Core;
using UnityEngine;

namespace ASK.Core
{
    public class Timescaler : MonoBehaviour
    {
        [Serializable]
        public struct TimeScale
        {
            public float Val;
            public int Priority;

            public TimeScale(float v, int p)
            {
                Val = v;
                Priority = p;
            }
        }
        
        public class TimeScaleComparer : IComparer<TimeScale>
        {
            public int Compare(TimeScale x, TimeScale y)
            {
                return x.Priority - y.Priority;
            }
        }

        [SerializeField]
        private OrderedList<TimeScale> _scales = new(new TimeScaleComparer());
        
        public TimeScale ApplyTimescale(float scale, int priority)
        {
            var ret = new TimeScale(scale, priority);
            _scales.Add(ret);
            return ret;
        }

        public void RemoveTimescale(TimeScale t)
        {
            _scales.Remove(t);
        }

        public float Working => _scales[_scales.Count - 1].Val;
    }
}
