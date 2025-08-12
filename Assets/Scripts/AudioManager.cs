using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Playback Settings")]
    public AudioSource playbackAudio;
    public float startDelay = 3f;
    
    private bool isPlaybackScheduled = false;

    private void Start()
    {
        EnsureAudioSourceReference();
        Debug.Log("AudioManager: AudioManager started, waiting for audio clip assignment.");
    }
    
    private void EnsureAudioSourceReference()
    {
        // If no AudioSource is explicitly assigned, try to get one from the current GameObject
        if (playbackAudio == null)
        {
            playbackAudio = GetComponent<AudioSource>();
            if (playbackAudio == null)
            {
                Debug.LogWarning("AudioManager: No AudioSource found or assigned!", this);
                return;
            }
            else
            {
                Debug.Log($"AudioManager: Using AudioSource from current GameObject ({gameObject.name})");
            }
        }
        else
        {
            Debug.Log($"AudioManager: Using explicitly assigned AudioSource ({playbackAudio.gameObject.name})");
        }
    }

    public void StartDelayedPlayback()
    {
        // Ensure we have a valid AudioSource reference
        EnsureAudioSourceReference();
        
        // Check if playbackAudio is properly assigned
        if (playbackAudio == null)
        {
            Debug.LogError("AudioManager: playbackAudio is null! Cannot start playback.");
            return;
        }
        
        // Check if an audio clip is assigned to the AudioSource
        if (playbackAudio.clip == null)
        {
            Debug.LogWarning("AudioManager: No AudioClip assigned to AudioSource!", playbackAudio);
            return;
        }

        // Don't schedule playback if it's already scheduled
        if (isPlaybackScheduled)
        {
            Debug.Log("AudioManager: Playback already scheduled, canceling previous and scheduling new one.");
            CancelInvoke(nameof(PlayAudio));
        }

        Debug.Log($"AudioManager: Ready to play '{playbackAudio.clip.name}' after {startDelay} seconds (volume: {playbackAudio.volume:F2}, enabled: {playbackAudio.enabled}).");
        Invoke(nameof(PlayAudio), startDelay);
        isPlaybackScheduled = true;
    }

    private void PlayAudio()
    {
        if (playbackAudio != null && playbackAudio.clip != null)
        {
            // Additional checks before playing
            if (!playbackAudio.enabled)
            {
                Debug.LogWarning("AudioManager: AudioSource is disabled, enabling it.");
                playbackAudio.enabled = true;
            }
            
            // Ensure volume is not zero
            if (playbackAudio.volume <= 0f)
            {
                Debug.LogWarning($"AudioManager: AudioSource volume is {playbackAudio.volume}, setting to 1.0");
                playbackAudio.volume = 1.0f;
            }
            
            // Stop any currently playing audio to ensure clean playback
            if (playbackAudio.isPlaying)
            {
                playbackAudio.Stop();
            }
            
            // Reset time to start from beginning
            playbackAudio.time = 0f;
            
            Debug.Log($"AudioManager: Starting playback of '{playbackAudio.clip.name}' (length: {playbackAudio.clip.length:F1}s, volume: {playbackAudio.volume:F2})");
            
            playbackAudio.Play();
            
            // Verify audio is actually playing
            if (playbackAudio.isPlaying)
            {
                Debug.Log("AudioManager: Audio playback started successfully.");
            }
            else
            {
                Debug.LogError("AudioManager: Audio failed to start playing despite Play() call!");
            }
        }
        else
        {
            if (playbackAudio == null)
            {
                Debug.LogError("AudioManager: Cannot play audio - playbackAudio is null!");
            }
            else if (playbackAudio.clip == null)
            {
                Debug.LogError("AudioManager: Cannot play audio - AudioClip is null!");
            }
        }
        isPlaybackScheduled = false;
    }
    
    public void StopPlayback()
    {
        if (playbackAudio != null && playbackAudio.isPlaying)
        {
            playbackAudio.Stop();
        }
        
        if (isPlaybackScheduled)
        {
            CancelInvoke(nameof(PlayAudio));
            isPlaybackScheduled = false;
        }
        
        // Reset playback position to ensure clean state for next song
        if (playbackAudio != null)
        {
            playbackAudio.time = 0f;
        }
        
        Debug.Log("AudioManager: Playback stopped and reset");
    }
}
