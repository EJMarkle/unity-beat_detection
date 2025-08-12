using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class FrequencyBandEvent : UnityEvent<float> { }

[System.Serializable] 
public class BeatEvent : UnityEvent { }

public class PlaybackBeatDetector : MonoBehaviour
{
    [Header("Audio Setup")]
    public AudioSource source;
    
    [Header("Beat Detection Settings (For Scoring)")]
    [Range(0.001f, 0.1f)]
    public float beatThreshold = 0.005f;
    [Range(0.1f, 1.0f)]
    public float beatCooldown = 0.3f;
    [Range(1.0f, 10.0f)]
    public float sensitivity = 2.0f;
    
    [Header("Frequency Band Settings (For Shadergraphs)")]
    [Range(0.001f, 0.05f)]
    public float lowBeatThreshold = 0.01f;
    [Range(0.001f, 0.05f)]
    public float midBeatThreshold = 0.005f;
    [Range(0.001f, 0.05f)]
    public float highSustainThreshold = 0.002f;
    
    [Range(0.1f, 1.0f)]
    public float lowBeatCooldown = 0.3f;
    [Range(0.1f, 1.0f)]
    public float midBeatCooldown = 0.2f;
    
    [Range(1.0f, 5.0f)]
    public float lowSensitivity = 2.0f;
    [Range(1.0f, 5.0f)]
    public float midSensitivity = 1.8f;
    [Range(1.0f, 5.0f)]
    public float highSensitivity = 1.5f;
    
    [Header("Frequency Band Events")]
    public BeatEvent OnLowBeat = new BeatEvent();
    public BeatEvent OnMidBeat = new BeatEvent(); 
    public FrequencyBandEvent OnHighFrequency = new FrequencyBandEvent();
    
    [Header("Debug Options")]
    public bool showDebugInfo = true;
    public bool visualizeLevels = false;
    public bool showFrequencyEvents = true;
    
    private float[] samples = new float[1024];
    
    // Original beat detection (for scoring)
    private float lastBeatTime = -999f;
    private float[] bassHistory = new float[10];
    private int historyIndex = 0;
    private float currentBassLevel;
    private float averageBassLevel;

    // Frequency band detection (for shadergraphs)
    private float[] lowHistory = new float[8];
    private float[] midHistory = new float[8];
    private float[] highHistory = new float[6];
    private int lowHistoryIndex = 0;
    private int midHistoryIndex = 0;
    private int highHistoryIndex = 0;
    
    private float currentLowLevel;
    private float currentMidLevel;
    private float currentHighLevel;
    private float averageLowLevel;
    private float averageMidLevel;
    private float averageHighLevel;
    
    private float lastLowBeatTime = -999f;
    private float lastMidBeatTime = -999f;
    
    // Warning throttling
    private float lastWarningTime = -999f;
    private const float WARNING_INTERVAL = 5f; // Only show warning every 5 seconds

    // Public properties for scoring system (unchanged)
    public bool BeatJustOccurred { get; private set; }
    public float LastBeatTime { get; private set; } = -999f;
    
    // Public properties for frequency bands
    public bool LowBeatJustOccurred { get; private set; }
    public bool MidBeatJustOccurred { get; private set; }
    public float CurrentHighLevel { get; private set; }
    public float NormalizedHighLevel { get; private set; }

    void Start()
    {
        // Validate AudioSource assignment
        if (source == null)
        {
            source = GetComponent<AudioSource>();
            if (source == null)
            {
                // Try to find AudioManager's AudioSource
                AudioManager audioManager = FindObjectOfType<AudioManager>();
                if (audioManager != null && audioManager.playbackAudio != null)
                {
                    source = audioManager.playbackAudio;
                    Debug.Log("[PlaybackBeatDetector] Using AudioManager's playbackAudio as source");
                }
                else
                {
                    // Fallback to BeatDetection AudioSource
                    GameObject beatDetectionObj = GameObject.Find("BeatDetection");
                    if (beatDetectionObj != null)
                    {
                        AudioSource beatAudioSource = beatDetectionObj.GetComponent<AudioSource>();
                        if (beatAudioSource != null)
                        {
                            source = beatAudioSource;
                            Debug.Log("[PlaybackBeatDetector] Using BeatDetection AudioSource as fallback");
                        }
                    }
                }
                
                if (source == null)
                {
                    Debug.LogError("[PlaybackBeatDetector] No AudioSource found! Please assign one or ensure AudioManager/BeatDetection has AudioSources.");
                    enabled = false;
                    return;
                }
            }
        }
        
        // Initialize histories with small values
        for (int i = 0; i < bassHistory.Length; i++)
            bassHistory[i] = 0.001f;
            
        for (int i = 0; i < lowHistory.Length; i++)
            lowHistory[i] = 0.001f;
            
        for (int i = 0; i < midHistory.Length; i++)
            midHistory[i] = 0.001f;
            
        for (int i = 0; i < highHistory.Length; i++)
            highHistory[i] = 0.001f;
            
        Debug.Log("[PlaybackBeatDetector] Initialized with frequency band detection");
    }

    void Update()
    {
        // Reset beat flags
        BeatJustOccurred = false;
        LowBeatJustOccurred = false;
        MidBeatJustOccurred = false;

        if (source == null || !source.isPlaying)
        {
            if (showDebugInfo && source != null && !source.isPlaying)
            {
                // Only show warning every 5 seconds to avoid spam
                if (Time.time - lastWarningTime > WARNING_INTERVAL)
                {
                    Debug.LogWarning("[PlaybackBeatDetector] AudioSource is not playing!");
                    lastWarningTime = Time.time;
                }
            }
            return;
        }

        // Get spectrum data
        source.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

        // Process original beat detection (for scoring - unchanged)
        ProcessOriginalBeatDetection();
        
        // Process frequency band detection (for shadergraphs)
        ProcessFrequencyBandDetection();
    }
    
    private void ProcessOriginalBeatDetection()
    {
        // Focus on bass frequencies (roughly 20-250 Hz)
        // For 44100 Hz sample rate with 1024 samples: each bin = ~43 Hz
        // So bins 0-5 cover roughly 0-250 Hz (bass range)
        currentBassLevel = 0f;
        for (int i = 0; i < 6; i++)
        {
            currentBassLevel += samples[i];
        }
        currentBassLevel /= 6f;

        // Update rolling average
        bassHistory[historyIndex] = currentBassLevel;
        historyIndex = (historyIndex + 1) % bassHistory.Length;
        
        averageBassLevel = 0f;
        for (int i = 0; i < bassHistory.Length; i++)
            averageBassLevel += bassHistory[i];
        averageBassLevel /= bassHistory.Length;

        // Beat detection: current level significantly above recent average
        float threshold = averageBassLevel * sensitivity;
        bool cooldownPassed = Time.time - lastBeatTime > beatCooldown;
        
        if (currentBassLevel > beatThreshold && currentBassLevel > threshold && cooldownPassed)
        {
            BeatJustOccurred = true;
            lastBeatTime = Time.time;
            LastBeatTime = Time.time;

            if (showDebugInfo)
            {
                Debug.Log($"[PlaybackBeatDetector] 🥁 BEAT at {Time.time:F2}s | " +
                         $"Bass: {currentBassLevel:F4} | Avg: {averageBassLevel:F4} | " +
                         $"Threshold: {threshold:F4}");
            }
        }
    }
    
    private void ProcessFrequencyBandDetection()
    {
        // Process Low Frequencies (20-250 Hz, bins 0-5)
        currentLowLevel = 0f;
        for (int i = 0; i < 6; i++)
        {
            currentLowLevel += samples[i];
        }
        currentLowLevel /= 6f;
        
        lowHistory[lowHistoryIndex] = currentLowLevel;
        lowHistoryIndex = (lowHistoryIndex + 1) % lowHistory.Length;
        
        averageLowLevel = 0f;
        for (int i = 0; i < lowHistory.Length; i++)
            averageLowLevel += lowHistory[i];
        averageLowLevel /= lowHistory.Length;
        
        // Process Mid Frequencies (250-4000 Hz, bins 6-93)
        currentMidLevel = 0f;
        for (int i = 6; i < 94; i++)
        {
            currentMidLevel += samples[i];
        }
        currentMidLevel /= 88f;
        
        midHistory[midHistoryIndex] = currentMidLevel;
        midHistoryIndex = (midHistoryIndex + 1) % midHistory.Length;
        
        averageMidLevel = 0f;
        for (int i = 0; i < midHistory.Length; i++)
            averageMidLevel += midHistory[i];
        averageMidLevel /= midHistory.Length;
        
        // Process High Frequencies (4000+ Hz, bins 94-511)
        currentHighLevel = 0f;
        for (int i = 94; i < 512; i++)
        {
            currentHighLevel += samples[i];
        }
        currentHighLevel /= 418f;
        
        highHistory[highHistoryIndex] = currentHighLevel;
        highHistoryIndex = (highHistoryIndex + 1) % highHistory.Length;
        
        averageHighLevel = 0f;
        for (int i = 0; i < highHistory.Length; i++)
            averageHighLevel += highHistory[i];
        averageHighLevel /= highHistory.Length;
        
        // Detect Low Beat
        float lowThreshold = averageLowLevel * lowSensitivity;
        bool lowCooldownPassed = Time.time - lastLowBeatTime > lowBeatCooldown;
        
        if (currentLowLevel > lowBeatThreshold && currentLowLevel > lowThreshold && lowCooldownPassed)
        {
            LowBeatJustOccurred = true;
            lastLowBeatTime = Time.time;
            OnLowBeat.Invoke();
            
            if (showFrequencyEvents)
                Debug.Log($"[PlaybackBeatDetector] 🔉 LOW BEAT at {Time.time:F2}s | Level: {currentLowLevel:F4}");
        }
        
        // Detect Mid Beat
        float midThreshold = averageMidLevel * midSensitivity;
        bool midCooldownPassed = Time.time - lastMidBeatTime > midBeatCooldown;
        
        if (currentMidLevel > midBeatThreshold && currentMidLevel > midThreshold && midCooldownPassed)
        {
            MidBeatJustOccurred = true;
            lastMidBeatTime = Time.time;
            OnMidBeat.Invoke();
            
            if (showFrequencyEvents)
                Debug.Log($"[PlaybackBeatDetector] 🔊 MID BEAT at {Time.time:F2}s | Level: {currentMidLevel:F4}");
        }
        
        // Process High Frequencies (sustained)
        CurrentHighLevel = currentHighLevel;
        float highThreshold = averageHighLevel * highSensitivity;
        
        // Normalize high level for sustained output (0-1 range)
        if (currentHighLevel > highSustainThreshold && currentHighLevel > highThreshold)
        {
            NormalizedHighLevel = Mathf.Clamp01((currentHighLevel - highThreshold) / (highThreshold * 2f));
        }
        else
        {
            NormalizedHighLevel = 0f;
        }
        
        // Only trigger high frequency event if there's meaningful activity
        // This prevents constant triggering when audio isn't playing or signal is too low
        if (NormalizedHighLevel > 0.01f) // Only invoke when there's actual high frequency content
        {
            OnHighFrequency.Invoke(NormalizedHighLevel);
            
            if (showFrequencyEvents)
                Debug.Log($"[PlaybackBeatDetector] 🎵 HIGH FREQUENCY at {Time.time:F2}s | Level: {NormalizedHighLevel:F2}");
        }

        // Debug visualization
        if (visualizeLevels && Time.frameCount % 30 == 0)
        {
            Debug.Log($"[PlaybackBeatDetector] Bands - Low: {currentLowLevel:F4} | " +
                     $"Mid: {currentMidLevel:F4} | High: {currentHighLevel:F4} (Norm: {NormalizedHighLevel:F2})");
        }
    }

    // Original methods for scoring system (unchanged)
    public float TimeSinceLastBeat()
    {
        if (LastBeatTime < 0)
            return 999f;
            
        return Time.time - LastBeatTime;
    }
    
    public float GetCurrentBassLevel()
    {
        return currentBassLevel;
    }
    
    public float GetAverageBassLevel()
    {
        return averageBassLevel;
    }
    
    // New methods for frequency band access
    public float GetCurrentLowLevel()
    {
        return currentLowLevel;
    }
    
    public float GetCurrentMidLevel()
    {
        return currentMidLevel;
    }
    
    public float GetCurrentHighLevel()
    {
        return currentHighLevel;
    }
    
    public float GetNormalizedHighLevel()
    {
        return NormalizedHighLevel;
    }
}
