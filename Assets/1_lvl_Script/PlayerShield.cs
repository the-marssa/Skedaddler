using System.Collections;
using UnityEngine;
using JSAM;

public class PlayerShield : MonoBehaviour
{
    [SerializeField] private GameObject shieldVisual;

    [Header("Shield loop SFX (JSAM Sound)")]
    [SerializeField] private Run_audiolibrarySounds shieldLoop = Run_audiolibrarySounds.Shield_sfx;
    [SerializeField] private bool play3DFromThis = false; 

    private float _until;
    private Coroutine _expiryRoutine;
    private bool _loopPlaying;

    public bool IsActive => Time.time < _until;
    public float Remaining => Mathf.Max(0f, _until - Time.time);

    private void Awake()
    {
        GameRefs.PlayerShield = this;
        if (shieldVisual) shieldVisual.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameRefs.PlayerShield == this) GameRefs.PlayerShield = null;
        StopLoop();
    }

    private void OnDisable() => StopLoop();

    public void Enable(float seconds)
    {
        float add = Mathf.Max(0f, seconds);
        bool wasInactive = !IsActive;

        _until = Mathf.Max(_until, Time.time + add);

        if (shieldVisual && !shieldVisual.activeSelf) shieldVisual.SetActive(true);

        if (wasInactive) StartLoop();

        if (_expiryRoutine != null) StopCoroutine(_expiryRoutine);
        _expiryRoutine = StartCoroutine(ExpiryWatcher());
    }

    public void Disable()
    {
        _until = 0f;
        if (_expiryRoutine != null) { StopCoroutine(_expiryRoutine); _expiryRoutine = null; }
        StopLoop();
        if (shieldVisual) shieldVisual.SetActive(false);
    }

    private IEnumerator ExpiryWatcher()
    {
        float wait = _until - Time.time;
        if (wait > 0f) yield return new WaitForSeconds(wait);

        _expiryRoutine = null;
        StopLoop();
        if (shieldVisual) shieldVisual.SetActive(false);
    }

    private void StartLoop()
    {
        if (_loopPlaying) return;
        if (play3DFromThis) AudioManager.PlaySound(shieldLoop, transform);
        else AudioManager.PlaySound(shieldLoop);
        _loopPlaying = true;
    }

    private void StopLoop()
    {
        if (!_loopPlaying) return;
        AudioManager.StopSound(shieldLoop);
        _loopPlaying = false;
    }
}
