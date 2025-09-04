using UnityEngine;
using Lofelt.NiceVibrations;

public static class HapticsManager
{
    public static void Vibrate()
    {
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
    }
}
