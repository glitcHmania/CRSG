using System;
using UnityEngine;

public class Timer
{
    public float Duration;
    private float _startTime;
    private bool _isRunning;
    private Action _onFinish;

    public Timer(float duration, Action onFinish = null)
    {
        Duration = duration;
        _startTime = Time.time;
        _isRunning = true;
        _onFinish = onFinish;
    }

    public bool IsRunning => _isRunning;

    public void Update()
    {
        if (!_isRunning) return;

        if (Time.time - _startTime >= Duration)
        {
            _isRunning = false;
            _onFinish?.Invoke();
        }
    }

    public void Reset()
    {
        _startTime = Time.time;
        _isRunning = true;
    }
}
