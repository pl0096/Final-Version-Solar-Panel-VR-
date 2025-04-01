using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SolarPanelOptimizer : MonoBehaviour
{
    [Header("UI References")]
    public Slider tiltSlider;
    public TextMeshProUGUI angleText;
    public TextMeshProUGUI powerText;
    public TextMeshProUGUI sunPositionText;

    [Header("Solar References")]
    public Transform solarPanel;
    public SunController sunController;
    public Material panelMaterial; // Assign your solar panel material here

    [Header("Settings")]
    [Range(0.1f, 0.3f)]
    public float extinctionCoefficient = 0.15f;
    public float panelAzimuth = 180f;
    
    [Header("Visual Settings")]
    public float maxEmission = 1f;
    public float minEmission = 0f;
    public Color dayEmissionColor = Color.yellow;
    public Color nightEmissionColor = Color.black;

    void Start()
    {
        tiltSlider.minValue = 0;
        tiltSlider.maxValue = 90;
        tiltSlider.value = 90;
        tiltSlider.onValueChanged.AddListener(UpdateSolarPanel);
        
        UpdateSolarPanel(tiltSlider.value);
    }

    void Update()
    {
        UpdateSolarPanel(tiltSlider.value);
    }

    void UpdateSolarPanel(float tilt)
    {
        if (sunController == null) return;

        // Update panel rotation
        solarPanel.localEulerAngles = new Vector3(tilt, panelAzimuth, 0);
        
        // Get fresh sun data
        float sunAlt = sunController.sunAltitude;
        float sunAzi = sunController.sunAzimuth;
        bool isDay = sunAlt > 0;

        // Calculate power
        float power = isDay ? CalculateSolarPower(tilt, sunAlt, sunAzi) : 0f;

        // Update visual appearance
        UpdatePanelVisuals(isDay, power);

        // Update UI
        angleText.text = $"Panel Angle: {90f - tilt:F0}°";
        powerText.text = isDay ? $"Power: {power:F2} W/m²" : "NIGHT: 0 W/m²";
        powerText.color = isDay ? Color.green : Color.gray;
        
        sunPositionText.text = $"Sun Position:\nAltitude: {sunAlt:F1}°\nAzimuth: {sunAzi:F1}°";
    }

    void UpdatePanelVisuals(bool isDay, float power)
    {
        if (panelMaterial == null) return;
        
        float emissionIntensity = isDay ? Mathf.Clamp01(power / 1000f) : 0f;
        Color emissionColor = Color.Lerp(nightEmissionColor, dayEmissionColor, emissionIntensity);
        
        panelMaterial.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        DynamicGI.UpdateEnvironment();
    }

    float CalculateSolarPower(float panelTilt, float sunAlt, float sunAzi)
    {
        if (sunAlt <= 0) return 0f;

        float sunAltRad = sunAlt * Mathf.Deg2Rad;
        float sunAziRad = sunAzi * Mathf.Deg2Rad;
        float panelTiltRad = panelTilt * Mathf.Deg2Rad;
        float panelAziRad = panelAzimuth * Mathf.Deg2Rad;

        float cosIncidence = Mathf.Sin(sunAltRad) * Mathf.Cos(panelTiltRad) +
                           Mathf.Cos(sunAltRad) * Mathf.Sin(panelTiltRad) * 
                           Mathf.Cos(sunAziRad - panelAziRad);

        float airMass = 1f / Mathf.Max(0.1f, Mathf.Sin(sunAltRad));
        float atmosphericTransmittance = Mathf.Exp(-extinctionCoefficient * airMass);
        
        return 1000f * atmosphericTransmittance * Mathf.Max(0, cosIncidence);
    }
}