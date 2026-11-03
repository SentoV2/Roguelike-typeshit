using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class HealthManager : MonoBehaviour {
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

    private void Awake() {
        if (characterName == "Main") CreateDamageParticles();
        ResetHealth();
    }

    public void TakeDamage(int damageAmount) {
        if (IsAlive == false || IsInvulnerable || damageAmount <= 0) {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - damageAmount);
        if (characterName == "Main") EmitDamageParticles();
        HealthChanged?.Invoke(this);

        if (CurrentHealth == 0) {
            Died?.Invoke(this);
            if (destroyWhenDead) Destroy(gameObject);
        }
    }

    public void ApplyDamage(int amount, bool instantKill = false) {
        if (instantKill) Kill();
        else TakeDamage(amount);
    }

    public void Heal(int healAmount) {
        if (IsAlive == false || healAmount <= 0) {
            return;
        }

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + healAmount);
        HealthChanged?.Invoke(this);
    }

    public void SetInvulnerable(bool value) {
        IsInvulnerable = value;
    }

    public void Kill() {
        if (IsAlive == false) return;
        CurrentHealth = 0;
        if (characterName == "Main") EmitDamageParticles();
        HealthChanged?.Invoke(this);
        Died?.Invoke(this);
        if (destroyWhenDead) Destroy(gameObject);
    }

    // Återställer hälsan till startvärdet.
    public void ResetHealth() {
        MaxHealth = Mathf.Max(1, startHealth);
        CurrentHealth = MaxHealth;
        HealthChanged?.Invoke(this);
    }

    private ParticleSystem damageParticles;

    private void CreateDamageParticles() {
        GameObject particleObject = new GameObject("Damage Particles");
        particleObject.transform.SetParent(transform, false);
        damageParticles = particleObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = damageParticles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.07f);
        main.startSpeed = 0f;
        main.startColor = new Color(0.9f, 0.08f, 0.1f);
        var emission = damageParticles.emission;
        emission.enabled = false;
        var shape = damageParticles.shape;
            shape.enabled = false;

        ParticleSystem.ColorOverLifetimeModule fade = damageParticles.colorOverLifetime;
        fade.enabled = true;
        fade.color = new ParticleSystem.MinMaxGradient(new Gradient {
            colorKeys = new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        });

        ParticleSystemRenderer particleRenderer = particleObject.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null) particleShader = Shader.Find("Sprites/Default");
        if (particleShader != null) particleRenderer.material = new Material(particleShader);
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) {
            particleRenderer.sortingLayerID = sprite.sortingLayerID;
            particleRenderer.sortingOrder = sprite.sortingOrder + 1;
        }
    }

    private void EmitDamageParticles() {
        for (int i = 0; i < 12; i++) {
            Vector2 direction = Random.insideUnitCircle.normalized;
            ParticleSystem.EmitParams particle = new ParticleSystem.EmitParams {
                position = transform.position + new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(-0.1f, 0.2f), 0f),
                velocity = new Vector3(direction.x * Random.Range(1f, 2.5f), direction.y * Random.Range(1f, 2.5f), 0f),
                startColor = new Color(Random.Range(0.7f, 1f), 0.03f, 0.06f),
                startSize = Random.Range(0.035f, 0.07f),
                startLifetime = Random.Range(0.2f, 0.4f)
            };
            damageParticles.Emit(particle, 1);
        }
    }
}