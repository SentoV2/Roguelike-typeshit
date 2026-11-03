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
    private MonoBehaviour enemyMovement;
    private float lastGroundedTime, jumpStartedTime, lastDashTime = -10f;
    private float dashQueuedTime;
    private bool hasJumped, airDashUsed, movementWasEnabled;
    private bool dashQueued;
    private Coroutine dashRoutine;
    private bool IsGrounded => Time.time - lastGroundedTime < 0.12f;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
            if (script != this && script.GetType().Name == "EnemyMovment") { enemyMovement = script; break; }
        lastGroundedTime = Time.time;
    }

    private void Update() {
        if (AdminCommands.ConsoleOpen || AdminCommands.AdminFlying) { dashQueued = false; return; }
        if (DashKeyPressed()) { dashQueued = true; dashQueuedTime = Time.unscaledTime; }
        // Vänta kort så Shift + Enter hinner öppna admin-konsolen.
        if (!dashQueued || Time.unscaledTime - dashQueuedTime < 0.15f) return;
        dashQueued = false;
        if (Time.time - lastDashTime < dashCooldown) return;
        if (IsGrounded) StartDash(false);
        else if (hasJumped && !airDashUsed && Time.time - jumpStartedTime >= jumpDashDelay) StartDash(true);
    }

    private void FixedUpdate() {
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

    private void StartDash(bool jumpDash) {
        lastDashTime = Time.time;
        if (jumpDash) airDashUsed = true;
        float direction = Mathf.Sign(transform.right.x);
        if (direction == 0f) direction = 1f;
        Vector2 force = Vector2.right * direction * (jumpDash ? jumpDashForce : groundDashForce);
        if (jumpDash) force.y = jumpDashUpForce;
        body.AddForce(force, ForceMode2D.Impulse);

        // Pausa EnemyMovment under dashen så att kraften fungerar.
        movementWasEnabled = enemyMovement != null && enemyMovement.enabled;
        if (movementWasEnabled) enemyMovement.enabled = false;
        dashRoutine = StartCoroutine(EndDash());
    }

    private IEnumerator EndDash() {
        yield return new WaitForSeconds(dashDuration);
        if (enemyMovement != null && movementWasEnabled && !AdminCommands.AdminFlying) enemyMovement.enabled = true;
        dashRoutine = null;
    }

    private void OnDisable() {
        if (dashRoutine != null) StopCoroutine(dashRoutine);
        if (enemyMovement != null && movementWasEnabled && !AdminCommands.AdminFlying) enemyMovement.enabled = true;
    }
}