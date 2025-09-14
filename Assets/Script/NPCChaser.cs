using UnityEngine;

[DisallowMultipleComponent]
public class NPCChaser : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float appearEvery = 40f;
    [SerializeField] private float appearDuration = 6f;
    [SerializeField] private float startDistanceBehind = 8f;
    [SerializeField] private float speed = 15f;
    [SerializeField] private float catchDistance = 1.4f;

    float timer;
    bool active;
    Vector3 vel;

    void OnEnable()
    {
        active = false;
        timer = 0f;
        vel = Vector3.zero;
    }

    void Update()
    {
        if (!player) return;

        timer += Time.deltaTime;

        
        if (!active && timer >= appearEvery)
        {
            active = true;
            timer = 0f;
            var p = player.position - player.forward * startDistanceBehind;
            p.y = player.position.y;
            transform.position = p;
            transform.LookAt(player.position);
        }

      
        if (active)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, player.position, ref vel, 0.1f, speed);
            transform.LookAt(player.position);

            if (Vector3.Distance(transform.position, player.position) <= catchDistance)
            {
                active = false;
                enabled = false; 
                if (GameCore.Instance) GameCore.Instance.EndRun(died: true, badEnding: true);
            }

            if (timer >= appearDuration) active = false;
        }
    }
}
