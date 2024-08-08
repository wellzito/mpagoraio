using JetBrains.Annotations;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionSelector : MonoBehaviour
{
    public Dictionary<int, string> regions = new Dictionary<int, string>{
            { 0 , "asia" },
            { 1, "au" },
            { 2, "cae" },
            { 3, "cn" },
            { 4, "eu" },
            { 5, "hk" },
            { 6, "in" },
            { 7, "jp" },
            { 8, "sa" },
            { 9, "kr" },
            { 10, "tr" },
            { 11, "uae" },
            { 12, "us" },
            { 13, "usw" },
            { 14, "ussc" },
    };
    public Dropdown dropdownRegion;

    void Start()
    {
        GetSettings();
        ChangeRegion();
    }

    void GetSettings()
    {
        dropdownRegion.value = PlayerPrefs.GetInt("serverIndex", 0);
    }
    public void ChangeRegion()
    {
        if (dropdownRegion == null)
        {
            Debug.LogError("Dropdown Region is null!!!!!");
            return;
        }

        int index = dropdownRegion.value;
        string region = index >= 1 ? regions[index - 1] : "";
        PlayerPrefs.SetString("server", region);
        PlayerPrefs.SetInt("serverIndex", index);
        Debug.Log($"Region: {region}");
    }
}
