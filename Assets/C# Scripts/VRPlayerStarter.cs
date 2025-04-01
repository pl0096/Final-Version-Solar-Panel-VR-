using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class VRPlayerStarter : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Distance behind solar panel (1-2 meters)")]
    [Range(1f, 2f)] public float spawnDistance = 1.5f;
    [Tooltip("Height above roof surface")]
    public float heightAboveRoof = 0.1f;

    [Header("Required References")]
    public Transform buildingRoof;
    public Transform solarPanel;

    [Header("Movement Settings")]
    public float initialMoveSpeed = 0f;
    public float normalMoveSpeed = 1.5f;

    private XROrigin xrOrigin;
    private ContinuousMoveProviderBase moveProvider;

    void Start()
    {
        InitializeVRComponents();
        PositionPlayerBehindPanel();
        Invoke(nameof(EnableMovement), 1f);
    }

    void InitializeVRComponents()
    {
        xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin == null)
        {
            Debug.LogError("XROrigin not found in scene!", this);
            enabled = false;
            return;
        }
        moveProvider = FindObjectOfType<ContinuousMoveProviderBase>();
        if (moveProvider != null) moveProvider.moveSpeed = initialMoveSpeed;
    }

    void PositionPlayerBehindPanel()
    {
        if (!AreReferencesValid()) return;

        // 1. Calculate position 1-2m behind panel (using negative forward)
        Vector3 spawnPosition = solarPanel.position - solarPanel.forward * spawnDistance;
        spawnPosition.y = buildingRoof.position.y + heightAboveRoof;

        // 2. Force world-Z forward direction (ignoring panel rotation)
        Vector3 worldForward = Vector3.forward;

        // 3. Collision check - move further back if needed
        if (Physics.CheckSphere(spawnPosition, 0.5f))
        {
            spawnPosition -= solarPanel.forward * 0.5f; // Extra 0.5m back
            Debug.Log($"Adjusted spawn position to {Vector3.Distance(spawnPosition, solarPanel.position):F1}m behind panel");
        }

        // 4. Apply position and rotation
        xrOrigin.transform.position = spawnPosition;
        xrOrigin.transform.rotation = Quaternion.LookRotation(worldForward);
    }

    void EnableMovement() => moveProvider.moveSpeed = normalMoveSpeed;

    bool AreReferencesValid()
    {
        if (xrOrigin == null) return LogError("XROrigin missing!");
        if (buildingRoof == null) return LogError("Roof reference missing!");
        if (solarPanel == null) return LogError("Solar panel reference missing!");
        return true;
    }

    bool LogError(string message)
    {
        Debug.LogError(message, this);
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (!buildingRoof || !solarPanel) return;

        // Visualize spawn range (1-2m behind panel)
        Vector3 minSpawn = solarPanel.position - solarPanel.forward * 1f;
        Vector3 maxSpawn = solarPanel.position - solarPanel.forward * 2f;
        minSpawn.y = maxSpawn.y = buildingRoof.position.y + heightAboveRoof;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(minSpawn, 0.15f);
        Gizmos.DrawSphere(maxSpawn, 0.15f);
        Gizmos.DrawLine(minSpawn, maxSpawn);

        // Show current spawn position
        Vector3 spawnPos = solarPanel.position - solarPanel.forward * spawnDistance;
        spawnPos.y = buildingRoof.position.y + heightAboveRoof;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(spawnPos, 0.2f);
        Gizmos.DrawLine(spawnPos, spawnPos + Vector3.forward * 2f); // Z-forward direction
    }
}