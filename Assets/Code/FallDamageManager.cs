using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallDamageManager : MonoBehaviour
{
    [Header("Fall Damage")]
    [Min(0f)][SerializeField] private float minimumFallHeight = 3f;
    [Min(0)][SerializeField] private int fallDamage = 20;
    [SerializeField] private HealthManager healthManager;

    private Rigidbody body;
    private bool falling;
    private float startHeight;

    public float MinimumFallHeight => minimumFallHeight;
    public int FallDamage => fallDamage;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        if (healthManager == null) healthManager = GetComponent<HealthManager>();
        if (healthManager == null) healthManager = GetComponentInParent<HealthManager>();
    }

    private void FixedUpdate()
    {
        if (!falling && body.linearVelocity.y < 0f)
        {
            falling = true;
            startHeight = transform.position.y;
        }
    }

    private void OnCollisionEnter(Collision hit)
    {
        if (!falling || !IsLanding(hit)) return;

        if (startHeight - transform.position.y >= minimumFallHeight && healthManager != null)
            healthManager.ApplyDamage(fallDamage);

        falling = false;
    }

    // Kontrollera att träffen är mot marken.
    private bool IsLanding(Collision hit)
    {
        foreach (ContactPoint point in hit.contacts)
            if (point.normal.y > 0.5f) return true;
        return false;
    }

    // Värdena kan ändras under spelets gång.
    public void SetFallDamage(int amount) => fallDamage = Mathf.Max(0, amount);
    public void SetMinimumFallHeight(float height) => minimumFallHeight = Mathf.Max(0f, height);

    private void OnValidate()
    {
        fallDamage = Mathf.Max(0, fallDamage);
        minimumFallHeight = Mathf.Max(0f, minimumFallHeight);
    }
}