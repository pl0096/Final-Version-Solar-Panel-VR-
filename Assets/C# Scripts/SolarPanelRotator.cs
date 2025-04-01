using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class SolarPanelRotator : XRGrabInteractable
{
    [Header("Rotation Settings")]
    public float minAngle = 0f;
    public float maxAngle = 90f;
    public float rotationStep = 15f;
    public float smoothFactor = 10f;

    [Header("Input Settings")]
    public InputActionReference gripAction;
    public InputActionReference triggerAction;

    // Simulator keys
    private KeyCode simGripKey = KeyCode.G;
    private KeyCode simTriggerKey = KeyCode.T;

    private float targetAngle;
    private bool useDirectRotation = true;

    protected override void Awake()
    {
        base.Awake();
        targetAngle = transform.localEulerAngles.x;
    }

    void Update()
    {
        // Handle both VR and simulator input
        bool gripPressed = IsGripPressed();
        bool triggerPressed = IsTriggerPressed();

        if (gripPressed)
        {
            useDirectRotation = !useDirectRotation;
            Debug.Log($"Mode: {(useDirectRotation ? "Direct" : "Button")}");
        }

        if (!useDirectRotation && triggerPressed)
        {
            targetAngle = Mathf.Clamp(targetAngle + rotationStep, minAngle, maxAngle);
            Debug.Log($"Rotated to: {targetAngle}°");
        }

        // Smooth rotation
        float currentAngle = Mathf.LerpAngle(
            transform.localEulerAngles.x, 
            targetAngle, 
            smoothFactor * Time.deltaTime
        );
        transform.localEulerAngles = new Vector3(currentAngle, 0, 0);
    }

    private bool IsGripPressed()
    {
        // Check for real VR input
        if (gripAction?.action?.IsPressed() == true) 
            return true;
        
        // Fallback to simulator key
        return Input.GetKeyDown(simGripKey);
    }

    private bool IsTriggerPressed()
    {
        // Check for real VR input
        if (triggerAction?.action?.IsPressed() == true) 
            return true;
        
        // Fallback to simulator key
        return Input.GetKeyDown(simTriggerKey);
    }

    // Visual debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.5f);
    }
}