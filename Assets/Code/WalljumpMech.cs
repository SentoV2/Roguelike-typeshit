using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class WalljumpMech : MonoBehaviour
{
    [Header("Wall Jump")]
    [SerializeField] private float wallJumpForceX = 18f;
    [SerializeField] private float wallJumpForceY = 12f;
    [SerializeField] private LayerMask wallLayers = ~0;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float phaseTime = 0.3f;

    private Rigidbody2D body;
    private Collider2D wallCollider;
    private Collider2D[] playerColliders;
    private SpriteRenderer playerSprite;
    private readonly List<Collider2D> phasedWalls = new List<Collider2D>();
    private RigidbodyConstraints2D originalConstraints;
    private float originalGravity;
    private bool originalFlipX;
    private bool savedFacing;
    private bool attachedToWall;
    private bool jumpQueued;
    private bool phasing;
    private int jumpDirection;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        originalGravity = body.gravityScale;
        originalConstraints = body.constraints;
        playerColliders = GetComponentsInChildren<Collider2D>();
        playerSprite = GetComponentInChildren<SpriteRenderer>();

        HealthManager health = GetComponent<HealthManager>();
        if (health == null) health = GetComponentInParent<HealthManager>();
        bool isPlayer = gameObject.tag == playerTag || transform.root.gameObject.tag == playerTag ||
                        (health != null && health.CharacterName == "Main");
        if (!isPlayer) enabled = false;
    }

    private void Update()
    {
        if (attachedToWall && JumpPressed()) jumpQueued = true;
    }

    private void FixedUpdate()
    {
        if (!attachedToWall) return;
        body.linearVelocity = Vector2.zero;
        if (!jumpQueued) return;

        Collider2D launchWall = wallCollider;
        body.constraints = originalConstraints;
        body.gravityScale = originalGravity;
        body.linearVelocity = new Vector2(jumpDirection * wallJumpForceX, Mathf.Max(12f, wallJumpForceY));
        RestoreFacing();
        attachedToWall = false;
        wallCollider = null;
        jumpQueued = false;
        if (launchWall != null) StartCoroutine(PhaseOtherWalls(launchWall));
    }

    private void OnCollisionEnter2D(Collision2D collision) => CheckWall(collision);
    private void OnCollisionStay2D(Collision2D collision) => CheckWall(collision);

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider != wallCollider) return;
        attachedToWall = false;
        wallCollider = null;
        jumpQueued = false;
        body.constraints = originalConstraints;
        body.gravityScale = originalGravity;
        RestoreFacing();
    }

    private void CheckWall(Collision2D collision)
    {
        if (phasing || (wallLayers.value & (1 << collision.gameObject.layer)) == 0) return;
        Collider2D wall = collision.collider;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            if (Mathf.Abs(contact.normal.x) < 0.95f || Mathf.Abs(contact.normal.y) > 0.2f) continue;

            float playerX = collision.otherCollider.bounds.center.x;
            jumpDirection = playerX < contact.point.x ? -1 : 1;
            if (Mathf.Abs(playerX - contact.point.x) < 0.01f)
                jumpDirection = contact.normal.x > 0f ? 1 : -1;

            if (!attachedToWall) SaveFacing();
            attachedToWall = true;
            wallCollider = wall;
            body.constraints = originalConstraints | RigidbodyConstraints2D.FreezePositionX |
                               RigidbodyConstraints2D.FreezePositionY;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            FaceAwayFromWall(jumpDirection);
            return;
        }
    }

    private IEnumerator PhaseOtherWalls(Collider2D launchWall)
    {
        phasing = true;
        if (playerColliders.Length == 0)
        {
            phasing = false;
            yield break;
        }
        Bounds playerBounds = playerColliders[0].bounds;
        float distance = Mathf.Abs(wallJumpForceX) * phaseTime + playerBounds.extents.x;
        Vector2 center = body.position + Vector2.right * jumpDirection * distance * 0.5f +
                         Vector2.up * wallJumpForceY * phaseTime * 0.5f;
        Vector2 size = new Vector2(distance + playerBounds.size.x,
                                   wallJumpForceY * phaseTime + playerBounds.size.y);

        foreach (Collider2D wall in Physics2D.OverlapBoxAll(center, size, 0f, wallLayers))
        {
            if (wall == null || wall.isTrigger || wall == launchWall || wall.attachedRigidbody == body ||
                wall.transform.root == launchWall.transform.root || wall.bounds.size.y <= wall.bounds.size.x) continue;

            SetWallCollision(wall, true);
            phasedWalls.Add(wall);
        }

        yield return new WaitForSeconds(phaseTime);
        RestorePhasedWalls();
        phasing = false;
    }

    private void SetWallCollision(Collider2D wall, bool ignore)
    {
        foreach (Collider2D playerCollider in playerColliders)
            if (playerCollider != null && playerCollider.enabled && playerCollider != wall)
                Physics2D.IgnoreCollision(playerCollider, wall, ignore);
    }

    private void RestorePhasedWalls()
    {
        foreach (Collider2D wall in phasedWalls)
            if (wall != null) SetWallCollision(wall, false);
        phasedWalls.Clear();
    }

    private void FaceAwayFromWall(int direction)
    {
        if (playerSprite != null) playerSprite.flipX = direction < 0;
    }

    private void SaveFacing()
    {
        if (savedFacing) return;
        if (playerSprite != null) originalFlipX = playerSprite.flipX;
        savedFacing = true;
    }

    private void RestoreFacing()
    {
        if (!savedFacing) return;
        if (playerSprite != null) playerSprite.flipX = originalFlipX;
        savedFacing = false;
    }

    private bool JumpPressed()
    {
        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null) pressed = Keyboard.current.spaceKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        pressed |= Input.GetKeyDown(KeyCode.Space);
#endif
        return pressed;
    }

    private void OnDisable()
    {
        if (body != null)
        {
            body.constraints = originalConstraints;
            body.gravityScale = originalGravity;
        }
        RestorePhasedWalls();
        attachedToWall = false;
        phasing = false;
        RestoreFacing();
    }

    private void OnValidate()
    {
        wallJumpForceX = Mathf.Max(0f, wallJumpForceX);
        wallJumpForceY = Mathf.Max(12f, wallJumpForceY);
        phaseTime = Mathf.Max(0f, phaseTime);
    }
}
