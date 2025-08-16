using System.Collections;
using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    [SerializeField] private GameObject shieldVisual;

    private float _until;                
    private Coroutine _expiryRoutine;   

    public bool IsActive => Time.time < _until;
    public float Remaining => Mathf.Max(0f, _until - Time.time);

    private void Awake()
    {
        GameRefs.PlayerShield = this;
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameRefs.PlayerShield == this) GameRefs.PlayerShield = null;
    }

    public void Enable(float seconds)
    {
        float add = Mathf.Max(0f, seconds);
        _until = Mathf.Max(_until, Time.time + add);   

   
        if (shieldVisual != null && !shieldVisual.activeSelf)
            shieldVisual.SetActive(true);

       
        if (_expiryRoutine != null) StopCoroutine(_expiryRoutine);
        _expiryRoutine = StartCoroutine(ExpiryWatcher());
    }

  
    public void Disable()
    {
        _until = 0f;
        if (_expiryRoutine != null) { StopCoroutine(_expiryRoutine); _expiryRoutine = null; }
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    private IEnumerator ExpiryWatcher()
    {
        float wait = _until - Time.time;
        if (wait > 0f) yield return new WaitForSeconds(wait);

        _expiryRoutine = null;
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }
}
