using UnityEngine;

public class BlockCollision : MonoBehaviour
{
    public PlaybackBeatDetector beatDetector; 

    [Header("Timing Windows (in seconds)")]
    public float perfectWindow = 0.05f;
    public float goodWindow = 0.12f;
    public float okWindow = 0.2f;

    [Header("Console Debug")]
    public bool enableConsoleStats = true;
    [Tooltip("Log detailed stats periodically")]
    public bool logDetailedStats = false;
    [Tooltip("How often to log detailed stats (in seconds)")]
    [Range(5f, 30f)]
    public float statsLogInterval = 10f;

    // Static counters shared across all blocks
    private static int perfectCount = 0;
    private static int goodCount = 0;
    private static int okCount = 0;
    private static int missCount = 0;
    private static int totalHits = 0;
    
    // Static console logging
    private static bool staticConsoleEnabled = true;
    private static bool staticDetailedLogging = false;
    private static float staticLastStatsLogTime = 0f;
    private static float staticStatsInterval = 10f;

    void Start()
    {
        // If not assigned, find it automatically
        if (beatDetector == null)
        {
            beatDetector = FindObjectOfType<PlaybackBeatDetector>();
            
            if (beatDetector == null)
            {
                Debug.LogWarning("[BlockCollision] No PlaybackBeatDetector found in the scene!");
            }
        }

        // Setup static console logging settings
        staticConsoleEnabled = enableConsoleStats;
        staticDetailedLogging = logDetailedStats;
        staticStatsInterval = statsLogInterval;
        
        // Log initial stats
        LogDetailedStats();
    }

    void Update()
    {
        // Log detailed stats periodically
        LogDetailedStats();
    }

    static void LogDetailedStats()
    {
        if (!staticDetailedLogging || Time.time - staticLastStatsLogTime < staticStatsInterval) return;
        
        staticLastStatsLogTime = Time.time;
        
        float accuracy = totalHits > 0 ? (perfectCount + goodCount + okCount) * 100f / totalHits : 0f;
        
        Debug.Log("=== BEAT SABER STATS ===");
        Debug.Log($"🎯 Perfect: {perfectCount}");
        Debug.Log($"👍 Good: {goodCount}");
        Debug.Log($"👌 Ok: {okCount}");
        Debug.Log($"❌ Misses: {missCount}");
        Debug.Log($"📊 Total Hits: {totalHits}");
        Debug.Log($"🎯 Accuracy: {accuracy:F1}%");
    }

    private void OnTriggerEnter(Collider other)
    {
        string thisTag = gameObject.tag;
        string otherTag = other.gameObject.tag;

        // Check for collision with Miss wall
        if (otherTag == "Miss")
        {
            missCount++;
            totalHits++;

            // Register miss with GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(HitQuality.Miss);
            }

            if (staticConsoleEnabled)
            {
                Debug.Log($"[BlockCollision] Block {thisTag} hit miss wall - MISS!");
                Debug.Log($"🎯 Totals — Perfect: {perfectCount}, Good: {goodCount}, Ok: {okCount}, Misses: {missCount}, Total: {totalHits}");
            }

            Destroy(gameObject);
            return;
        }

        // Check for sword collisions
        bool isCorrectPair = (thisTag == "BlockL" && otherTag == "SwordL") ||
                             (thisTag == "BlockR" && otherTag == "SwordR");

        if (isCorrectPair)
        {
            float timeSinceBeat = beatDetector != null ? beatDetector.TimeSinceLastBeat() : float.MaxValue;
            string result = "Miss";
            HitQuality hitQuality = HitQuality.Miss;

            if (timeSinceBeat <= perfectWindow)
            {
                result = "Perfect";
                hitQuality = HitQuality.Perfect;
                perfectCount++;
            }
            else if (timeSinceBeat <= goodWindow)
            {
                result = "Good";
                hitQuality = HitQuality.Good;
                goodCount++;
            }
            else if (timeSinceBeat <= okWindow)
            {
                result = "Ok";
                hitQuality = HitQuality.OK;
                okCount++;
            }
            else
            {
                // Poor timing counts as miss
                hitQuality = HitQuality.Miss;
                missCount++;
            }

            totalHits++;

            // Register hit quality with GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(hitQuality);
            }

            if (staticConsoleEnabled)
            {
                // Console logging
                Debug.Log($"[BlockCollision] Block destroyed with rating: <b>{result}</b> (Δt = {timeSinceBeat:F3}s)");
                Debug.Log($"🎯 Totals — Perfect: {perfectCount}, Good: {goodCount}, Ok: {okCount}, Misses: {missCount}, Total: {totalHits}");
            }

            Destroy(gameObject);
        }
    }

    // Public method to reset all counters
    [ContextMenu("Reset Counters")]
    public static void ResetCounters()
    {
        perfectCount = 0;
        goodCount = 0;
        okCount = 0;
        missCount = 0;
        totalHits = 0;
        
        Debug.Log("[BlockCollision] All counters reset!");
    }
    
    // Public method to get current stats
    public static string GetCurrentStats()
    {
        float accuracy = totalHits > 0 ? (perfectCount + goodCount + okCount) * 100f / totalHits : 0f;
        return $"Perfect: {perfectCount}, Good: {goodCount}, Ok: {okCount}, Misses: {missCount}, Total: {totalHits}, Accuracy: {accuracy:F1}%";
    }
}
