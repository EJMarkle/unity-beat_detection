using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockSpawner : MonoBehaviour
{
    [Header("Block Prefabs")]
    [Tooltip("Left hand block prefab (spawned on Low beats)")]
    public GameObject blockLPrefab;
    
    [Tooltip("Right hand block prefab (spawned on Mid beats)")]
    public GameObject blockRPrefab;
    
    [Header("Spawning Settings")]
    [Tooltip("Speed at which blocks move straight out")]
    [Range(1f, 10f)]
    public float blockSpeed = 5f;
    
    [Header("Block Settings")]
    [Tooltip("How long blocks live before being destroyed")]
    [Range(0f, 20f)]
    public float blockLifetime = 10f;
    
    [Header("Beat Response Settings")]
    [Tooltip("Minimum time between beat spawns to prevent spam")]
    [Range(0.05f, 0.5f)]
    public float beatCooldown = 0.1f;
    
    [Tooltip("Intensity threshold for beat spawning (0-100)")]
    [Range(0f, 100f)]
    public float intensityThreshold = 20f;
    
    [Header("Scene Visualization")]
    [Tooltip("Show spawn visualization in scene view")]
    public bool showSpawnArea = true;

    [Header("Console Debug")]
    [Tooltip("Show debug logs")]
    public bool debugMode = false;
    [Tooltip("Show detailed spawn info periodically")]
    public bool showDetailedInfo = false;
    [Tooltip("How often to log detailed spawn info (in seconds)")]
    [Range(1.0f, 10.0f)]
    public float detailedInfoInterval = 5.0f;

    private List<GameObject> activeBlocks = new List<GameObject>();
    
    // Beat detection timing
    private float lastLowBeatTime = -1f;
    private float lastMidBeatTime = -1f;
    
    // Console debug timing
    private float lastDetailedInfoTime = 0f;
    
    void Start()
    {
        // Subscribe to beat events
        BeatEventSystem.OnBeat += OnBeatDetected;
        
        if (debugMode)
        {
            Debug.Log("[BlockSpawner] Initialized and connected to BeatEventSystem");
        }
        
        // Log detailed info on start if enabled
        LogDetailedSpawnInfo();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from beat events to prevent memory leaks
        BeatEventSystem.OnBeat -= OnBeatDetected;
    }
    
    private void OnBeatDetected(object sender, BeatEventArgs e)
    {
        // Check if beat intensity meets threshold
        if (e.Intensity < intensityThreshold)
        {
            return;
        }
        
        // Spawn blocks based on beat type as specified
        switch (e.BeatType)
        {
            case BeatType.Low:
                if (CanSpawnBeat(BeatType.Low))
                {
                    SpawnSpecificBlock(blockLPrefab, BlockType.Left);
                    lastLowBeatTime = Time.time;
                    
                    if (debugMode)
                    {
                        Debug.Log($"[BlockSpawner] LOW BEAT! Spawning BlockL (Left Hand) - Intensity: {e.Intensity:F1}");
                    }
                }
                break;
                
            case BeatType.Mid:
                if (CanSpawnBeat(BeatType.Mid))
                {
                    SpawnSpecificBlock(blockRPrefab, BlockType.Right);
                    lastMidBeatTime = Time.time;
                    
                    if (debugMode)
                    {
                        Debug.Log($"[BlockSpawner] MID BEAT! Spawning BlockR (Right Hand) - Intensity: {e.Intensity:F1}");
                    }
                }
                break;
        }
    }
    
    private bool CanSpawnBeat(BeatType beatType)
    {
        float lastBeatTime = beatType switch
        {
            BeatType.Low => lastLowBeatTime,
            BeatType.Mid => lastMidBeatTime,
            _ => -1f
        };
        
        return Time.time - lastBeatTime >= beatCooldown;
    }
    
    public void SpawnSpecificBlock(GameObject prefab, BlockType blockType)
    {
        if (prefab == null)
        {
            Debug.LogError($"[BlockSpawner] Prefab for {blockType} block is not assigned!");
            return;
        }
        
        // Spawn at this object's position
        Vector3 spawnPosition = transform.position;
        Vector3 moveDirection = transform.forward; // Move straight forward from spawner
        
        // Keep the spawner's rotation
        Quaternion spawnRotation = transform.rotation;
        
        // Spawn the block
        GameObject newBlock = Instantiate(prefab, spawnPosition, spawnRotation);
        
        // Ensure the block has the correct type (crucial for hand detection)
        Block blockComponent = newBlock.GetComponent<Block>();
        if (blockComponent != null)
        {
            blockComponent.blockType = blockType;
        }
        else
        {
            // Add Block component if it doesn't exist
            blockComponent = newBlock.AddComponent<Block>();
            blockComponent.blockType = blockType;
            Debug.LogWarning($"[BlockSpawner] Added missing Block component to {newBlock.name} with type {blockType}");
        }
        
        // Set the correct tag based on block type
        switch (blockType)
        {
            case BlockType.Left:
                newBlock.tag = "BlockL";
                break;
            case BlockType.Right:
                newBlock.tag = "BlockR";
                break;
        }
        
        // Add movement component
        BlockMovement movement = newBlock.GetComponent<BlockMovement>();
        if (movement == null)
        {
            movement = newBlock.AddComponent<BlockMovement>();
        }
        
        movement.Initialize(blockSpeed, blockLifetime, moveDirection);
        
        // Add collision detection component
        BlockCollision collision = newBlock.GetComponent<BlockCollision>();
        if (collision == null)
        {
            collision = newBlock.AddComponent<BlockCollision>();
            
        }
        
        // Track active blocks
        activeBlocks.Add(newBlock);
        
        // Remove destroyed blocks from list
        StartCoroutine(RemoveBlockAfterLifetime(newBlock));
        
        if (debugMode)
        {
            Debug.Log($"[BlockSpawner] Spawned {blockType} block ({prefab.name}) at {spawnPosition} moving {moveDirection}");
        }
    }
    
    public void ClearAllBlocks()
    {
        foreach (GameObject block in activeBlocks)
        {
            if (block != null)
            {
                Destroy(block);
                Debug.Log("[BlockSpawner] Destroyed block: " + block.name);
            }
        }
        activeBlocks.Clear();
        
        if (debugMode)
        {
            Debug.Log("[BlockSpawner] Cleared all active blocks");
        }
    }
    
    private Vector3 CalculateSpawnPosition()
    {
        // Blocks spawn at transform.position
        return transform.position;
    }
    
    private IEnumerator RemoveBlockAfterLifetime(GameObject block)
    {
        yield return new WaitForSeconds(blockLifetime);
        
        if (block != null)
        {
            activeBlocks.Remove(block);
            Destroy(block);
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showSpawnArea) return;
        
        // Draw spawner position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        // Draw movement direction arrow (straight forward)
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
    
    void Update()
    {
        // Log detailed info periodically
        LogDetailedSpawnInfo();
    }
    
    void LogDetailedSpawnInfo()
    {
        if (!showDetailedInfo || Time.time - lastDetailedInfoTime < detailedInfoInterval) return;
        
        lastDetailedInfoTime = Time.time;
        
        Debug.Log("=== BLOCK SPAWNER DEBUG ===");
        Debug.Log($"Active Blocks: {activeBlocks.Count}");
        Debug.Log($"Block Speed: {blockSpeed:F1}m/s");
        Debug.Log($"Intensity Threshold: {intensityThreshold:F0}");
        Debug.Log($"Beat Cooldown: {beatCooldown:F2}s");
        Debug.Log("LOW Beats → BlockL (Left Hand)");
        Debug.Log("MID Beats → BlockR (Right Hand)");
        Debug.Log($"Spawning from: {transform.name}");
    }
    
    [ContextMenu("Clear All Blocks")]
    public void ClearAllBlocksFromMenu()
    {
        ClearAllBlocks();
    }
}