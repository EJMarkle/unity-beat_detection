using UnityEngine;

public class ShaderPulseTrigger : MonoBehaviour
{
    public Renderer targetRenderer;
    public string shaderProperty = "_PulseAmount";

    [Header("Pulse Settings")]
    public float pulseValue = 1f;
    public float decaySpeed = 2f;

    private Material materialInstance;
    private float currentValue = 0f;

    void Start()
    {
        if (targetRenderer != null)
        {
            // Make sure we're not editing the shared material
            materialInstance = targetRenderer.material;
        }
    }

    void Update()
    {
        if (materialInstance == null) return;

        // Decay the value back to 0 over time
        if (currentValue > 0f)
        {
            currentValue = Mathf.MoveTowards(currentValue, 0f, Time.deltaTime * decaySpeed);
            materialInstance.SetFloat(shaderProperty, currentValue);
        }
    }

    // Call this from UnityEvent or script
    public void TriggerPulse()
    {
        currentValue = pulseValue;
        materialInstance.SetFloat(shaderProperty, currentValue);
    }
}
