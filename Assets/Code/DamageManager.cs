using UnityEngine;

public class DamageManager : MonoBehaviour {
    [Header("Damage")]
    [Min(1)][SerializeField] private int damageAmount = 10;

    public void DealDamage(GameObject target) {
        if (target == null) {
            return;
        }

        HealthManager targetHealth = target.GetComponent<HealthManager>();

        // Kollidern kan sitta på ett barnobjekt.
        if (targetHealth == null) {
            targetHealth = target.GetComponentInParent<HealthManager>();
        }

        if (targetHealth != null) {
            targetHealth.ApplyDamage(damageAmount, AdminCommands.WeaponInstantKill);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        DealDamage(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other) {
        DealDamage(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        DealDamage(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        DealDamage(other.gameObject);
    }

    private void OnValidate() {
        damageAmount = Mathf.Max(1, damageAmount);
    }
}