using System.Collections;
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


    [SerializeField]
    GameObject player;
    [SerializeField]
    float attackRange = 1;

    [SerializeField]
    float distencingPlayer = 1.2f;

    Rigidbody2D rigidbody;

    Vector3 playerPos;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        rigidbody = GetComponent<Rigidbody2D>();
        startPostion = transform.position;
        StartCoroutine(uppdatePlayerPos());
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
            if (Vector3.Distance(transform.position, player.transform.position) < attackRange) {
                enemyState = State.ATTACKING;
            }
        }
        else if (enemyState == State.ATTACKING) {


            if (looksRight && transform.position.x < playerPos.x) {
                if (Vector3.Distance(transform.position, playerPos) > distencingPlayer) {
                    rigidbody.linearVelocityX = speed * Time.fixedDeltaTime;
                }
            }
            else if (!looksRight && transform.position.x > playerPos.x) {
                if (Vector3.Distance(transform.position, playerPos) > distencingPlayer) {
                    rigidbody.linearVelocityX = -speed * Time.fixedDeltaTime;
                }
            }
            else {
                flip();
            }

        }
    }

    void flip() {
        looksRight = !looksRight;
        transform.eulerAngles += new Vector3(0, 180, 0);
    }

    IEnumerator uppdatePlayerPos() {
        HealthManager healthManager = GetComponent<HealthManager>();

        while (healthManager.IsAlive) {
            playerPos = player.transform.position;
            yield return new WaitForSeconds(1f);
        }
    }

}
