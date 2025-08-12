using UnityEngine;

public class BlockMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    
    [Header("Destruction")]
    public float lifetime = 10f;
    
    [Header("Effects")]
    public bool rotateWhileMoving = false;
    public Vector3 rotationSpeed = new Vector3(0, 45f, 0);
    
    private Vector3 moveDirection;
    private float timeAlive;
    private bool isInitialized = false;
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Update lifetime
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifetime)
        {
            DestroyBlock();
            return;
        }
        
        // Move the block straight forward
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        
        // Rotate if enabled
        if (rotateWhileMoving)
        {
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }
    
    public void Initialize(float speed, float blockLifetime, Vector3 direction)
    {
        moveSpeed = speed;
        lifetime = blockLifetime;
        timeAlive = 0f;
        moveDirection = direction.normalized;
        isInitialized = true;
    }
    
    public void Initialize(float speed, float blockLifetime)
    {
        // Use the block's forward direction as default
        Initialize(speed, blockLifetime, transform.forward);
    }
    
    private void DestroyBlock()
    {
        Destroy(gameObject);
    }
    
    void OnDrawGizmos()
    {
        if (!isInitialized) return;
        
        // Draw movement direction
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, moveDirection * 3f);
    }
}