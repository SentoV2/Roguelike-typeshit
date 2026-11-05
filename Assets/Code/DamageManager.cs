using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DamageManager : MonoBehaviour {
    [Header("Attack")]
    [Min(1)][SerializeField] private int damageAmount = 10;
    [SerializeField] private float attackRange = 0.6f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float enemyReach = 0.4f;
    [SerializeField] private LayerMask targetLayers = ~0;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string enemyCharacterName = "Enemy";

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackState = "Attack";
    [SerializeField] private string hitState = "Hit";

    private float lastAttackTime = -10f;
    private float lastEnemyAttackTime = -10f;
    private HealthManager ownHealth;
    private bool IsEnemy => HasTagInHierarchy(transform.root, enemyTag) ||
                            (ownHealth != null && ownHealth.CharacterName == enemyCharacterName);

    private void Awake() {
        ownHealth = GetComponent<HealthManager>();
        if (ownHealth == null) ownHealth = GetComponentInParent<HealthManager>();
        if (ownHealth == null) ownHealth = GetComponentInChildren<HealthManager>(true);
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (animator == null) animator = GetComponentInParent<Animator>();
    }

    private void Update() {
        if (IsEnemy) {
            if (Time.time - lastEnemyAttackTime < attackCooldown) return;
            HealthManager player = FindTarget(enemyReach, IsPlayer);
            if (player != null) {
                if (ApplyDamageTo(player, false)) lastEnemyAttackTime = Time.time;
            }
            return;
        }

        if (!LeftClickPressed() || Time.time - lastAttackTime < attackCooldown) return;
        HealthManager enemy = FindTarget(attackRange, IsEnemyTarget);
        if (enemy == null) return;
        if (ApplyDamageTo(enemy, AdminCommands.WeaponInstantKill)) lastAttackTime = Time.time;
    }

    public void DealDamage(GameObject target) {
        if (target == null) return;
        HealthManager health = target.GetComponent<HealthManager>();
        if (health == null) health = target.GetComponentInParent<HealthManager>();
        if (health == null) health = target.GetComponentInChildren<HealthManager>();
        if (health != null && health.transform.root != transform.root && IsEnemyTarget(health))
            ApplyDamageTo(health, AdminCommands.WeaponInstantKill);
    }

    private bool ApplyDamageTo(HealthManager target, bool instantKill) {
        if (target == null || !target.IsAlive) return false;
        PlayAnimation(animator, attackState);
        int healthBefore = target.CurrentHealth;
        target.ApplyDamage(damageAmount, instantKill);
        if (instantKill || target.CurrentHealth < healthBefore) {
            Animator targetAnimator = target.transform.root.GetComponentInChildren<Animator>(true);
            PlayAnimation(targetAnimator, hitState);
        }
        return true;
    }

    private void PlayAnimation(Animator targetAnimator, string stateName) {
        if (targetAnimator == null || targetAnimator.runtimeAnimatorController == null ||
            string.IsNullOrWhiteSpace(stateName)) return;

        string fullStateName = stateName.Contains(".") ? stateName : "Base Layer." + stateName;
        int stateHash = Animator.StringToHash(fullStateName);
        if (targetAnimator.HasState(0, stateHash)) targetAnimator.Play(stateHash, 0, 0f);
    }

    private HealthManager FindTarget(float range, System.Predicate<HealthManager> isValid) {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, range, targetLayers);
        HealthManager closest = null;
        float closestDistance = range;
        foreach (Collider2D hit in nearby) {
            HealthManager health = hit.GetComponentInParent<HealthManager>();
            if (health == null || !health.IsAlive || health.transform.root == transform.root || !isValid(health)) continue;
            float distance = Vector2.Distance(transform.position, health.transform.position);
            if (distance <= closestDistance) {
                closest = health;
                closestDistance = distance;
            }
        }
        return closest;
    }

    private bool IsEnemyTarget(HealthManager health) {
        return health.CharacterName == enemyCharacterName || HasTagInHierarchy(health.transform.root, enemyTag);
    }

    private bool IsPlayer(HealthManager health) {
        return health.CharacterName == "Main" || HasTagInHierarchy(health.transform.root, playerTag);
    }

    private bool HasTagInHierarchy(Transform root, string tagName) {
        if (string.IsNullOrEmpty(tagName)) return false;
        foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
            if (item.tag == tagName) return true;
        return false;
    }

    private bool LeftClickPressed() {
        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) pressed = Mouse.current.leftButton.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        pressed |= Input.GetMouseButtonDown(0);
#endif
        return pressed;
    }

    private void OnValidate() {
        damageAmount = Mathf.Max(1, damageAmount);
        attackRange = Mathf.Max(0.01f, attackRange);
        attackCooldown = Mathf.Max(0f, attackCooldown);
        enemyReach = Mathf.Max(0.01f, enemyReach);
    }
}