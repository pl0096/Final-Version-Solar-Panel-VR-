using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

public class CSVParser : MonoBehaviour
{
    [System.Serializable]
    public class SunPosition
    {
        public string month;
        public string time;
        public float azimuth;
        public float altitude;
    }

    public static List<SunPosition> ParseCSV(string csvText)
    {
        List<SunPosition> data = new List<SunPosition>();
        if (string.IsNullOrEmpty(csvText)) return data;

        string[] lines = csvText.Split('\n');
        if (lines.Length < 2) return data;

        // Skip header row if exists
        int startLine = lines[0].ToLower().Contains("month") ? 1 : 0;

        for (int i = startLine; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');
            if (values.Length >= 4)
            {
                try
                {
                    data.Add(new SunPosition
                    {
                        month = values[0].Trim(),
                        time = values[1].Trim(),
                        azimuth = float.Parse(values[2], CultureInfo.InvariantCulture),
                        altitude = float.Parse(values[3], CultureInfo.InvariantCulture)
                    });
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing line {i+1}: {e.Message}");
                }
            }
        }

        return data;
    }
}