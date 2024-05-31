using System;

public class TimerService : Service
{
    public event Action<float> OnTimerUpdate;

    public override void OnUpdate(float delta)
    {
        OnTimerUpdate?.Invoke(delta);
    }
}
