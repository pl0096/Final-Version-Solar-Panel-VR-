using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using TMPro;

public class VRPosterSystem : MonoBehaviour
{
    [Header("Poster Settings")]
    public Texture2D[] posters = new Texture2D[4];
    public Vector3 posterPosition = new Vector3(0, 3.8f, 0);
    public float width = 1f;
    public float height = 2f;
    public float zoomScale = 2f; // How much to zoom (2x default)

    [Header("VR UI Elements")]
    public Canvas uiCanvas;
    public Button nextButton;
    public Button prevButton;
    public Button zoomButton;
    public TextMeshProUGUI counterText;
    public float uiOffset = 0.5f;

    private Renderer posterRenderer;
    private int currentPosterIndex = 0;
    private bool isZoomed = false;
    private Vector3 originalScale;

    void Start()
    {
        // Create and position poster
        GameObject poster = GameObject.CreatePrimitive(PrimitiveType.Quad);
        poster.transform.position = posterPosition;
        originalScale = new Vector3(width, height, 1f);
        poster.transform.localScale = originalScale;
        poster.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        
        // Set up material
        posterRenderer = poster.GetComponent<Renderer>();
        posterRenderer.material = new Material(Shader.Find("Unlit/Texture"));
        UpdatePoster();

        // Configure VR UI
        SetupVRUI();
    }

    void SetupVRUI()
    {
        // Position UI below the poster
        uiCanvas.renderMode = RenderMode.WorldSpace;
        uiCanvas.worldCamera = Camera.main;
        uiCanvas.transform.position = posterPosition + Vector3.down * (height/2 + uiOffset);
        uiCanvas.transform.localScale = Vector3.one * 0.002f;
        
        // Make buttons work with VR
        nextButton.gameObject.AddComponent<XRSimpleInteractable>();
        prevButton.gameObject.AddComponent<XRSimpleInteractable>();
        zoomButton.gameObject.AddComponent<XRSimpleInteractable>();
        
        // Set up button events
        nextButton.onClick.AddListener(NextPoster);
        prevButton.onClick.AddListener(PreviousPoster);
        zoomButton.onClick.AddListener(ToggleZoom);
    }

    public void NextPoster()
    {
        currentPosterIndex = (currentPosterIndex + 1) % posters.Length;
        UpdatePoster();
        PlayHaptic();
    }

    public void PreviousPoster()
    {
        currentPosterIndex = (currentPosterIndex - 1 + posters.Length) % posters.Length;
        UpdatePoster();
        PlayHaptic();
    }

    public void ToggleZoom()
    {
        isZoomed = !isZoomed;
        posterRenderer.transform.localScale = isZoomed ? 
            originalScale * zoomScale : 
            originalScale;
        
        zoomButton.GetComponentInChildren<TextMeshProUGUI>().text = 
            isZoomed ? "Zoom Out" : "Zoom In";
        
        PlayHaptic();
    }

    void UpdatePoster()
    {
        if(posters[currentPosterIndex] != null)
        {
            posterRenderer.material.mainTexture = posters[currentPosterIndex];
            counterText.text = $"Poster {currentPosterIndex + 1}/{posters.Length}";
        }
    }

    void PlayHaptic()
    {
        var controllers = FindObjectsOfType<XRBaseController>();
        foreach(var controller in controllers)
        {
            controller.SendHapticImpulse(0.3f, 0.1f);
        }
    }

    void Update()
    {
        // Make UI face the camera
        if(uiCanvas != null && Camera.main != null)
        {
            uiCanvas.transform.rotation = Quaternion.LookRotation(
                uiCanvas.transform.position - Camera.main.transform.position);
        }
    }
}