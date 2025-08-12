using UnityEngine;

public enum BlockType
{
    Left,
    Right
}

public class Block : MonoBehaviour
{
    [Header("Block Configuration")]
    public BlockType blockType = BlockType.Left;
    
    [Header("Visual Effects")]
    public GameObject hitEffect;
    public GameObject missEffect;
    
    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip missSound;
    
    [Header("Scoring")]
    public int scoreValue = 100;
    public int perfectScoreBonus = 50;
    
    private bool hasBeenHit = false;
    private bool hasBeenMissed = false;
    
    

    public void OnBlockHit(Vector3 hitDirection, float hitForce)
    {
        if (hasBeenHit) return; // Prevent multiple hits
        
        hasBeenHit = true;
        
        // Play hit effect
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        
        // Play hit sound
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
        
        // Add score (you'll implement scoring system later)
        AddScore(scoreValue);
        
        // Apply physics if there's a rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(hitDirection * hitForce, ForceMode.Impulse);
        }
        
        // Destroy after a short delay
        Destroy(gameObject, 2f);
    }
    
    public void OnBlockMissed()
    {
        if (hasBeenMissed) return; // Prevent multiple misses
        
        hasBeenMissed = true;
        
        // Play miss effect
        if (missEffect != null)
        {
            Instantiate(missEffect, transform.position, Quaternion.identity);
        }
        
        // Play miss sound
        if (missSound != null)
        {
            AudioSource.PlayClipAtPoint(missSound, transform.position);
        }
        
        // Deduct score or handle miss logic
        HandleMiss();
        
        // Block will be destroyed by BlockMovement when it reaches the player
    }
    
    private void AddScore(int points)
    {
        // Placeholder for scoring system
        Debug.Log($"[Block] Hit! +{points} points");
        
        // You can implement a proper scoring system later
        // Example: GameManager.Instance.AddScore(points);
    }
    
    private void HandleMiss()
    {
        // Placeholder for miss handling
        Debug.Log("[Block] Missed block!");
        
        // You can implement proper miss handling later
        // Example: GameManager.Instance.RegisterMiss();
    }
    
    public BlockType GetBlockType()
    {
        return blockType;
    }
    
    public bool IsCorrectHand(BlockType handType)
    {
        return blockType == handType;
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check for sword collision
        if (other.CompareTag("Sword") || other.name.Contains("Sword"))
        {
            // Determine which hand hit the block
            BlockType hitByHand = BlockType.Left;
            if (other.name.Contains("SwordR") || other.name.Contains("Right"))
            {
                hitByHand = BlockType.Right;
            }
            
            // Check if correct hand hit the block
            if (IsCorrectHand(hitByHand))
            {
                Vector3 hitDirection = (transform.position - other.transform.position).normalized;
                OnBlockHit(hitDirection, 10f);
            }
            else
            {
                // Wrong hand hit the block
                Debug.Log($"[Block] Wrong hand! {blockType} block hit by {hitByHand} hand");
                OnBlockMissed();
            }
        }
    }
}