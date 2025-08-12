using UnityEngine;

public class RunnerCameraTarget : MonoBehaviour
{
    [SerializeField] Transform player;

    [Header("follow")]
    [SerializeField] bool followX = false;  
    [SerializeField] bool followY = true;    
    [SerializeField] bool followZ = true;  

    [Header("road center")]
    [SerializeField] Vector3 basePos = new Vector3(0f, 1.6f, 0f);

    [Header("smooth")]
    [SerializeField] float smooth = 0.12f;

    Vector3 vel;

    void LateUpdate()
    {
        if (!player) return;

        Vector3 target = basePos;
        if (followX) target.x = player.position.x;
        if (followY) target.y = player.position.y;
        if (followZ) target.z = player.position.z;

        transform.position = Vector3.SmoothDamp(transform.position, target, ref vel, smooth);
    }
}
