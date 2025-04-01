using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

public class SunController : MonoBehaviour
{
    [System.Serializable]
    public class SunPosition
    {
        public string month;
        public string time;
        public float azimuth;
        public float altitude;
    }

    [Header("UI References")]
    public TMP_Dropdown monthDropdown;
    public Slider timeSlider;
    public Toggle autoToggle;
    public TextMeshProUGUI timeDisplay;
    public Light directionalLight;
    public Light ambientLight;

    [Header("Lighting Settings")]
    [Range(0.1f, 100f)] public float daySpeed = 30f;
    public float maxSunIntensity = 1.5f;
    public float minSunIntensity = 0.1f;
    public Color dayAmbientColor = new Color(0.7f, 0.7f, 0.7f);
    public Color nightAmbientColor = new Color(0.1f, 0.1f, 0.2f);
    public AnimationCurve lightIntensityCurve;

    private Dictionary<string, List<SunPosition>> monthlyData = new Dictionary<string, List<SunPosition>>();
    public string currentMonth;
    public float currentTime;
    private bool isAnimating;
    private bool isInitialized;

    void Start()
    {
        LoadSunData();
        Initialize();
    }

    void LoadSunData()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("sundata");
        if (csvFile == null)
        {
            Debug.LogError("CSV file not found in Resources!");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        if (lines.Length < 2)
        {
            Debug.LogError("CSV file is empty!");
            return;
        }

        int startLine = lines[0].ToLower().Contains("month") ? 1 : 0;

        for (int i = startLine; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');
            if (values.Length >= 4)
            {
                try
                {
                    SunPosition pos = new SunPosition
                    {
                        month = values[0].Trim(),
                        time = values[1].Trim(),
                        azimuth = float.Parse(values[2], CultureInfo.InvariantCulture),
                        altitude = float.Parse(values[3], CultureInfo.InvariantCulture)
                    };

                    if (!monthlyData.ContainsKey(pos.month))
                        monthlyData[pos.month] = new List<SunPosition>();
                    
                    monthlyData[pos.month].Add(pos);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing line {i+1}: {e.Message}");
                }
            }
        }
    }

    void Initialize()
    {
        if (monthlyData.Count == 0)
        {
            Debug.LogError("No sun data available!");
            return;
        }

        autoToggle.onValueChanged.AddListener(OnToggleAnimation);
        monthDropdown.onValueChanged.AddListener(SetMonth);
        timeSlider.onValueChanged.AddListener(SetTime);

        PopulateMonthDropdown();
        currentMonth = monthDropdown.options[0].text;
        isInitialized = true;

        UpdateUI();
    }

    void Update()
    {
        if (!isInitialized) return;

        if (isAnimating)
        {
            currentTime += Time.deltaTime * daySpeed / 60f;
            if (currentTime >= 24f)
            {
                currentTime = 0f;
                AdvanceMonth();
            }
            UpdateSunPosition();
        }

        UpdateUI();
        UpdateLighting();
    }

    void UpdateLighting()
    {
        if (directionalLight == null || ambientLight == null) return;

        float sunPositionInfluence = Mathf.Clamp01((directionalLight.transform.rotation.eulerAngles.x + 90) / 180f);
        
        directionalLight.intensity = Mathf.Lerp(minSunIntensity, maxSunIntensity, 
            lightIntensityCurve.Evaluate(sunPositionInfluence));
        
        RenderSettings.ambientIntensity = sunPositionInfluence;
        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dayAmbientColor, sunPositionInfluence);
        
        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(0.3f, 1.3f, sunPositionInfluence));
            DynamicGI.UpdateEnvironment();
        }
    }

    void OnToggleAnimation(bool animate)
    {
        isAnimating = animate;
        monthDropdown.interactable = !animate;
        UpdateUI();
    }

    void PopulateMonthDropdown()
    {
        if (monthDropdown == null) return;

        monthDropdown.ClearOptions();
        string[] monthOrder = { "January", "February", "March", "April", "May", "June", 
                              "July", "August", "September", "October", "November", "December" };
        
        List<string> availableMonths = monthOrder.Where(m => monthlyData.ContainsKey(m)).ToList();
        monthDropdown.AddOptions(availableMonths);
    }

    void SetMonth(int monthIndex)
    {
        if (!isInitialized) return;
        currentMonth = monthDropdown.options[monthIndex].text;
        UpdateSunPosition();
        UpdateUI();
    }

    void SetTime(float hours)
    {
        if (!isInitialized) return;
        currentTime = hours;
        UpdateSunPosition();
        UpdateUI();
    }

    void UpdateUI()
    {
        if (!isInitialized || timeDisplay == null) return;
        
        if (timeSlider != null) 
            timeSlider.SetValueWithoutNotify(currentTime);
        
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime % 1) * 60);
        timeDisplay.text = $"{hours:00}:{minutes:00}";
    }

    void AdvanceMonth()
    {
        if (!isInitialized) return;
        int nextIndex = (monthDropdown.value + 1) % monthDropdown.options.Count;
        monthDropdown.value = nextIndex;
        currentMonth = monthDropdown.options[nextIndex].text;
    }

    // Add these public properties to your SunController class
public float sunAzimuth { get; private set; }
public float sunAltitude { get; private set; }

// Then modify the UpdateSunPosition() method to store these values:
void UpdateSunPosition()
{
    if (!monthlyData.ContainsKey(currentMonth)) return;

    var monthData = monthlyData[currentMonth];
    if (monthData.Count == 0) return;

    SunPosition prev = monthData[0];
    SunPosition next = monthData[0];
    
    for (int i = 1; i < monthData.Count; i++)
    {
        if (ParseTime(monthData[i].time) >= currentTime)
        {
            next = monthData[i];
            prev = monthData[i-1];
            break;
        }
    }

    float timePrev = ParseTime(prev.time);
    float timeNext = ParseTime(next.time);
    float t = (timeNext - timePrev) > 0 ? (currentTime - timePrev) / (timeNext - timePrev) : 0;
    
    sunAzimuth = Mathf.LerpAngle(prev.azimuth, next.azimuth, t);
    sunAltitude = Mathf.LerpAngle(prev.altitude, next.altitude, t);

    if (directionalLight != null)
    {
        directionalLight.transform.rotation = Quaternion.Euler(sunAltitude, sunAzimuth, 0);
    }
}

    float ParseTime(string timeStr)
    {
        string[] parts = timeStr.Split(':');
        if (parts.Length != 2 || 
            !float.TryParse(parts[0], out float hours) || 
            !float.TryParse(parts[1], out float minutes))
            return 0f;
        
        return hours + minutes/60f;
    }
}