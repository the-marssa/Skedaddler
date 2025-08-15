using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    [SerializeField] private GameObject shieldVisual;
    private float _until;

    public bool IsActive => Time.time < _until;
    public float Remaining => Mathf.Max(0f, _until - Time.time);

    private void Awake() => GameRefs.PlayerShield = this;

    private void Update()
    {
        if (shieldVisual) shieldVisual.SetActive(IsActive);
    }

    public void Enable(float seconds)
    {
        _until = Mathf.Max(_until, Time.time + Mathf.Max(0f, seconds));
    }
}
