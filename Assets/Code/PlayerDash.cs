using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyDash : MonoBehaviour {
    [Header("Dash")]
    [SerializeField] private float groundDashForce = 7f;
    [SerializeField] private float jumpDashForce = 12f;
    [SerializeField] private float jumpDashUpForce = 2f;
    [SerializeField] private float jumpDashDelay = 0.8f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.35f;

    private Rigidbody2D body;
    private ParticleSystem dashParticles;
    private MonoBehaviour enemyMovement;
    private float lastGroundedTime, jumpStartedTime, lastDashTime = -10f;
    private float dashQueuedTime;
    private float lastDirectionInputTime = -1f;
    private int lastDirection = 1;
    private bool hasJumped, airDashUsed, movementWasEnabled;
    private bool dashQueued, isDashing;
    private float activeDashSpeed;
    private int dashDirection;
    private Coroutine dashRoutine;
    private bool IsGrounded => Time.time - lastGroundedTime < 0.12f;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        CreateDashParticles();
        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
            if (script != this && script.GetType().Name == "EnemyMovment") { enemyMovement = script; break; }
        lastDirection = transform.right.x < 0f ? -1 : 1;
        lastGroundedTime = Time.time;
    }

    private void Update() {
        if (AdminCommands.ConsoleOpen || AdminCommands.AdminFlying) { dashQueued = false; return; }
        float inputDirection = ReadHorizontalInput();
        if (Mathf.Abs(inputDirection) > 0.01f) {
            lastDirection = inputDirection > 0f ? 1 : -1;
            lastDirectionInputTime = Time.time;
        }
        if (DashKeyPressed()) { dashQueued = true; dashQueuedTime = Time.unscaledTime; }
        // Vänta kort så Shift + Enter hinner öppna admin-konsolen.
        if (!dashQueued || Time.unscaledTime - dashQueuedTime < 0.15f) return;
        dashQueued = false;
        if (Time.time - lastDashTime < dashCooldown) return;
        if (IsGrounded) StartDash(false);
        else if (hasJumped && !airDashUsed && Time.time - jumpStartedTime >= jumpDashDelay) StartDash(true);
    }

    private void FixedUpdate() {
        if (isDashing) {
            body.linearVelocity = new Vector2(dashDirection * activeDashSpeed, body.linearVelocity.y);
            return;
        }
        if (Time.time - lastDirectionInputTime > 0.25f && Mathf.Abs(body.linearVelocity.x) > 0.05f)
            lastDirection = body.linearVelocity.x > 0f ? 1 : -1;
        if (IsGrounded && body.linearVelocity.y <= 0.05f) { hasJumped = false; airDashUsed = false; }
        else if (!hasJumped && body.linearVelocity.y > 0.05f) { hasJumped = true; jumpStartedTime = Time.time; }
    }

    private void OnCollisionEnter2D(Collision2D hit) => CheckGround(hit);
    private void OnCollisionStay2D(Collision2D hit) => CheckGround(hit);

    private void CheckGround(Collision2D hit) {
        foreach (ContactPoint2D point in hit.contacts)
            if (point.normal.y > 0.5f) { lastGroundedTime = Time.time; break; }
    }

    private bool DashKeyPressed() {
        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
            pressed = Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        pressed |= Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
#endif
        return pressed;
    }

    private float ReadHorizontalInput() {
        float direction = 0f;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
            direction = (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1f : 0f)
                      - (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? 1f : 0f);
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (direction == 0f) direction = Input.GetAxisRaw("Horizontal");
#endif
        return direction;
    }

    private void StartDash(bool jumpDash) {
        lastDashTime = Time.time;
        if (jumpDash) airDashUsed = true;
        dashDirection = lastDirection;
        activeDashSpeed = jumpDash ? jumpDashForce : groundDashForce;
        isDashing = true;
        body.linearVelocity = new Vector2(dashDirection * activeDashSpeed,
            jumpDash ? jumpDashUpForce : body.linearVelocity.y);
        EmitDashParticles();

        // Pausa EnemyMovment under dashen så att kraften fungerar.
        movementWasEnabled = enemyMovement != null && enemyMovement.enabled;
        if (movementWasEnabled) enemyMovement.enabled = false;
        dashRoutine = StartCoroutine(EndDash());
    }

    private void CreateDashParticles() {
        GameObject particleObject = new GameObject("Dash Particles");
        particleObject.transform.SetParent(transform, false);
        dashParticles = particleObject.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = dashParticles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.22f);
        main.startSpeed = 0f;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.08f, 0.08f, 0.09f), new Color(0.58f, 0.58f, 0.62f));
        var emission = dashParticles.emission;
        emission.enabled = false;
        var shape = dashParticles.shape;
        shape.enabled = false;

        ParticleSystem.ColorOverLifetimeModule fade = dashParticles.colorOverLifetime;
        fade.enabled = true;
        fade.color = new ParticleSystem.MinMaxGradient(new Gradient {
            colorKeys = new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) }
        });

        ParticleSystemRenderer particlesRenderer = particleObject.GetComponent<ParticleSystemRenderer>();
        particlesRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null) particleShader = Shader.Find("Sprites/Default");
        if (particleShader != null) particlesRenderer.material = new Material(particleShader);
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        particlesRenderer.sortingOrder = sprite != null ? sprite.sortingOrder - 1 : 0;
        if (sprite != null) particlesRenderer.sortingLayerID = sprite.sortingLayerID;
    }

    private void EmitDashParticles() {
        for (int i = 0; i < 12; i++) {
            ParticleSystem.EmitParams particle = new ParticleSystem.EmitParams();
            particle.position = transform.position + new Vector3(-lastDirection * 0.35f, Random.Range(-0.2f, 0.2f), 0f);
            particle.velocity = new Vector3(-lastDirection * Random.Range(2f, 4f), Random.Range(-1.5f, 1.5f), 0f);
            particle.startColor = Random.value < 0.5f ? new Color(0.08f, 0.08f, 0.09f) : new Color(0.58f, 0.58f, 0.62f);
            particle.startSize = Random.Range(0.12f, 0.22f);
            particle.startLifetime = Random.Range(0.25f, 0.55f);
            dashParticles.Emit(particle, 1);
        }
    }

    private IEnumerator EndDash() {
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        if (enemyMovement != null && movementWasEnabled && !AdminCommands.AdminFlying) enemyMovement.enabled = true;
        dashRoutine = null;
    }

    private void OnDisable() {
        if (dashRoutine != null) StopCoroutine(dashRoutine);
        isDashing = false;
        if (enemyMovement != null && movementWasEnabled && !AdminCommands.AdminFlying) enemyMovement.enabled = true;
    }
}