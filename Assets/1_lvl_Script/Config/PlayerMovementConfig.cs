using UnityEngine;


[CreateAssetMenu(menuName = "Configs/Player/Movement")]
public class PlayerMovementConfig : ScriptableObject
{
    [Header("Lateral movement")]
    public float lateralSpeed = 4f;
    [Range(0.02f, 0.3f)] public float xSmoothTime = 0.10f;
    [Range(0f, 0.3f)] public float inputDeadZone = 0.08f;


    [Header("Jump")]
    public float jumpHeight = 1.9f;
}