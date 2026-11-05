using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DamageManager : MonoBehaviour
{
    [Header("Attack")]
    [Min(1)][SerializeField] private int damageAmount = 10;
    [SerializeField] private float attackRange = 0.6f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float enemyReach = 0.4f;
    [SerializeField] private LayerMask targetLayers = ~0;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private string playerTag = "Player";

    private float lastAttackTime = -10f;
    private float lastEnemyAttackTime = -10f;
    private bool IsEnemy => gameObject.tag == enemyTag || transform.root.gameObject.tag == enemyTag;

    private void Update()
    {
        if (AdminCommands.ConsoleOpen || AdminCommands.AdminFlying) return;
        if (IsEnemy)
        {
            if (Time.time - lastEnemyAttackTime < attackCooldown) return;
            HealthManager player = FindTarget(enemyReach, IsPlayer);
            if (player != null)
            {
                player.ApplyDamage(damageAmount, AdminCommands.WeaponInstantKill);
                lastEnemyAttackTime = Time.time;
            }
            return;
        }

        if (!LeftClickPressed() || Time.time - lastAttackTime < attackCooldown) return;
        HealthManager enemy = FindTarget(attackRange, IsEnemyTarget);
        if (enemy == null) return;
        enemy.ApplyDamage(damageAmount, AdminCommands.WeaponInstantKill);
        lastAttackTime = Time.time;
    }

    public void DealDamage(GameObject target)
    {
        if (target == null) return;
        HealthManager health = target.GetComponent<HealthManager>();
        if (health == null) health = target.GetComponentInParent<HealthManager>();
        if (health != null && health.transform.root != transform.root && IsEnemyTarget(health))
            health.ApplyDamage(damageAmount, AdminCommands.WeaponInstantKill);
    }

    private HealthManager FindTarget(float range, System.Predicate<HealthManager> isValid)
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, range, targetLayers);
        HealthManager closest = null;
        float closestDistance = range;
        foreach (Collider2D hit in nearby)
        {
            HealthManager health = hit.GetComponentInParent<HealthManager>();
            if (health == null || !health.IsAlive || health.transform.root == transform.root || !isValid(health)) continue;
            float distance = Vector2.Distance(transform.position, health.transform.position);
            if (distance <= closestDistance)
            {
                closest = health;
                closestDistance = distance;
            }
        }
        return closest;
    }

    private bool IsEnemyTarget(HealthManager health)
    {
        return health.gameObject.tag == enemyTag || health.transform.root.gameObject.tag == enemyTag;
    }

    private bool IsPlayer(HealthManager health)
    {
        return health.CharacterName == "Main" || health.gameObject.tag == playerTag || health.transform.root.gameObject.tag == playerTag;
    }

    private bool LeftClickPressed()
    {
        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) pressed = Mouse.current.leftButton.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        pressed |= Input.GetMouseButtonDown(0);
#endif
        return pressed;
    }

    private void OnValidate()
    {
        damageAmount = Mathf.Max(1, damageAmount);
        attackRange = Mathf.Max(0.01f, attackRange);
        attackCooldown = Mathf.Max(0f, attackCooldown);
        enemyReach = Mathf.Max(0.01f, enemyReach);
    }
}