using System;
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
    public bool IsInvulnerable { get; private set; }

    // Andra komponenter kan lyssna på dessa händelser.
    public event Action<HealthManager> HealthChanged;
    public event Action<HealthManager> Died;

    private void Awake()
    {
        ResetHealth();
    }

    public void TakeDamage(int damageAmount)
    {
        if (IsAlive == false || IsInvulnerable || damageAmount <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - damageAmount);
        HealthChanged?.Invoke(this);

        if (CurrentHealth == 0)
        {
            Died?.Invoke(this);
            if (destroyWhenDead) Destroy(gameObject);
        }
    }

    public void ApplyDamage(int amount, bool instantKill = false)
    {
        if (instantKill) Kill();
        else TakeDamage(amount);
    }

    public void Heal(int healAmount)
    {
        if (IsAlive == false || healAmount <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + healAmount);
        HealthChanged?.Invoke(this);
    }

    public void SetInvulnerable(bool value)
    {
        IsInvulnerable = value;
    }

    public void Kill()
    {
        if (IsAlive == false) return;
        CurrentHealth = 0;
        HealthChanged?.Invoke(this);
        Died?.Invoke(this);
        if (destroyWhenDead) Destroy(gameObject);
    }

    // Återställer hälsan till startvärdet.
    public void ResetHealth()
    {
        MaxHealth = Mathf.Max(1, startHealth);
        CurrentHealth = MaxHealth;
        HealthChanged?.Invoke(this);
    }
}