using System.Collections;
<<<<<<< Updated upstream
using System.Collections.Generic;
=======
>>>>>>> Stashed changes
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WalljumpMech : MonoBehaviour
{
    [Header("Wall Jump")]
    [SerializeField] private float wallJumpForceX = 18f;
<<<<<<< Updated upstream
    [SerializeField] private float wallJumpForceY = 12f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float minimumWallHeight = 2f;
=======
    [SerializeField] private float wallJumpForceY = 6f;
>>>>>>> Stashed changes
    [SerializeField] private LayerMask wallLayers = ~0;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float phaseTime = 0.3f;

    private Rigidbody2D body;
    private Collider2D wallCollider;
    private Collider2D ignoredWall;
    private Collider2D[] playerColliders;
    private SpriteRenderer playerSprite;
<<<<<<< Updated upstream
    private readonly List<Collider2D> phasedWalls = new List<Collider2D>();
    private bool originalFlipX;
    private bool savedFacing;
    private bool wallSliding;
    private bool jumpQueued;
    private bool phasing;
    private int jumpDirection;
=======
    private Collider2D playerCollider;
    private RigidbodyConstraints2D originalConstraints;
    private float originalGravity;
    private int jumpDirection;
    private bool originalFlipX, savedFacing, attachedToWall, jumpQueued;
>>>>>>> Stashed changes

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerColliders = GetComponentsInChildren<Collider2D>();
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();

        HealthManager health = GetComponent<HealthManager>();
        if (health == null) health = GetComponentInParent<HealthManager>();
        bool isPlayer = gameObject.tag == playerTag || transform.root.gameObject.tag == playerTag ||
                        (health != null && health.CharacterName == "Main");
        if (!isPlayer) enabled = false;
    }

    private void Update()
    {
        if (wallSliding && JumpPressed()) jumpQueued = true;
    }

<<<<<<< Updated upstream
    private void FixedUpdate()
    {
        if (!wallSliding) return;
        if (jumpQueued)
        {
            Collider2D launchWall = wallCollider;
            body.linearVelocity = new Vector2(jumpDirection * wallJumpForceX, Mathf.Max(12f, wallJumpForceY));
            ignoredWall = launchWall;
            StopWallSlide();
            if (launchWall != null) StartCoroutine(PhaseOtherWalls(launchWall));
            return;
        }

        Vector2 velocity = body.linearVelocity;
        velocity.x = 0f;
        velocity.y = Mathf.Max(velocity.y, -wallSlideSpeed);
        body.linearVelocity = velocity;
=======
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
>>>>>>> Stashed changes
    }

    private void OnCollisionEnter2D(Collision2D collision) => CheckWall(collision);
    private void OnCollisionStay2D(Collision2D collision) => CheckWall(collision);

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider == ignoredWall) ignoredWall = null;
        if (collision.collider != wallCollider) return;
        StopWallSlide();
    }

    private void CheckWall(Collision2D collision)
    {
        if (phasing || collision.collider == ignoredWall ||
            (wallLayers.value & (1 << collision.gameObject.layer)) == 0) return;

        Collider2D wall = collision.collider;
        if (wall.bounds.size.y <= minimumWallHeight || wall.bounds.size.y <= wall.bounds.size.x ||
            body.linearVelocity.y > 0f) return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            if (Mathf.Abs(contact.normal.x) < 0.95f || Mathf.Abs(contact.normal.y) > 0.2f) continue;

            float side = collision.otherCollider.bounds.center.x - wall.bounds.center.x;
            jumpDirection = Mathf.Abs(side) > 0.01f
                ? (side < 0f ? -1 : 1)
                : (contact.normal.x > 0f ? 1 : -1);

            if (!wallSliding) SaveFacing();
            wallSliding = true;
            wallCollider = wall;
            jumpQueued = false;
            FaceAwayFromWall(jumpDirection);
            return;
        }
    }

    private void StopWallSlide()
    {
        wallSliding = false;
        wallCollider = null;
        jumpQueued = false;
<<<<<<< Updated upstream
        RestoreFacing();
    }

    private IEnumerator PhaseOtherWalls(Collider2D launchWall)
    {
        phasing = true;
        if (phaseTime <= 0f || playerColliders.Length == 0)
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
                wall.bounds.size.y <= wall.bounds.size.x) continue;
            SetWallCollision(wall, true);
            phasedWalls.Add(wall);
=======
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
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
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
>>>>>>> Stashed changes
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

        pressed |= Input.GetKeyDown(KeyBindManager.getKeyCode("jump"));
        return pressed;
    }

<<<<<<< Updated upstream
    private void OnDisable()
    {
        RestorePhasedWalls();
        StopWallSlide();
        ignoredWall = null;
        phasing = false;
=======
    private void OnDisable() {
        if (body == null) return;
        body.constraints = originalConstraints;
        body.gravityScale = originalGravity;
        if (ignoredWall != null && playerCollider != null)
            Physics2D.IgnoreCollision(playerCollider, ignoredWall, false);
        ignoredWall = null;
        attachedToWall = false;
        RestoreFacing();
>>>>>>> Stashed changes
    }

    private void OnValidate()
    {
        wallJumpForceX = Mathf.Max(0f, wallJumpForceX);
        wallJumpForceY = Mathf.Max(12f, wallJumpForceY);
        wallSlideSpeed = Mathf.Max(0f, wallSlideSpeed);
        minimumWallHeight = Mathf.Max(2f, minimumWallHeight);
        phaseTime = Mathf.Max(0f, phaseTime);
    }
}
