using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private string characterName = "Main";

    [Header("Health")]
    [Min(1)][SerializeField] private int startHealth = 100;
    [SerializeField] private bool destroyWhenDead = false;

    // Namn och hälsa kan läsas från andra skript.
    public string CharacterName { get { return characterName; } }
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }
    public bool IsAlive { get { return CurrentHealth > 0; } }

    private void Awake()
    {
        ResetHealth();
    }

    public void TakeDamage(int damageAmount)
    {
        if (IsAlive == false || damageAmount <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - damageAmount);

        if (CurrentHealth == 0 && destroyWhenDead)
        {
            Destroy(gameObject);
        }
    }

    public void Heal(int healAmount)
    {
        if (IsAlive == false || healAmount <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + healAmount);
    }

    // Återställer hälsan till startvärdet.
    public void ResetHealth()
    {
        MaxHealth = Mathf.Max(1, startHealth);
        CurrentHealth = MaxHealth;
    }
}