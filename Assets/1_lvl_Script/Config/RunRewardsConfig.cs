using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Runner/Rewards")]
public class RunRewardsConfig : ScriptableObject
{
    [Header("Collectibles values")]
    public int starScore = 1;    
    public int letterValue = 1;  
    public int heartHeal = 1;   

    [Header("Powerups durations (sec)")]
    [Min(0f)] public float magnetDuration = 5f;
    [Min(0f)] public float shieldDuration = 5f;
}
