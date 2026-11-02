using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    // Keys
    KeyCode left = KeyCode.D;
    KeyCode right = KeyCode.A;

    KeyCode jump = KeyCode.Space;



    [SerializeField]
    int speed = 100;

    [SerializeField]
    int jumpforce = 50;

    // Grund Check
    [SerializeField]
    Transform grundCheck;

    [SerializeField]
    float grundCheckDistencs = 0.5f;

    [SerializeField]
    LayerMask grundLayer;


    bool OnGrund = true;

    Rigidbody2D rigidbody2D;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update() {
        
    }

    void FixedUpdate() {
        
        if (Input.GetKey(left)) {
            rigidbody2D.linearVelocityX = speed * Time.fixedDeltaTime;
        }
        else if (Input.GetKey(right)) {
            rigidbody2D.linearVelocityX = -speed * Time.fixedDeltaTime;
        }
        else {
            rigidbody2D.linearVelocityX = 0;
        }
        OnGrund = Physics2D.OverlapCircle(grundCheck.position, grundCheckDistencs, grundLayer);

        if (Input.GetKey(jump) && OnGrund) {
            rigidbody2D.AddForceY(jumpforce);
        }

    }
}
