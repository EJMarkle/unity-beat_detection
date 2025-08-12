using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BeatDetector : MonoBehaviour
{
    public AudioSource source;

    [Header("Detection Settings")]
    [Range(0.1f, 3.0f)]
    public float sensitivity = 1.5f;
    [Range(0.0f, 1.0f)]
    public float adaptiveThreshold = 0.15f;
    [Range(0.05f, 0.5f)]
    public float minCooldown = 0.15f;
    [Range(0.1f, 2.0f)]
    public float maxCooldown = 0.4f;
    
    [Header("Low Frequency Band")]
    [Tooltip("Minimum frequency for low band (bass, kick drums)")]
    [Range(0f, 20000f)]
    public float lowFreqMin = 20f;
    
    [Tooltip("Maximum frequency for low band (bass, kick drums)")]
    [Range(0f, 20000f)]
    public float lowFreqMax = 250f;
    
    [Header("Mid Frequency Band")]
    [Tooltip("Minimum frequency for mid band (snare, vocals, instruments)")]
    [Range(0f, 20000f)]
    public float midFreqMin = 250f;
    
    [Tooltip("Maximum frequency for mid band (snare, vocals, instruments)")]
    [Range(0f, 20000f)]
    public float midFreqMax = 4000f;
    
    [Header("High Frequency Band")]
    [Tooltip("Minimum frequency for high band (cymbals, hi-hats)")]
    [Range(0f, 20000f)]
    public float highFreqMin = 4000f;
    
    [Tooltip("Maximum frequency for high band (cymbals, hi-hats)")]
    [Range(0f, 20000f)]
    public float highFreqMax = 20000f;
    
    [Header("Frequency Range Settings")]
    [Range(0.1f, 5.0f)]
    public float lowFreqWeight = 2.0f;      // Weight for bass detection
    [Range(0.1f, 5.0f)]
    public float midFreqWeight = 1.5f;      // Weight for mid detection
    [Range(0.1f, 5.0f)]
    public float highFreqWeight = 1.0f;     // Weight for high detection
    
    [Header("Frequency Band Thresholds")]
    [Range(1.0f, 3.0f)]
    public float lowBeatThreshold = 1.6f;   // Threshold multiplier for low frequencies
    [Range(1.0f, 3.0f)]
    public float midBeatThreshold = 1.8f;   // Threshold multiplier for mid frequencies
    [Range(1.0f, 3.0f)]
    public float highBeatThreshold = 2.0f;  // Threshold multiplier for high frequencies

    [Header("Advanced Settings")]
    [Range(2, 50)]
    public int historySize = 3;  // Minimal for immediate beat detection startup
    [Range(1, 10)]
    public int minHistoryForDetection = 1;  // Start detecting immediately
    [Range(0.01f, 0.5f)]
    public float varianceWeight = 0.1f;

    [Header("Console Debug")]
    [Tooltip("Show detailed frequency band info in console")]
    public bool showDetailedInfo = false;
    [Tooltip("How often to log detailed info (in seconds)")]
    [Range(0.5f, 5.0f)]
    public float detailedInfoInterval = 2.0f;
    
    private float lastDetailedInfoTime = 0f;

    private const int SAMPLE_SIZE = 1024;
    private const float SAMPLE_RATE = 44100f; // Standard audio sample rate
    
    private float[] samples = new float[SAMPLE_SIZE];
    
    // Separate history tracking for each frequency band
    private Queue<float> lowFreqHistory = new Queue<float>();
    private Queue<float> midFreqHistory = new Queue<float>();
    private Queue<float> highFreqHistory = new Queue<float>();
    
    // Separate timing for each frequency band
    private float lastLowBeatTime;
    private float lastMidBeatTime;
    private float lastHighBeatTime;
    
    // Adaptive thresholds for each band
    private float lowAdaptiveThreshold;
    private float midAdaptiveThreshold;
    private float highAdaptiveThreshold;

    void Start()
    {
        // Ensure we have a valid AudioSource reference
        EnsureAudioSourceReference();
        
        // Get the selected song from GameManager
        if (GameManager.Instance != null && GameManager.Instance.GetSelectedSong() != null)
        {
            AudioClip selectedSong = GameManager.Instance.GetSelectedSong();
            
            if (source != null)
            {
                source.clip = selectedSong;
                Debug.Log($"BeatDetector: Set audio clip to {selectedSong.name}");
            }
            else
            {
                Debug.LogWarning("BeatDetector: AudioSource not assigned!");
            }
        }
        else
        {
            Debug.LogWarning("BeatDetector: No selected song found in GameManager!");
        }
        
        lowAdaptiveThreshold = adaptiveThreshold;
        midAdaptiveThreshold = adaptiveThreshold;
        highAdaptiveThreshold = adaptiveThreshold;
        
        // Validate frequency ranges
        ValidateFrequencyRanges();
    }
    
    private void EnsureAudioSourceReference()
    {
        // If no AudioSource is explicitly assigned, try to get one from the current GameObject
        if (source == null)
        {
            source = GetComponent<AudioSource>();
            if (source == null)
            {
                Debug.LogWarning("BeatDetector: No AudioSource found or assigned!", this);
                return;
            }
            else
            {
                Debug.Log($"BeatDetector: Using AudioSource from current GameObject ({gameObject.name})");
            }
        }
        else
        {
            Debug.Log($"BeatDetector: Using explicitly assigned AudioSource ({source.gameObject.name})");
        }
    }
    
    private void ValidateFrequencyRanges()
    {
        // Validate each band independently
        if (lowFreqMin >= lowFreqMax)
        {
            Debug.LogWarning($"[BeatDetector] Low frequency band invalid! Min ({lowFreqMin}) should be less than Max ({lowFreqMax})");
        }
        
        if (midFreqMin >= midFreqMax)
        {
            Debug.LogWarning($"[BeatDetector] Mid frequency band invalid! Min ({midFreqMin}) should be less than Max ({midFreqMax})");
        }
        
        if (highFreqMin >= highFreqMax)
        {
            Debug.LogWarning($"[BeatDetector] High frequency band invalid! Min ({highFreqMin}) should be less than Max ({highFreqMax})");
        }
        
        // Log the configured ranges
        Debug.Log($"[BeatDetector] Independent Frequency Ranges:");
        Debug.Log($"  Low Band: {lowFreqMin:F0}-{lowFreqMax:F0} Hz");
        Debug.Log($"  Mid Band: {midFreqMin:F0}-{midFreqMax:F0} Hz");
        Debug.Log($"  High Band: {highFreqMin:F0}-{highFreqMax:F0} Hz");
    }

    void Update()
    {
        if (source == null || !source.isPlaying) return;
        
        // Get spectrum data with higher resolution
        source.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

        // Calculate energy in different frequency bands using independent ranges
        float lowFreqEnergy = CalculateFrequencyBandEnergy(GetFrequencyBin(lowFreqMin), GetFrequencyBin(lowFreqMax));
        float midFreqEnergy = CalculateFrequencyBandEnergy(GetFrequencyBin(midFreqMin), GetFrequencyBin(midFreqMax));
        float highFreqEnergy = CalculateFrequencyBandEnergy(GetFrequencyBin(highFreqMin), GetFrequencyBin(highFreqMax));
        
        // Update energy history for each band
        UpdateFrequencyHistory(lowFreqEnergy, midFreqEnergy, highFreqEnergy);

        // Update adaptive thresholds for each band
        UpdateAdaptiveThresholds(lowFreqEnergy, midFreqEnergy, highFreqEnergy);

        // Check for beats in each frequency range
        CheckFrequencyBeat(BeatType.Low, lowFreqEnergy, lowFreqHistory, ref lastLowBeatTime, lowBeatThreshold, lowFreqWeight, lowAdaptiveThreshold);
        CheckFrequencyBeat(BeatType.Mid, midFreqEnergy, midFreqHistory, ref lastMidBeatTime, midBeatThreshold, midFreqWeight, midAdaptiveThreshold);
        CheckFrequencyBeat(BeatType.High, highFreqEnergy, highFreqHistory, ref lastHighBeatTime, highBeatThreshold, highFreqWeight, highAdaptiveThreshold);
        
        // Log detailed information periodically
        LogDetailedInfo();
    }

    private int GetFrequencyBin(float frequency)
    {
        // Convert frequency to FFT bin index
        int bin = Mathf.RoundToInt(frequency * SAMPLE_SIZE / SAMPLE_RATE);
        return Mathf.Clamp(bin, 0, SAMPLE_SIZE - 1);
    }

    private float CalculateFrequencyBandEnergy(int startBin, int endBin)
    {
        float energy = 0f;
        int bandsToSum = Mathf.Min(endBin, SAMPLE_SIZE);
        int actualStart = Mathf.Max(startBin, 0);
        
        for (int i = actualStart; i < bandsToSum; i++)
        {
            energy += samples[i] * samples[i]; // Use power spectrum
        }
        
        int bandCount = bandsToSum - actualStart;
        return bandCount > 0 ? energy / bandCount : 0f;
    }

    private void UpdateFrequencyHistory(float lowEnergy, float midEnergy, float highEnergy)
    {
        lowFreqHistory.Enqueue(lowEnergy);
        midFreqHistory.Enqueue(midEnergy);
        highFreqHistory.Enqueue(highEnergy);

        while (lowFreqHistory.Count > historySize)
        {
            lowFreqHistory.Dequeue();
            midFreqHistory.Dequeue();
            highFreqHistory.Dequeue();
        }
    }

    private void UpdateAdaptiveThresholds(float lowEnergy, float midEnergy, float highEnergy)
    {
        if (lowFreqHistory.Count > 0)
        {
            lowAdaptiveThreshold = UpdateSingleAdaptiveThreshold(lowAdaptiveThreshold, lowFreqHistory);
            midAdaptiveThreshold = UpdateSingleAdaptiveThreshold(midAdaptiveThreshold, midFreqHistory);
            highAdaptiveThreshold = UpdateSingleAdaptiveThreshold(highAdaptiveThreshold, highFreqHistory);
        }
    }

    private float UpdateSingleAdaptiveThreshold(float currentThreshold, Queue<float> history)
    {
        float averageEnergy = history.Average();
        float variance = history.Sum(x => (x - averageEnergy) * (x - averageEnergy)) / history.Count;
        
        // Adapt threshold based on recent energy levels and variance
        return Mathf.Lerp(currentThreshold, 
            averageEnergy + (variance * varianceWeight), 
            Time.deltaTime * 2f);
    }

    private void CheckFrequencyBeat(BeatType beatType, float currentEnergy, Queue<float> history, 
                                   ref float lastBeatTime, float thresholdMultiplier, float weight, float adaptiveThresh)
    {
        // Use minimal history requirement for immediate detection
        if (history.Count < minHistoryForDetection) return;

        // Calculate dynamic cooldown based on beat type
        float dynamicCooldown = CalculateDynamicCooldown(lastBeatTime);
        if (Time.time - lastBeatTime < dynamicCooldown) return;

        // Calculate threshold for this frequency band
        float averageEnergy = history.Count > 0 ? history.Average() : 0.001f;
        float threshold = Mathf.Max(adaptiveThresh, averageEnergy * thresholdMultiplier) * sensitivity * weight;

        // Detection criteria specific to frequency band
        bool energySpike = currentEnergy > threshold;
        bool suddenIncrease = currentEnergy > averageEnergy * GetSuddenIncreaseMultiplier(beatType);

        if (energySpike && suddenIncrease)
        {
            float intensity = CalculateBeatIntensity(currentEnergy, averageEnergy);
            BeatEventSystem.RaiseBeat(intensity, beatType, currentEnergy);
            
            lastBeatTime = Time.time;

            Debug.Log($"[BeatDetector] {beatType} BEAT! Intensity: {intensity:F1}, Energy: {currentEnergy:F4}, Time: {Time.time:F2}s");
        }
    }

    private float GetSuddenIncreaseMultiplier(BeatType beatType)
    {
        // Different sensitivity for different frequency ranges
        return beatType switch
        {
            BeatType.Low => 1.3f,   // Bass hits are usually more pronounced
            BeatType.Mid => 1.4f,   // Mid frequencies need slightly higher threshold
            BeatType.High => 1.5f,  // High frequencies are often more noisy
            _ => 1.4f
        };
    }

    private float CalculateDynamicCooldown(float lastBeatTime)
    {
        float timeSinceLastBeat = Time.time - lastBeatTime;
        
        // Shorter cooldown for consistent tempo, longer for sporadic beats
        if (timeSinceLastBeat < 0.6f)
        {
            return minCooldown; // Allow faster beats
        }
        else
        {
            return Mathf.Lerp(minCooldown, maxCooldown, 
                Mathf.Clamp01((timeSinceLastBeat - 0.6f) / 2.0f));
        }
    }

    private float CalculateBeatIntensity(float currentEnergy, float averageEnergy)
    {
        if (averageEnergy <= 0.001f) return 50f;

        float energyRatio = currentEnergy / averageEnergy;
        float intensity = Mathf.Clamp(energyRatio * 25f, 1f, 100f);
        
        return intensity;
    }
    void LogDetailedInfo()
    {
        if (!showDetailedInfo || Time.time - lastDetailedInfoTime < detailedInfoInterval) return;
        if (!Application.isPlaying || lowFreqHistory.Count == 0 || source == null || !source.isPlaying) return;
        
        lastDetailedInfoTime = Time.time;
        
        Debug.Log("=== BEAT DETECTION DEBUG ===");
        Debug.Log($"Audio Source: {source.name} (Playing: {source.isPlaying}, Volume: {source.volume:F3})");
        Debug.Log("=== Independent Frequency Band Configuration ===");
        Debug.Log($"Low Band: {lowFreqMin:F0}-{lowFreqMax:F0} Hz");
        Debug.Log($"Mid Band: {midFreqMin:F0}-{midFreqMax:F0} Hz");
        Debug.Log($"High Band: {highFreqMin:F0}-{highFreqMax:F0} Hz");
        Debug.Log("=== Current Energy Levels ===");
        Debug.Log($"Low Freq Energy: {(lowFreqHistory.Count > 0 ? lowFreqHistory.Last() : 0):F6}");
        Debug.Log($"Mid Freq Energy: {(midFreqHistory.Count > 0 ? midFreqHistory.Last() : 0):F6}");
        Debug.Log($"High Freq Energy: {(highFreqHistory.Count > 0 ? highFreqHistory.Last() : 0):F6}");
        Debug.Log("=== Adaptive Thresholds ===");
        Debug.Log($"Low Threshold: {lowAdaptiveThreshold:F6}");
        Debug.Log($"Mid Threshold: {midAdaptiveThreshold:F6}");
        Debug.Log($"High Threshold: {highAdaptiveThreshold:F6}");
        Debug.Log("=== Time Since Last Beat ===");
        Debug.Log($"Low Beat: {(Time.time - lastLowBeatTime):F2}s ago");
        Debug.Log($"Mid Beat: {(Time.time - lastMidBeatTime):F2}s ago");
        Debug.Log($"High Beat: {(Time.time - lastHighBeatTime):F2}s ago");
        Debug.Log($"History Count: {lowFreqHistory.Count}/{historySize}");
    }
}
