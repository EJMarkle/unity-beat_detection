using System;

public static class BeatEventSystem
{
    public static event EventHandler<BeatEventArgs> OnBeat;

    public static void RaiseBeat(float intensity, BeatType beatType, float frequencyEnergy)
    {
        OnBeat?.Invoke(null, new BeatEventArgs(intensity, beatType, frequencyEnergy));
    }
}
