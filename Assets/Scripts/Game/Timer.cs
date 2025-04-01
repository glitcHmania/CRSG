using System;
using UnityEngine;

public class Timer
{
    public float Duration;
    public bool IsFinished { get; private set; }
    public float RemainingTime { get; private set; }

    private float _startTime;
    private Action _onFinish;
    private bool _hasStarted;

    public Timer(float duration, Action onFinish = null)
    {
        Duration = duration;
        _onFinish = onFinish;
        RemainingTime = duration;
        IsFinished = false;
        _hasStarted = false;
    }

    public void Update()
    {
        if (!_hasStarted)
        {
            _startTime = Time.time;
            _hasStarted = true;
        }

        if (IsFinished) return;

        float elapsed = Time.time - _startTime;
        RemainingTime = Mathf.Max(0f, Duration - elapsed);

        if (elapsed >= Duration)
        {
            IsFinished = true;
            _onFinish?.Invoke();
        }
    }

    public void Reset()
    {
        RemainingTime = Duration;
        IsFinished = false;
        _hasStarted = false;
    }
}
