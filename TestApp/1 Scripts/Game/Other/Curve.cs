using DesertImage.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Game
{
    public readonly struct Curve
    {
        private readonly BufferList<float> _data;
        private readonly float2 _timeRange;

        public float2 TimeRange => _timeRange;

        public Curve(BufferList<float> data, AnimationCurve curve, int intervalCount)
        {
            _data = data;

            var timeFrom = curve.keys[0].time;
            var timeTo = curve.keys[^1].time;
            var timeStep = (timeTo - timeFrom) / intervalCount;

            for (var i = 0; i < intervalCount + 1; i++)
            {
                _data[i] = curve.Evaluate(timeFrom + i * timeStep);
            }

            _timeRange = new float2(timeFrom, timeTo);
        }

        public float Evaluate(float time)
        {
            var intervalCount = _data.Capacity - 1;

            var clamped = math.unlerp(_timeRange.x, _timeRange.y, math.clamp(time, _timeRange.x, _timeRange.y));
            var timeInInterval = clamped * intervalCount;
            var segmentIndex = (int)math.floor(timeInInterval);
            
            if (segmentIndex >= intervalCount) return _data.Read(intervalCount);
            
            var bottom = _data.Read(segmentIndex);
            var top = _data.Read(segmentIndex + 1);

            return math.lerp(bottom, top, timeInInterval - segmentIndex);
        }
    }
}