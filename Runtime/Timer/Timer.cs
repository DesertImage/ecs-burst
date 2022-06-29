using System;
using DesertImage.ECS;
using DesertImage.Managers;
using Managers;

namespace DesertImage.Timers
{
    public class Timer : ITimer
    {
        public event Action<ITimer> OnFinish;
        public event Action<ITimer> OnDispose;

        public int Id { get; }
        public float Time { get; private set; }

        private static int _timersIdCounter;

        private float _targetTime;
        private Action _action;

        private bool _isPlaying;

        private bool _isIgnoreTimescale;

        public Timer()
        {
            Id = _timersIdCounter++;
        }

        public virtual void Tick()
        {
            if (!_isPlaying) return;

            Time += _isIgnoreTimescale ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;

            if (Time < _targetTime) return;

            _isPlaying = false;

            _action.Invoke();

            _action = null;

            Completed();

            ReturnToPool();
        }

        protected void Completed()
        {
            OnFinish?.Invoke(this);
        }

        #region PLAY / STOP / RESET

        public void Play(Action action, float timeDelay = 1f, bool ignoreTimeScale = false)
        {
            _isPlaying = true;

            _action = action;

            _targetTime = timeDelay;

            _isIgnoreTimescale = ignoreTimeScale;
        }

        public void Play(Action<Timer> action, float timeDelay = 1f, bool ignoreTimeScale = false)
        {
            _isPlaying = true;

            _action = () => action?.Invoke(this);

            _targetTime = timeDelay;

            _isIgnoreTimescale = ignoreTimeScale;
        }

        public void Stop()
        {
            _isPlaying = false;

            Reset();
        }

        public void PlayAndReturnToPool()
        {
            if (!_isPlaying) return;

            _action?.Invoke();

            ReturnToPool();
        }

        private void Reset()
        {
            Time = 0f;

            _action = null;

            _targetTime = 0.3f;
        }

        #endregion

        #region POOL STUFF

        public void OnCreate()
        {
            Reset();

            Core.Instance?.Get<TimersUpdater>()?.Add(this);
        }

        public void ReturnToPool()
        {
            Stop();

            Core.Instance?.Get<TimersUpdater>().Remove(this);
            Core.Instance?.Get<ManagerTimers>().ReturnInstance(this);
        }

        #endregion

        public void Dispose()
        {
            OnDispose?.Invoke(this);
            //TODO: return to pool by manager
        }
    }
}