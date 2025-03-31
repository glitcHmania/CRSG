using System;
using UnityEngine;

public class Timer
{
    public float Duration;
    public bool IsFinished { get; private set; }
    public float RemainingTime => Duration - (Time.time - _startTime);
    private float _startTime;
    private Action _onFinish;

    public Timer(float duration, Action onFinish = null)
    {
        Duration = duration;
        _startTime = Time.time;
        _onFinish = onFinish;
    }

    public void Start()
    {
        _startTime = Time.time;
    }

    public void Update()
    {
        if (Time.time - _startTime >= Duration)
        {
            IsFinished = true;
            _onFinish?.Invoke();
        }
    }

    public void Reset()
    {
        _startTime = Time.time;
        IsFinished = false;
    }
}
