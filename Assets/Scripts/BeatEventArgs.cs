public enum BeatType
{
    Low,    // Bass frequencies (20-250 Hz)
    Mid,    // Mid frequencies (250-4000 Hz)
    High    // High frequencies (4000-20000 Hz)
}

public class BeatEventArgs : System.EventArgs
{
    public float Intensity { get; private set; } // 0–100
    public BeatType BeatType { get; private set; }
    public float FrequencyEnergy { get; private set; } // Energy in the specific frequency range

    public BeatEventArgs(float intensity, BeatType beatType, float frequencyEnergy)
    {
        Intensity = intensity;
        BeatType = beatType;
        FrequencyEnergy = frequencyEnergy;
    }
}
