using UnityEngine;

public class EnemyMovment : MonoBehaviour {
    enum State {
        IDLE,
        ATTACKING
    }

    State enemyState = State.IDLE;

    // Idle
    [Header("Idle")]
    Vector3 startPostion;
    [SerializeField]
    int walkDistens = 1;
    

    [Header("Movement")]
    [SerializeField]
    int speed;

    bool looksRight = true;

    Rigidbody2D rigidbody;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        rigidbody = GetComponent<Rigidbody2D>();
        startPostion = transform.position;
    }

    void FixedUpdate() {
        if (enemyState == State.IDLE) {
            if (looksRight && transform.position.x < startPostion.x + walkDistens) {
                rigidbody.linearVelocityX = speed * Time.fixedDeltaTime;      
            }
            else if (!looksRight && transform.position.x > startPostion.x - walkDistens) {
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
