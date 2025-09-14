using UnityEngine;

[DisallowMultipleComponent]
public class PlayerPowers : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform attractCenter;
    [SerializeField] private LayerMask collectableMask = ~0;

    [Header("Durations (sec)")]
    [SerializeField] private float magnetDuration = 5f;
    [SerializeField] private float shieldDuration = 5f;
    [SerializeField] private float letterSlowDuration = 5f;

    [Header("Speed Factors")]
    [SerializeField] private float shieldSpeedFactor = 2f;
    [SerializeField] private float letterSlowFactor = 0.5f;

    [Header("Magnet")]
    [SerializeField] private float magnetRadius = 6f;
    [SerializeField] private float magnetPullSpeed = 20f;

    [Header("Obstacle Detection")]
    [Tooltip(" 'Obstacle'")]
    [SerializeField] private LayerMask obstaclesMask;

    [Header("Hit Tuning")]
    [SerializeField] private float hitCooldown = 0.35f;

    [Header("Auto Hitbox (optional)")]
    [SerializeField] private bool autoAddHitbox = true;    
    [SerializeField] private bool ensureTriggerCollider = true;

    private bool magnetActive, shieldActive, slowActive;
    private float magnetT, shieldT, slowT, hitCdT;

    public bool MagnetActive => magnetActive;
    public bool ShieldActive => shieldActive;
    public bool LetterSlowActive => slowActive;
    public float MagnetRemaining => magnetActive ? Mathf.Max(0f, magnetT) : 0f;
    public float ShieldRemaining => shieldActive ? Mathf.Max(0f, shieldT) : 0f;
    public float LetterRemaining => slowActive ? Mathf.Max(0f, slowT) : 0f;
    public float MagnetDuration => magnetDuration;
    public float ShieldDuration => shieldDuration;
    public float LetterDuration => letterSlowDuration;

    void Reset()
    {
        if (!attractCenter) attractCenter = transform;
    }

    void Awake()
    {
        if (autoAddHitbox)
        {
            
            var rb = GetComponent<Rigidbody>();
            if (!rb) rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            
            var col = GetComponent<Collider>();
            if (!col) col = gameObject.AddComponent<CapsuleCollider>();
            if (ensureTriggerCollider) col.isTrigger = true;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (hitCdT > 0f) hitCdT -= dt;

        if (magnetActive) { magnetT -= dt; if (magnetT <= 0f) magnetActive = false; }
        if (shieldActive) { shieldT -= dt; if (shieldT <= 0f) { shieldActive = false; GameCore.Instance?.SetSpeedFactor(1f); } }
        if (slowActive) { slowT -= dt; if (slowT <= 0f) { slowActive = false; GameCore.Instance?.SetSpeedFactor(1f); } }

        
        if (magnetActive && attractCenter)
        {
            var hits = Physics.OverlapSphere(attractCenter.position, magnetRadius, collectableMask, QueryTriggerInteraction.Collide);
            foreach (var h in hits)
            {
                var go = h.attachedRigidbody ? h.attachedRigidbody.gameObject : h.gameObject;
                if (!go || !go.activeInHierarchy) continue;

                var pickup = go.GetComponentInParent<CollectablePickup>();
                if (!pickup) continue;
                if (pickup.kind == CollectableKind.Letter) continue; 

                var dir = (attractCenter.position - go.transform.position).normalized;
                go.transform.position += dir * magnetPullSpeed * dt;
            }
        }

        GameCore.Instance?.TickTime(dt);
    }

    
    void OnTriggerEnter(Collider other) { TryHitIfObstacle(other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject); }
    void OnCollisionEnter(Collision c) { TryHitIfObstacle(c.rigidbody ? c.rigidbody.gameObject : c.gameObject); }

    void TryHitIfObstacle(GameObject go)
    {
        if (!go || !go.activeInHierarchy) return;

        bool isObstacle =
            go.CompareTag("Obstacle") ||
            ((obstaclesMask.value != 0) && ((1 << go.layer) & obstaclesMask.value) != 0);

        if (!isObstacle) return;

        TryObstacleHit(1);
    }

    public bool TryObstacleHit(int dmg = 1, bool withHitPause = true)
    {
        if (ShieldActive) return false;
        if (hitCdT > 0f) return false;

        GameCore.Instance?.ApplyHit(Mathf.Max(1, dmg));
        hitCdT = hitCooldown;

        if (withHitPause)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) { try { pc.HitPause(); } catch { } }
        }
        return true;
    }

    
    public void ActivateMagnet() { magnetActive = true; magnetT = magnetDuration; }
    public void ActivateShield() { shieldActive = true; shieldT = shieldDuration; GameCore.Instance?.SetSpeedFactor(shieldSpeedFactor); }
    public void ActivateLetterSlow() { slowActive = true; slowT = letterSlowDuration; GameCore.Instance?.SetSpeedFactor(letterSlowFactor); }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (attractCenter)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawWireSphere(attractCenter.position, magnetRadius);
        }
    }
#endif
}
