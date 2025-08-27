using UnityEngine;
using JSAM;

public class PlayerMagnet : MonoBehaviour
{
    [SerializeField] private float radius = 6f;
    [SerializeField] private float pullSpeed = 10f;

    [Header("Loop sound Magnet (Sound Library)")]
    [SerializeField] private Run_audiolibrarySounds magnetLoop = Run_audiolibrarySounds.Magnet_sfx;
    [SerializeField] private bool play3DFromThis = false;

    private float _until;
    private bool _loopPlaying;

    public bool IsActive => Time.time < _until;
    public float Remaining => Mathf.Max(0f, _until - Time.time);
    public float Radius => radius;
    public float PullSpeed => pullSpeed;
    public Vector3 Center => transform.position;


    private void Awake() { GameRefs.PlayerMagnet = this; }
    private void OnDestroy()
    {
        if (GameRefs.PlayerMagnet == this) GameRefs.PlayerMagnet = null;
        if (_loopPlaying) JSAM.AudioManager.StopSound(magnetLoop);
    }

    private void Update()
    {
        if (_loopPlaying && !IsActive)
        {
            JSAM.AudioManager.StopSound(magnetLoop);
            _loopPlaying = false;
        }
    }

    public void Enable(float seconds)
    {
        if (seconds <= 0f) return;

        bool wasInactive = !IsActive;
        _until = Mathf.Max(_until, Time.time + seconds);

        if (wasInactive)
        {
            if (play3DFromThis) JSAM.AudioManager.PlaySound(magnetLoop, transform);
            else JSAM.AudioManager.PlaySound(magnetLoop);
            _loopPlaying = true;
        }
    }
}
