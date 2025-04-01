using System.Collections.Generic;
using UnityEngine;

public class SunDataManager : MonoBehaviour
{
    public Dictionary<string, List<CSVParser.SunPosition>> monthlyData = 
        new Dictionary<string, List<CSVParser.SunPosition>>();

    void Start()
    {
        LoadAndOrganizeData();
    }

    public void LoadAndOrganizeData()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("sundata");
        if (csvFile == null)
        {
            Debug.LogError("CSV file not found in Resources!");
            return;
        }

        List<CSVParser.SunPosition> allData = CSVParser.ParseCSV(csvFile.text);
        if (allData == null || allData.Count == 0)
        {
            Debug.LogError("No valid data parsed from CSV!");
            return;
        }

        OrganizeByMonth(allData);
        Debug.Log($"Loaded {allData.Count} sun positions across {monthlyData.Count} months");
    }

    void OrganizeByMonth(List<CSVParser.SunPosition> data)
    {
        monthlyData.Clear();
        
        foreach (CSVParser.SunPosition pos in data)
        {
            if (!monthlyData.ContainsKey(pos.month))
            {
                monthlyData[pos.month] = new List<CSVParser.SunPosition>();
            }
            monthlyData[pos.month].Add(pos);
        }
    }
}