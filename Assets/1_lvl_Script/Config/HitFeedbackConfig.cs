using UnityEngine;


[CreateAssetMenu(menuName = "Configs/Feedback/Hit")]
public class HitFeedbackConfig : ScriptableObject
{
    [Header("Camera Shake")]
    public float duration = 0.25f;
    public float amplitude = 2f;


    [Header("Haptics")]
    public bool useHaptics = true;


    [Header("VFX/SFX (optional)")]
    public GameObject hitVfxPrefab;
    public AudioClip hitSfx;
}