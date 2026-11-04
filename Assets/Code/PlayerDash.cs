using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour {
    [Header("Dash")]
    [SerializeField] private float groundDashForce = 7f;
    [SerializeField] private float jumpDashForce = 12f;
    [SerializeField] private float jumpDashUpForce = 2f;
    [SerializeField] private float upDashForce = 12f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.35f;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundCheckDistance = 0.08f;

    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
    private ParticleSystem dashParticles;
    private MonoBehaviour enemyMovement;
    private float lastGroundedTime, lastDashTime = -10f;
    private float dashQueuedTime;
    private float lastDirectionInputTime = -1f;
    private int lastDirection = 1;
    private bool airDashUsed, movementWasEnabled;
    private bool dashQueued, queuedUpDash, isDashing, isUpDashing;
    private float activeDashSpeed;
    private int dashDirection;
    private Coroutine dashRoutine;
    private bool IsGrounded {
        get {
            if (Time.time - lastGroundedTime < 0.15f) return true;
            if (bodyCollider == null) return false;
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(groundLayers);
            filter.useTriggers = false;
            int count = bodyCollider.Cast(Vector2.down, filter, groundHits, groundCheckDistance);
            for (int i = 0; i < count; i++)
                if (groundHits[i].normal.y > 0.5f) return true;
            return false;
        }
    }

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        CreateDashParticles();
        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
            if (script != this && script.GetType().Name == "PlayerMovement") { enemyMovement = script; break; }
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
        bool upDashPressed = UpDashKeyPressed();
        if (DashKeyPressed() || upDashPressed) {
            dashQueued = true;
            queuedUpDash = upDashPressed;
            dashQueuedTime = Time.unscaledTime;
        }
        // Vänta kort så Shift + Enter hinner öppna admin-konsolen.
        if (!dashQueued || Time.unscaledTime - dashQueuedTime < 0.15f) return;
        bool useUpDash = queuedUpDash;
        dashQueued = false;
        queuedUpDash = false;
        if (Time.time - lastDashTime < dashCooldown) return;
        bool grounded = IsGrounded;
        if (useUpDash && (grounded || !airDashUsed)) StartDash(false, true);
        else if (!useUpDash) StartDash(!grounded);
    }

    private void FixedUpdate() {
        if (isDashing) {
            body.linearVelocity = isUpDashing
                ? new Vector2(0f, activeDashSpeed)
                : new Vector2(dashDirection * activeDashSpeed, body.linearVelocity.y);
            return;
        }
        if (Time.time - lastDirectionInputTime > 0.25f && Mathf.Abs(body.linearVelocity.x) > 0.05f)
            lastDirection = body.linearVelocity.x > 0f ? 1 : -1;
        if (IsGrounded && body.linearVelocity.y <= 0.05f) airDashUsed = false;
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
            pressed = Keyboard.current.leftShiftKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        pressed |= Input.GetKeyDown(KeyCode.LeftShift);
#endif
        return pressed;
    }

    private bool UpDashKeyPressed() {
        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null) pressed = Keyboard.current.qKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        pressed |= Input.GetKeyDown(KeyCode.Q);
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

    private void StartDash(bool jumpDash, bool upDash = false) {
        lastDashTime = Time.time;
        if (jumpDash || (upDash && !IsGrounded)) airDashUsed = true;
        dashDirection = lastDirection;
        isUpDashing = upDash;
        activeDashSpeed = Mathf.Max(0.1f, upDash ? upDashForce : jumpDash ? jumpDashForce : groundDashForce);
        isDashing = true;
        body.linearVelocity = upDash ? new Vector2(0f, activeDashSpeed) : new Vector2(dashDirection * activeDashSpeed,
            jumpDash ? jumpDashUpForce : body.linearVelocity.y);
        EmitDashParticles(upDash);

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

    private void EmitDashParticles(bool upDash = false) {
        for (int i = 0; i < 12; i++) {
            ParticleSystem.EmitParams particle = new ParticleSystem.EmitParams();
            particle.position = transform.position + (upDash
                ? new Vector3(Random.Range(-0.2f, 0.2f), -0.35f, 0f)
                : new Vector3(-lastDirection * 0.35f, Random.Range(-0.2f, 0.2f), 0f));
            particle.velocity = upDash
                ? new Vector3(Random.Range(-1.5f, 1.5f), -Random.Range(2f, 4f), 0f)
                : new Vector3(-lastDirection * Random.Range(2f, 4f), Random.Range(-1.5f, 1.5f), 0f);
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