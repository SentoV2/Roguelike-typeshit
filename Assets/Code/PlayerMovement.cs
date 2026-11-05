using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    // movment veribales
    [Header("Movement")]
    [SerializeField]
    int speed = 100;

    [SerializeField]
    int jumpforce = 50;

    // Grund Check
    [Header("Grund Check")]
    [SerializeField]
    Transform grundCheck;

    [SerializeField]
    float grundCheckDistencs = 0.5f;

    [SerializeField]
    LayerMask grundLayer;


    Animator animator;
    bool OnGrund = true;
    bool looksRight = true;

    Rigidbody2D rigidbody2D;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    void FixedUpdate() {
        // gets the left/right movment
        if (Input.GetKey(KeyBindManager.getKeyCode("right"))) {
            move(speed);
            if (!looksRight) {
                flip();
            }
        }
        else if (Input.GetKey(KeyBindManager.getKeyCode("left"))) {
            move(-speed);
            if (looksRight) {
                flip();
            }
        }
        else {
            move(0);
        }

        // checks if the player is on grund
        OnGrund = Physics2D.OverlapCircle(grundCheck.position, grundCheckDistencs, grundLayer);

        animator.SetBool("Jump", !OnGrund);

        // jumps the player if the player is on grund
        if (Input.GetKey(KeyBindManager.getKeyCode("jump")) && OnGrund) {
            rigidbody2D.AddForceY(jumpforce);
        }

    }

    void move(int speed) {
        rigidbody2D.linearVelocityX = speed * Time.fixedDeltaTime;
        
        animator.SetFloat("walk", Mathf.Abs(rigidbody2D.linearVelocityX));

    }

    void flip() {
        looksRight = !looksRight;
        transform.eulerAngles += new Vector3(0, 180, 0);
    }

}
