using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class WalljumpMech : MonoBehaviour {
    [Header("Wall Jump")]
    [SerializeField] private float wallJumpDelay = 0.5f;
    [SerializeField] private float wallJumpForceX = 8f;
    [SerializeField] private float wallJumpForceY = 9f;
    [SerializeField] private LayerMask wallLayers = ~0;

    private Rigidbody2D body;
    private Collider2D wallCollider;
    private Collider2D ignoredWall;
    private float originalGravity;
    private float wallTouchTime;
    private int jumpDirection;
    private bool attachedToWall;
    private bool jumpQueued;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        originalGravity = body.gravityScale;
    }

    private void Update() {
        if (attachedToWall && JumpPressed()) jumpQueued = true;
    }

    private void FixedUpdate() {
        if (!attachedToWall) return;

        body.linearVelocity = Vector2.zero;
        body.gravityScale = 0f;
        if (!jumpQueued || Time.time - wallTouchTime < wallJumpDelay) return;

        body.gravityScale = originalGravity;
        body.linearVelocity = new Vector2(jumpDirection * wallJumpForceX, wallJumpForceY);
        ignoredWall = wallCollider;
        attachedToWall = false;
        wallCollider = null;
        jumpQueued = false;
    }

    private void OnCollisionEnter2D(Collision2D collision) => CheckWall(collision);
    private void OnCollisionStay2D(Collision2D collision) => CheckWall(collision);

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.collider == ignoredWall) ignoredWall = null;
        if (collision.collider != wallCollider) return;
        attachedToWall = false;
        wallCollider = null;
        body.gravityScale = originalGravity;
    }

    private void CheckWall(Collision2D collision) {
        if (collision.collider == ignoredWall) return;
        if ((wallLayers.value & (1 << collision.gameObject.layer)) == 0) return;
        foreach (ContactPoint2D contact in collision.contacts) {
            if (Mathf.Abs(contact.normal.x) < 0.5f || Mathf.Abs(contact.normal.y) > 0.75f) continue;
            int side = contact.normal.x > 0f ? 1 : -1;
            if (!attachedToWall || wallCollider != collision.collider || jumpDirection != side)
                wallTouchTime = Time.time;
            attachedToWall = true;
            wallCollider = collision.collider;
            jumpDirection = side;
            return;
        }
    }

    private bool JumpPressed() {
        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null) pressed = Keyboard.current.spaceKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        pressed |= Input.GetKeyDown(KeyCode.Space);
#endif
        return pressed;
    }

    private void OnDisable() {
        if (body != null) body.gravityScale = originalGravity;
    }
}