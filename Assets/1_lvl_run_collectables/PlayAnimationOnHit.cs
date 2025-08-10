using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PlayAnimationOnHit : MonoBehaviour
{
    [SerializeField] private Animation anim;
    [SerializeField] private string clipName = "";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool onlyOnce = true;
    [SerializeField] private bool disableColliderOnPlay = true;
    [SerializeField] private float reEnableDelay = 1.5f;

    private bool played;
    private Collider col;

    private void Awake()
    {
        if (!anim) anim = GetComponent<Animation>();
        col = GetComponent<Collider>();
        if (anim)
        {
            anim.playAutomatically = false;
            if (!string.IsNullOrEmpty(clipName) && anim[clipName] != null) anim[clipName].wrapMode = WrapMode.Once;
            else anim.wrapMode = WrapMode.Once;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) Play();
    }

    private void OnCollisionEnter(Collision c)
    {
        if (c.collider.CompareTag(playerTag)) Play();
    }

    private void Play()
    {
        if (onlyOnce && played) return;

        if (anim)
        {
            if (!string.IsNullOrEmpty(clipName)) anim.Play(clipName);
            else anim.Play();
        }

        played = true;

        if (disableColliderOnPlay && col && col.enabled)
        {
            if (onlyOnce || reEnableDelay <= 0f) col.enabled = false;
            else StartCoroutine(ReEnable());
        }
    }

    private IEnumerator ReEnable()
    {
        col.enabled = false;
        yield return new WaitForSeconds(reEnableDelay);
        col.enabled = true;
    }
}
