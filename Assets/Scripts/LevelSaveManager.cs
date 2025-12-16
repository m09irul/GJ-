using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelSaveManager
{
    // ================================
    // Save Level Data
    // ================================
    public static void SaveLevel(int level, int earnedStars, int earnedPackageQuality)
    {
        PlayerPrefs.SetInt(GetStarsKey(level), earnedStars);
        PlayerPrefs.SetInt(GetPackageKey(level), earnedPackageQuality);
        PlayerPrefs.Save();
    }

    // ================================
    // Load Level Data
    // ================================
    public static int GetStars(int level)
    {
        return PlayerPrefs.GetInt(GetStarsKey(level), 0);
    }

    public static int GetPackageQuality(int level)
    {
        return PlayerPrefs.GetInt(GetPackageKey(level), 0);
    }

    // ================================
    // Check If Level Has Saved Data
    // ================================
    public static bool HasLevelData(int level)
    {
        return PlayerPrefs.HasKey(GetStarsKey(level));
    }

    // ================================
    // Delete Level Data
    // ================================
    public static void ClearLevel(int level)
    {
        PlayerPrefs.DeleteKey(GetStarsKey(level));
        PlayerPrefs.DeleteKey(GetPackageKey(level));
    }

    // ================================
    // Internal Key Generators
    // ================================
    private static string GetStarsKey(int level)
    {
        return $"Level_{level}_Stars";
    }

    private static string GetPackageKey(int level)
    {
        return $"Level_{level}_Package";
    }
}
