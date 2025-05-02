using System;
using UnityEngine;

public class Timer
{
    public float RemainingTime { get; private set; }
    public float Elapsed { get; private set; }
    public float Duration { get; private set; }
    public bool IsFinished { get; private set; }

    private float _startTime;
    private Action _onFinish;
    private bool _hasStarted;

    public Timer(float duration, Action onFinish = null, bool startFinished = false)
    {
        Duration = duration;
        _onFinish = onFinish;
        RemainingTime = duration;
        IsFinished = startFinished;
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

        Elapsed = Time.time - _startTime;
        RemainingTime = Mathf.Max(0f, Duration - Elapsed);

        if (Elapsed >= Duration)
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

    public float GetRatio()
    {
        return Mathf.Clamp01(1f - RemainingTime / Duration);
    }
}

public class Chronometer
{
    public float Elapsed { get; private set; }
    public bool IsRunning { get; private set; }

    public Chronometer()
    {
        Elapsed = 0f;
        IsRunning = false;
    }

    public void Start()
    {
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    public void Reset()
    {
        Elapsed = 0f;
    }

    public void Restart()
    {
        Reset();
        Start();
    }

    public void Update()
    {
        if(!IsRunning)
        {
            IsRunning = true;
        }

        if (IsRunning)
        {
            Elapsed += Time.deltaTime;
        }
    }
}
