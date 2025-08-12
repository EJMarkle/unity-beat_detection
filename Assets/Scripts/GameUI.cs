using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("TextMeshPro UI References")]
    public TextMeshProUGUI scoreTMPText;
    public TextMeshProUGUI qualityTMPText;
    
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = GameManager.Instance;
        gameManager.lastHitQuality = "";   
        if (gameManager == null)
        {
            Debug.LogError("GameManager instance not found! Make sure GameManager is in the scene and runs before MenuManager.");
        }
    }

    void Update()
    {
        if (gameManager != null)
        {
            UpdateUI();
        }
    }
    
    
    void UpdateUI()
    {
        UpdateScoreDisplay();
        UpdateQualityDisplay();
    }
    
    void UpdateScoreDisplay()
    {
        string scoreDisplay = $"Score: {gameManager.GetTotalScore()}";
        
        if (scoreTMPText != null)
        {
            scoreTMPText.text = scoreDisplay;
        }
    }
    
    void UpdateQualityDisplay()
    {
        string qualityString = gameManager.GetLastHitQuality();
        
        if (qualityTMPText != null)
        {
            if (string.IsNullOrEmpty(qualityString))
            {
                qualityTMPText.text = "";
            }
            else
            {
                qualityTMPText.text = qualityString;
            }
        }
    }
    
    public void OnHitDetected(HitQuality quality)
    {
        if (gameManager != null)
        {
            gameManager.AddScore(quality);
        }
    }
    
    public void ResetGame()
    {
        if (gameManager != null)
        {
            gameManager.ResetScore();
        }
    }
}
