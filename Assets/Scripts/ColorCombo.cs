using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorCombo
{
    //Tanager Turquoise
    public static string[] AddNormalScore_3 = new string[3]
    {
        "#078282",
        "#95DBE5",
        "#339E66"
    };
    
    //Radiant Yellow
    public static string[] AddYellowScore_3 = new string[3]
    {
        "#F9A12E",
        "#FC766A",
        "#9B4A97"
    };
    

    public static string[] AddPurpleScore_3 = new string[3]
    {
        "#FF6F61",
        "#C5299B",
        "#963CBD"
    };
    
    //Windsor Wine
    public static string[] DamageText_3 = new string[3]
    {
        "#643E46",
        "#EE2737",
        "#BA0020"
    };
    
    public static string[] RegularText_3 = new string[3]
    {
        "#941E1E",
        "#FFD328",
        "#E92700"
    };

    public static Color32[] Hex2Color32(string[] hexColors)
    {
        Color32[] colors = new Color32[hexColors.Length];

        for (int i = 0; i < hexColors.Length; i++)
        {
            if (ColorUtility.TryParseHtmlString(hexColors[i], out Color color))
            {
                colors[i] = color;
            }
            else
            {
                Debug.LogWarning($"Invalid hex color: {hexColors[i]}");
                colors[i] = new Color32(0, 0, 0, 255); // Default to black if invalid
            }
        }

        return colors;
    }
    
    public static Color[] Hex2Color(string[] hexColors)
    {
        Color[] colors = new Color[hexColors.Length];

        for (int i = 0; i < hexColors.Length; i++)
        {
            if (ColorUtility.TryParseHtmlString(hexColors[i], out Color color))
            {
                colors[i] = color;
            }
            else
            {
                Debug.LogWarning($"Invalid hex color: {hexColors[i]}");
                colors[i] = Color.black; // Default to black if invalid
            }
        }

        return colors;
    }
}
