using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    KeyCode left = KeyCode.D;
    KeyCode right = KeyCode.A;

    [SerializeField]
    int speed = 1;

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

    }
}
