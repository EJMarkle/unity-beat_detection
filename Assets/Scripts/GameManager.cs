using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Scoring")]
    public int totalScore = 0;
    public string lastHitQuality;
    
    [Header("Score Values")]
    public int perfectScore = 5;
    public int goodScore = 3;
    public int okScore = 1;
    public int missScore = 0;
    
    [Header("Audio")]
    public AudioClip selectedSong;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    public void AddScore(HitQuality quality)
    {
        switch (quality)
        {
            case HitQuality.Perfect:
                totalScore += perfectScore;
                lastHitQuality = "perfect!";
                break;
            case HitQuality.Good:
                totalScore += goodScore;
                lastHitQuality = "good";
                break;
            case HitQuality.OK:
                totalScore += okScore;
                lastHitQuality = "ok";
                break;
            case HitQuality.Miss:
                totalScore += missScore;
                lastHitQuality = "miss";
                break;
        }
        
        Debug.Log($"Hit Quality: {lastHitQuality}, Total Score: {totalScore}");
    }
    
    public int GetTotalScore()
    {
        return totalScore;
    }
    
    public string GetLastHitQuality()
    {
        return lastHitQuality;
    }
    
    public void ResetScore()
    {
        totalScore = 0;
        lastHitQuality = "";
    }
    
    public void SetSelectedSong(AudioClip clip)
    {
        selectedSong = clip;
        Debug.Log($"GameManager: Selected song set to {(clip != null ? clip.name : "null")}");
    }
    
    public AudioClip GetSelectedSong()
    {
        return selectedSong;
    }
}

public enum HitQuality
{
    Miss,
    OK,
    Good,
    Perfect
}
