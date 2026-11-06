using UnityEngine;

/// <summary>
/// Attach to the player. Nearby objects with an EnemyMovement component
/// damage the player when they come within the configured range.
/// </summary>
public class RespawnProgression : MonoBehaviour {
    [Header("Player stats")]
    public int maxHealth = 100;
    public int health = 100;
    public int damage = 10;

    [Header("Boss stats")]
    public int bossMaxHealth = 100;
    public int bossDamage = 10;

    [Header("Enemy proximity damage")]
    [Min(0f)] public float enemyRange = 2f;
    [Min(0.05f)] public float damageInterval = 1f;
    [Min(0)] public int enemyDamage = 10;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private float nextDamageTime;

    private void Awake() {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        health = maxHealth;
    }

    private void Update() {
        if (Time.time < nextDamageTime) {
            return;
        }

        MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours) {
            if (behaviour == null || behaviour == this || behaviour.GetType().Name != "EnemyMovement") {
                continue;
            }

            if (Vector2.Distance(transform.position, behaviour.transform.position) <= enemyRange) {
                TakeDamage(enemyDamage);
                nextDamageTime = Time.time + damageInterval;
                return;
            }
        }
    }

    // Other scripts can also call this when the player takes damage.
    public void TakeDamage(int amount) {
        health -= amount;
        if (health <= 0) {
            DieAndRespawn();
        }
    }

    private void DieAndRespawn() {
        maxHealth += 50;
        damage += 10;
        bossMaxHealth += 50;
        bossDamage += 11;

        health = maxHealth;
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);

        Rigidbody2D body2D = GetComponent<Rigidbody2D>();
        if (body2D != null) {
            body2D.linearVelocity = Vector2.zero;
            body2D.angularVelocity = 0f;
        }

        Rigidbody body3D = GetComponent<Rigidbody>();
        if (body3D != null) {
            body3D.linearVelocity = Vector3.zero;
            body3D.angularVelocity = Vector3.zero;
        }
    }
}