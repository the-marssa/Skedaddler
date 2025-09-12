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

    void Update()
    {
        
        if (GameCore.Instance == null || GameCore.Instance.CurrentRun == null || !player) return;

        timer += Time.deltaTime;

        if (!active && timer >= appearEvery)
        {
            transform.position = player.position - player.forward * startDistanceBehind;
            transform.LookAt(player.position);
            timer = 0f;
            active = true;
            return;
        }

        if (active)
        {
            transform.position = Vector3.SmoothDamp(transform.position, player.position, ref vel, 0.1f, speed);
            transform.LookAt(player.position);

            if (Vector3.Distance(transform.position, player.position) <= catchDistance)
            {
                active = false;
                GameCore.Instance.EndRun(died: true, badEnding: true);
            }

            if (timer >= appearDuration) active = false;
        }
    }
}
