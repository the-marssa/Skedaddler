using UnityEngine;

public class PlayerMagnet : MonoBehaviour
{
    [SerializeField] private float radius = 6f;
    [SerializeField] private float pullSpeed = 10f;
    private float _until;

    public bool IsActive => Time.time < _until;
    public float Remaining => Mathf.Max(0f, _until - Time.time);

    public float Radius => radius;
    public float PullSpeed => pullSpeed;
    public Vector3 Center => transform.position;

    private void Awake() => GameRefs.PlayerMagnet = this;

    public void Enable(float seconds)
    {
        _until = Mathf.Max(_until, Time.time + Mathf.Max(0f, seconds));
    }
}
