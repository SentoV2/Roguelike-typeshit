using UnityEngine;

public class EnemyMovment : MonoBehaviour {
    enum State {
        IDLE,
        ATTACKING
    }

    State enemyState = State.IDLE;

    [Header("Idle")]
    [SerializeField]
    Transform idlePos1;
    [SerializeField]    
    Transform idlePos2;

    [Header("Movement")]
    [SerializeField]
    int speed;

    bool looksRight = true;

    Rigidbody2D rigidbody;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() {

        if (enemyState == State.IDLE) {
            if (looksRight && transform.position.x < idlePos1.position.x) {
                rigidbody.linearVelocityX = speed * Time.fixedDeltaTime;
                return;
            }
            else {
                flip();
            }

            if (!looksRight && transform.position.x > idlePos2.position.x) {
                rigidbody.linearVelocityX = -speed * Time.fixedDeltaTime;
            }
            else {
                flip();
            }
        }
        else {

        }
    }

    void flip() {
        looksRight = !looksRight;
        transform.eulerAngles += new Vector3(0, 180, 0);

    }
}
