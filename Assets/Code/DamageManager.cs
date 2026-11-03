using UnityEngine;

public class DamageManager : MonoBehaviour
{
    [Header("Damage")]
    [Min(1)][SerializeField] private int damageAmount = 10;

    [Header("Hit Detection")]
    [SerializeField] private bool damageOnCollision = true;
    [SerializeField] private bool damageOnTrigger = false;

    public void DealDamage(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        HealthManager targetHealth = target.GetComponent<HealthManager>();

        // Kollidern kan sitta pÃ¥ ett barnobjekt.
        if (targetHealth == null)
        {
            targetHealth = target.GetComponentInParent<HealthManager>();
        }

        if (targetHealth != null)
        {
            targetHealth.ApplyDamage(damageAmount, AdminCommands.WeaponInstantKill);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (damageOnCollision)
        {
            DealDamage(collision.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (damageOnTrigger)
        {
            DealDamage(other.gameObject);
        }
    }

    private void OnValidate()
    {
        damageAmount = Mathf.Max(1, damageAmount);
    }
}