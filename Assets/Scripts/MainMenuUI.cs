using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuUI : MonoBehaviour
{
    public GameObject swordL;
    public GameObject swordR;
    public GameObject menuUI;
    public GameObject gameUI;
    [Header("UI References")]
    public TMP_Dropdown songDropdown;
    
    [Header("Audio Settings")]
    public string audioFolderPath = "Audio/";
    [Header("Auto-Discovery")]
    [Tooltip("If true, automatically discover all audio files in Resources/Audio folder")]
    public bool autoDiscoverSongs = true;
    [Tooltip("Fallback songs to use if auto-discovery fails")]
    public List<string> fallbackSongs = new List<string> {
        "Parliament - Flashlight",
        "Thundercat - A Fan's Mail ",
        "Thundercat - Them Changes"
    };
    
    private List<string> availableAudioFiles = new List<string>();
    private List<AudioClip> loadedAudioClips = new List<AudioClip>();

    void Start()
    {
        if (autoDiscoverSongs)
        {
            AutoDiscoverAudioFiles();
        }
        else
        {
            LoadFallbackSongs();
        }
        
        PopulateDropdown();
        
        // Log the current state
        int successfulClips = loadedAudioClips.FindAll(c => c != null).Count;
        Debug.Log($"📊 Audio Loading Summary: {successfulClips}/{loadedAudioClips.Count} clips loaded successfully");
        
        if (successfulClips == 0)
        {
            Debug.LogError("🚨 No audio clips loaded! Check file locations:");
            Debug.LogError("Files should be at: Assets/Resources/Audio/[filename].mp3/.wav/.ogg");
        }
    }
    
    private void AutoDiscoverAudioFiles()
    {
        Debug.Log($"🔍 Auto-discovering audio files in Resources/{audioFolderPath}");
        
        // Load all AudioClips from the specified Resources folder
        AudioClip[] discoveredClips = Resources.LoadAll<AudioClip>(audioFolderPath);
        
        if (discoveredClips.Length > 0)
        {
            Debug.Log($"📁 Found {discoveredClips.Length} audio files in Resources/{audioFolderPath}");
            
            foreach (AudioClip clip in discoveredClips)
            {
                if (clip != null)
                {
                    availableAudioFiles.Add(clip.name);
                    loadedAudioClips.Add(clip);
                    Debug.Log($"✅ Auto-discovered: {clip.name}");
                }
            }
            
            // Sort alphabetically for better organization
            var sortedData = availableAudioFiles
                .Zip(loadedAudioClips, (name, clip) => new { name, clip })
                .OrderBy(x => x.name)
                .ToArray();
            
            availableAudioFiles.Clear();
            loadedAudioClips.Clear();
            
            foreach (var item in sortedData)
            {
                availableAudioFiles.Add(item.name);
                loadedAudioClips.Add(item.clip);
            }
            
            Debug.Log($"📋 Audio files sorted alphabetically");
        }
        else
        {
            Debug.LogWarning($"⚠️ No audio files found in Resources/{audioFolderPath}. Falling back to manual list.");
            LoadFallbackSongs();
        }
    }
    
    private void LoadFallbackSongs()
    {
        Debug.Log($"📝 Loading fallback songs ({fallbackSongs.Count} songs)");
        
        for (int i = 0; i < fallbackSongs.Count; i++)
        {
            string filePath = audioFolderPath + fallbackSongs[i];
            Debug.Log($"Attempting to load: Resources.Load<AudioClip>(\"{filePath}\")");
            
            AudioClip clip = Resources.Load<AudioClip>(filePath);
            
            if (clip != null)
            {
                availableAudioFiles.Add(fallbackSongs[i]);
                loadedAudioClips.Add(clip);
                Debug.Log($"✅ Successfully loaded: {fallbackSongs[i]}");
            }
            else
            {
                Debug.LogError($"❌ Failed to load: {fallbackSongs[i]}");
                Debug.LogError($"Expected at: Assets/Resources/{filePath}.mp3/.wav/.ogg");
                
                // Try alternative paths
                string alternativePath = fallbackSongs[i]; // without Audio/ prefix
                Debug.Log($"Trying alternative: Resources.Load<AudioClip>(\"{alternativePath}\")");
                clip = Resources.Load<AudioClip>(alternativePath);
                
                if (clip != null)
                {
                    availableAudioFiles.Add(fallbackSongs[i]);
                    loadedAudioClips.Add(clip);
                    Debug.Log($"✅ Success with alternative path: {alternativePath}");
                }
                else
                {
                    Debug.LogError($"❌ Alternative path also failed");
                    // Add placeholder to keep indices aligned
                    availableAudioFiles.Add(fallbackSongs[i] + " (MISSING)");
                    loadedAudioClips.Add(null);
                }
            }
        }
    }

    private void PopulateDropdown()
    {
        if (songDropdown != null)
        {
            songDropdown.ClearOptions();
            
            foreach (string audioFile in availableAudioFiles)
            {
                songDropdown.options.Add(new TMP_Dropdown.OptionData(audioFile));
            }
            
            songDropdown.RefreshShownValue();
        }
    }

    public void StartGame()
    {
        // Switch UI states
        menuUI.SetActive(false);
        gameUI.SetActive(true);
        swordL.SetActive(true);
        swordR.SetActive(true);
        
        // Set selected audio clip and start playback
        if (songDropdown != null && songDropdown.value < loadedAudioClips.Count)
        {
            AudioClip selectedClip = loadedAudioClips[songDropdown.value];
            
            if (selectedClip != null)
            {
                Debug.Log($"Selected song: {selectedClip.name} (dropdown value: {songDropdown.value})");
                SetAudioForGameObjects(selectedClip);
                
                // Start audio playback after a short delay
                StartCoroutine(StartAudioPlayback());
            }
            else
            {
                Debug.LogError($"Selected audio clip at index {songDropdown.value} is null!");
            }
        }
        else
        {
            Debug.LogError("No song selected or songDropdown is null!");
        }
        
        Debug.Log("Game Started! Swords enabled, attempting audio playback.");
    }
    
    private IEnumerator StartAudioPlayback()
    {
        // Wait a brief moment for audio sources to be set
        yield return new WaitForSeconds(0.1f);
        
        // Start BeatDetection audio source immediately for beat analysis
        BeatDetector beatDetector = FindObjectOfType<BeatDetector>();
        if (beatDetector != null)
        {
            // Ensure BeatDetector has a valid AudioSource reference
            if (beatDetector.source == null)
            {
                Debug.LogWarning("BeatDetector source is null, attempting to get AudioSource component");
                beatDetector.source = beatDetector.GetComponent<AudioSource>();
            }
            
            if (beatDetector.source != null && beatDetector.source.clip != null)
            {
                beatDetector.source.Play();
                Debug.Log("Started beat detection audio source");
            }
            else
            {
                Debug.LogError($"Cannot start beat detection: source={beatDetector.source != null}, clip={beatDetector.source?.clip != null}");
            }
        }
        else
        {
            Debug.LogError("BeatDetector not found in scene!");
        }
        
        // Now tell AudioManager to start its delayed playback
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.StartDelayedPlayback();
            Debug.Log($"AudioManager scheduled to start after {audioManager.startDelay} seconds");
        }
        else
        {
            Debug.LogError("AudioManager not found in scene!");
        }
    }

    private void SetAudioForGameObjects(AudioClip selectedClip)
    {
        if (selectedClip == null)
        {
            Debug.LogError("Selected audio clip is null! Cannot set audio for game objects.");
            return;
        }

        Debug.Log($"Setting audio clip: {selectedClip.name} for BeatDetection and AudioManager");

        // Set selected song in GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetSelectedSong(selectedClip);
            Debug.Log($"✓ Set GameManager selected song to: {selectedClip.name}");
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is null! BeatDetector won't be able to get the selected song.");
        }

        // Set BeatDetection audio
        GameObject beatDetectionObject = GameObject.Find("BeatDetection");
        if (beatDetectionObject != null)
        {
            AudioSource audioSource = beatDetectionObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.clip = selectedClip;
                Debug.Log($"✓ Set BeatDetection AudioSource clip to: {selectedClip.name}");
            }
            else
            {
                Debug.LogError("BeatDetection GameObject found but no AudioSource component!");
            }
            
            // Also check BeatDetector component
            BeatDetector beatDetector = beatDetectionObject.GetComponent<BeatDetector>();
            if (beatDetector != null && beatDetector.source != null)
            {
                beatDetector.source.clip = selectedClip;
                Debug.Log($"✓ Set BeatDetector.source clip to: {selectedClip.name}");
            }
        }
        else
        {
            Debug.LogError("BeatDetection GameObject not found in scene!");
        }

        // Set AudioManager audio
        GameObject audioManagerObject = GameObject.Find("AudioManager");
        if (audioManagerObject != null)
        {
            AudioSource audioSource = audioManagerObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.clip = selectedClip;
                Debug.Log($"✓ Set AudioManager AudioSource clip to: {selectedClip.name}");
            }
            else
            {
                Debug.LogError("AudioManager GameObject found but no AudioSource component!");
            }
            
            AudioManager audioManager = audioManagerObject.GetComponent<AudioManager>();
            if (audioManager != null)
            {
                if (audioManager.playbackAudio != null)
                {
                    audioManager.playbackAudio.clip = selectedClip;
                    Debug.Log($"✓ Set AudioManager.playbackAudio clip to: {selectedClip.name}");
                }
                else
                {
                    Debug.LogWarning("AudioManager.playbackAudio is null! Please assign it in the Inspector.");
                }
            }
            else
            {
                Debug.LogError("AudioManager component not found on AudioManager GameObject!");
            }
        }
        else
        {
            Debug.LogError("AudioManager GameObject not found in scene!");
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("VRScene");
    }

    public void RestartGameCompletely()
    {
        StartCoroutine(CompleteGameRestart());
    }

    private IEnumerator CompleteGameRestart()
    {
        Debug.Log("🔄 Starting complete game restart...");
        
        // Stop all audio systems
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.StopPlayback();
            Debug.Log("✓ AudioManager stopped");
        }
        
        // Stop beat detection
        BeatDetector beatDetector = FindObjectOfType<BeatDetector>();
        if (beatDetector != null && beatDetector.source != null)
        {
            beatDetector.source.Stop();
            beatDetector.source.clip = null;
            Debug.Log("✓ BeatDetector stopped and cleared");
        }
        
        PlaybackBeatDetector playbackDetector = FindObjectOfType<PlaybackBeatDetector>();
        if (playbackDetector != null && playbackDetector.source != null)
        {
            playbackDetector.source.Stop();
            playbackDetector.source.clip = null;
            Debug.Log("✓ PlaybackBeatDetector stopped and cleared");
        }
        
        // Stop all audio sources in the scene
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in allAudioSources)
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
            source.clip = null;
            source.time = 0f;
        }
        Debug.Log($"✓ Stopped and cleared {allAudioSources.Length} AudioSources");
        
        // Reset GameManager singleton
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetSelectedSong(null);
            Debug.Log("✓ GameManager cleared");
        }
        
        // Stop all coroutines on all MonoBehaviours
        MonoBehaviour[] allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour mb in allMonoBehaviours)
        {
            if (mb != this) // Don't stop our own coroutines yet
            {
                mb.StopAllCoroutines();
            }
        }
        Debug.Log($"✓ Stopped coroutines on {allMonoBehaviours.Length} MonoBehaviours");
        
        // Clear any cached audio clips from this component
        if (loadedAudioClips != null)
        {
            loadedAudioClips.Clear();
        }
        if (availableAudioFiles != null)
        {
            availableAudioFiles.Clear();
        }
        
        // Reset UI states
        if (menuUI != null) menuUI.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        if (swordL != null) swordL.SetActive(false);
        if (swordR != null) swordR.SetActive(false);
        
        // Wait a frame for cleanup to complete
        yield return null;
        
        // Force resource cleanup
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Debug.Log("✓ Resources cleaned up");
        
        // Wait another frame for garbage collection
        yield return null;
        
        Debug.Log("🔄 Reloading scene...");
        
        // Reload scene
        SceneManager.LoadScene("VRScene");
    }

    public void LoadSong()
    {
#if UNITY_EDITOR
        LoadSongEditor();
#else
        LoadSongRuntime();
#endif
    }

#if UNITY_EDITOR
    private void LoadSongEditor()
    {
        string path = EditorUtility.OpenFilePanel("Select Audio File", "", "mp3,wav,ogg,aiff,aif");
        
        if (!string.IsNullOrEmpty(path))
        {
            StartCoroutine(LoadAudioFromPath(path));
        }
    }
#endif

    private void LoadSongRuntime()
    {
        Debug.LogWarning("Runtime file selection not implemented. Consider using a third-party file browser like 'Runtime File Browser' from the Asset Store.");
        
        // For runtime implementation, you would need to:
        // 1. Install a file browser package from Asset Store (e.g., Runtime File Browser)
        // 2. Use that package's API to open a file dialog
        // 3. Load the selected file using the same LoadAudioFromPath coroutine
        
        // Example with Runtime File Browser (if installed):
        // SimpleFileBrowser.FileBrowser.ShowLoadDialog(OnFilesSelected, null, SimpleFileBrowser.FileBrowser.PickMode.Files, false, null, null, "Select Audio File", "Select");
    }

    private IEnumerator LoadAudioFromPath(string path)
    {
        string fileName = Path.GetFileNameWithoutExtension(path);
        
        // Convert to proper URL format
        string url = path;
        if (!url.StartsWith("file://"))
        {
            url = "file://" + path;
        }

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                
                if (clip != null)
                {
                    clip.name = fileName;
                    
                    // Add to our lists
                    availableAudioFiles.Add(fileName);
                    loadedAudioClips.Add(clip);
                    
                    // Update dropdown
                    PopulateDropdown();
                    
                    // Set dropdown to the newly loaded song
                    songDropdown.value = availableAudioFiles.Count - 1;
                    songDropdown.RefreshShownValue();
                    
                    Debug.Log($"Successfully loaded audio file: {fileName}");
                }
                else
                {
                    Debug.LogError($"Failed to create AudioClip from: {path}");
                }
            }
            else
            {
                Debug.LogError($"Failed to load audio file: {www.error}");
            }
        }
    }
}
