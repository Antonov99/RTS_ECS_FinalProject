using System;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace TimeManagement
{
    [Serializable]
    public class Timer : IPlayable, IPausable, IProgressable, IEndable, ITickable
    {
        public enum State
        {
            IDLE = 0,
            PLAYING = 1,
            PAUSED = 2,
            ENDED = 3
        }
        
        public event Action OnStarted;
        public event Action OnStopped;
        public event Action OnPaused;
        public event Action OnResumed;
        public event Action OnEnded;

        public event Action<State> OnStateChanged;

        public event Action<float> OnCurrentTimeChanged;
        public event Action<float> OnDurationChanged;
        public event Action<float> OnProgressChanged;

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        public State CurrentState
        {
            get { return _currentState; }
        }

#if ODIN_INSPECTOR
        [ShowInInspector, HideInEditorMode]
#endif
        public float Duration
        {
            get { return duration; }
            set { SetDuration(value); }
        }

#if ODIN_INSPECTOR
        [ShowInInspector, HideInEditorMode]
#endif
        public float CurrentTime
        {
            get { return _currentTime; }
            set { SetCurrentTime(value); }
        }

#if ODIN_INSPECTOR
        [ShowInInspector, HideInEditorMode]
#endif
        public float Progress
        {
            get { return GetProgress(); }
            set { SetProgress(value); }
        }

#if ODIN_INSPECTOR
        [ShowInInspector, HideInEditorMode]
#endif
        public bool Loop
        {
            get { return loop; }
            set { loop = value; }
        }

#if ODIN_INSPECTOR
        [HideInPlayMode]
#endif
        [SerializeField]
        private float duration;

#if ODIN_INSPECTOR
        [HideInPlayMode]
#endif
        [SerializeField]
        private bool loop;

        private float _currentTime;
        private State _currentState;

        
        public Timer()
        {
        }

        public Timer(float duration, bool loop = false)
        {
            this.duration = duration;
            this.loop = loop;
        }
        
        public State GetCurrentState() => _currentState;
        public bool IsIdle() => _currentState == State.IDLE;
        public bool IsPlaying() => _currentState == State.PLAYING;
        public bool IsPaused() => _currentState == State.PAUSED;
        public bool IsEnded() => _currentState == State.ENDED;

        public float GetDuration() => duration;
        public float GetCurrentTime() => _currentTime;

#if ODIN_INSPECTOR
        [Button]
#endif
        public void ForceStart()
        {
            Stop();
            Start();
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public void ForceStart(float currentTime)
        {
            Stop();
            Start(currentTime);
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public bool Start()
        {
            if (_currentState is not (State.IDLE or State.ENDED))
            {
                return false;
            }

            _currentTime = 0;
            _currentState = State.PLAYING;
            OnStateChanged?.Invoke(State.PLAYING);
            OnStarted?.Invoke();
            return true;
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public bool Start(float currentTime)
        {
            if (_currentState is not (State.IDLE or State.ENDED))
            {
                return false;
            }

            _currentTime = Mathf.Clamp(currentTime, 0, duration);
            _currentState = State.PLAYING;
            OnStateChanged?.Invoke(State.PLAYING);
            OnStarted?.Invoke();
            return true;
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public bool Pause()
        {
            if (_currentState != State.PLAYING)
            {
                return false;
            }

            _currentState = State.PAUSED;
            OnStateChanged?.Invoke(State.PAUSED);
            OnPaused?.Invoke();
            return true;
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public bool Resume()
        {
            if (_currentState != State.PAUSED)
            {
                return false;
            }

            _currentState = State.PLAYING;
            OnStateChanged?.Invoke(State.PLAYING);
            OnResumed?.Invoke();
            return true;
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public bool Stop()
        {
            if (_currentState == State.IDLE)
            {
                return false;
            }

            _currentTime = 0;
            _currentState = State.IDLE;
            OnStateChanged?.Invoke(State.IDLE);
            OnStopped?.Invoke();
            return true;
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public void Tick(float deltaTime)
        {
            if (_currentState != State.PLAYING)
            {
                return;
            }

            _currentTime = Mathf.Min(duration, _currentTime + deltaTime);
            OnCurrentTimeChanged?.Invoke(_currentTime);

            float progress = _currentTime / duration;
            OnProgressChanged?.Invoke(progress);

            if (progress >= 1)
            {
                Complete();
            }
        }
        
        private void Complete()
        {
            _currentState = State.ENDED;
            OnStateChanged?.Invoke(State.ENDED);
            OnEnded?.Invoke();

            if (loop)
            {
                Start();
            }
        }
        
        public float GetProgress()
        {
            return _currentState switch
            {
                State.PLAYING or State.PAUSED => _currentTime / duration,
                State.ENDED => 1,
                _ => 0
            };
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            _currentTime = duration * progress;
            OnCurrentTimeChanged?.Invoke(_currentTime);
            OnProgressChanged?.Invoke(progress);
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public void SetDuration(float newDuration)
        {
            if (newDuration < 0)
            {
                return;
            }

            if (Math.Abs(duration - newDuration) > float.Epsilon)
            {
                duration = newDuration;
                OnDurationChanged?.Invoke(newDuration);
            }
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public void SetCurrentTime(float time)
        {
            if (time < 0)
            {
                return;
            }

            float newTime = Mathf.Clamp(time, 0, duration);
            if (Math.Abs(newTime - _currentTime) > float.Epsilon)
            {
                _currentTime = newTime;
                OnCurrentTimeChanged?.Invoke(newTime);
                OnProgressChanged?.Invoke(GetProgress());
            }
        }
    }
}