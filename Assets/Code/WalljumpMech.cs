using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class WalljumpMech : MonoBehaviour {
    [Header("Wall Jump")]
    [SerializeField] private float wallJumpForceX = 18f;
    [SerializeField] private float wallJumpForceY = 6f;
    [SerializeField] private LayerMask wallLayers = ~0;
    [SerializeField] private string playerTag = "Player";

    private Rigidbody2D body;
    private Collider2D wallCollider;
    private Collider2D ignoredWall;
    private SpriteRenderer playerSprite;
    private Collider2D playerCollider;
    private RigidbodyConstraints2D originalConstraints;
    private float originalGravity;
    private int jumpDirection;
    private bool originalFlipX, savedFacing, attachedToWall, jumpQueued;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        originalGravity = body.gravityScale;
        originalConstraints = body.constraints;
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();

        HealthManager health = GetComponent<HealthManager>();
        if (health == null) health = GetComponentInParent<HealthManager>();
        bool isPlayer = gameObject.tag == playerTag || transform.root.gameObject.tag == playerTag ||
                        (health != null && health.CharacterName == "Main");
        if (!isPlayer) enabled = false;
    }

    private void Update() {
        if (attachedToWall && JumpPressed()) jumpQueued = true;
    }

    private void FixedUpdate() {
        if (!attachedToWall) return;
        body.linearVelocity = Vector2.zero;
        if (!jumpQueued) return;

        body.constraints = originalConstraints;
        body.gravityScale = originalGravity;
        body.linearVelocity = new Vector2(jumpDirection * wallJumpForceX, wallJumpForceY);
        RestoreFacing();
        ignoredWall = wallCollider;
        if (ignoredWall != null) Physics2D.IgnoreCollision(playerCollider, ignoredWall, true);
        if (ignoredWall != null) StartCoroutine(RestoreWallCollision(ignoredWall));
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
        jumpQueued = false;
        body.constraints = originalConstraints;
        body.gravityScale = originalGravity;
        RestoreFacing();
    }

    private void CheckWall(Collision2D collision) {
        Collider2D wall = collision.collider;
        if (wall == ignoredWall || (wallLayers.value & (1 << collision.gameObject.layer)) == 0) return;

        foreach (ContactPoint2D contact in collision.contacts) {
            if (Mathf.Abs(contact.normal.x) < 0.95f || Mathf.Abs(contact.normal.y) > 0.2f) continue;
            jumpDirection = bodyColliderCenterX() < wall.bounds.center.x ? -1 : 1;
            if (!attachedToWall) SaveFacing();
            attachedToWall = true;
            wallCollider = wall;
            body.constraints = originalConstraints | RigidbodyConstraints2D.FreezePositionX |
                               RigidbodyConstraints2D.FreezePositionY;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            FaceDirection(jumpDirection);
            return;
        }
    }

    private void FaceDirection(int direction) {
        if (playerSprite != null) playerSprite.flipX = direction < 0;
    }

    private void SaveFacing() {
        if (savedFacing) return;
        if (playerSprite != null) originalFlipX = playerSprite.flipX;
        savedFacing = true;
    }

    private void RestoreFacing() {
        if (!savedFacing) return;
        if (playerSprite != null) playerSprite.flipX = originalFlipX;
        savedFacing = false;
    }

    private float bodyColliderCenterX() {
        return playerCollider.bounds.center.x;
    }

    private IEnumerator RestoreWallCollision(Collider2D wall) {
        yield return new WaitForSeconds(0.1f);
        if (playerCollider != null && wall != null) Physics2D.IgnoreCollision(playerCollider, wall, false);
        if (ignoredWall == wall) ignoredWall = null;
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
        if (body == null) return;
        body.constraints = originalConstraints;
        body.gravityScale = originalGravity;
        if (ignoredWall != null && playerCollider != null)
            Physics2D.IgnoreCollision(playerCollider, ignoredWall, false);
        ignoredWall = null;
        attachedToWall = false;
        RestoreFacing();
    }
}