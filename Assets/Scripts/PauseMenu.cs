using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenu;

    [Header("Input References")]
    public InputActionReference primaryButtonAction;

    [Header("Pause Settings")]
    public bool pauseAudio = true;
    public bool showCursor = false;

    private bool isPaused = false;
    private float originalTimeScale = 1f;
    private AudioSource[] audioSources;

    void Start()
    {
        // Ensure pause menu starts hidden
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        // Get all audio sources for pausing
        if (pauseAudio)
        {
            audioSources = FindObjectsOfType<AudioSource>();
        }

        // Store original time scale
        originalTimeScale = Time.timeScale;

        // If no input action reference is assigned, try to find it automatically
        if (primaryButtonAction == null)
        {
            Debug.LogWarning("PauseMenu: No primaryButtonAction assigned. Please assign the XR Controller Primary Button action in the Inspector.");
        }
    }

    void OnEnable()
    {
        if (primaryButtonAction != null)
        {
            primaryButtonAction.action.performed += OnPrimaryButtonPressed;
            primaryButtonAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (primaryButtonAction != null)
        {
            primaryButtonAction.action.performed -= OnPrimaryButtonPressed;
            primaryButtonAction.action.Disable();
        }
    }

    private void OnPrimaryButtonPressed(InputAction.CallbackContext context)
    {
        Debug.Log("A button pressed - toggling pause");
        TogglePause();
    }
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("VRScene");
    }
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    public void PauseGame()
    {
        if (isPaused) return;
        
        isPaused = true;
        
        // Show pause menu
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }
        
        // Pause time
        Time.timeScale = 0f;
        
        // Pause audio sources
        if (pauseAudio && audioSources != null)
        {
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Pause();
                }
            }
        }
        
        // Show cursor if needed (useful for desktop testing)
        if (showCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        Debug.Log("Game Paused");
    }
    
    public void ResumeGame()
    {
        if (!isPaused) return;
        
        isPaused = false;
        
        // Hide pause menu
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        
        // Resume time
        Time.timeScale = originalTimeScale;
        
        // Resume audio sources
        if (pauseAudio && audioSources != null)
        {
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource != null)
                {
                    audioSource.UnPause();
                }
            }
        }
        
        // Hide cursor
        if (showCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        Debug.Log("Game Resumed");
    }
    
    // Public methods for UI buttons
    public void OnResumeButtonPressed()
    {
        ResumeGame();
    }
    
    public void OnQuitButtonPressed()
    {
        // First resume the game to restore normal time scale
        ResumeGame();
        
        // Then call the main menu return function
        MainMenuUI mainMenuUI = FindObjectOfType<MainMenuUI>();
        if (mainMenuUI != null)
        {
            mainMenuUI.ReturnToMenu();
        }
        else
        {
            Debug.LogWarning("MainMenuUI not found - cannot return to menu");
        }
    }
    
    // Alternative input method for testing in editor
    void Update()
    {
        // Allow keyboard input for testing in editor
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    public bool IsPaused()
    {
        return isPaused;
    }
}
