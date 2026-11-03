using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [SerializeField]
    float delay;

    [SerializeField]
    Transform player;

    [SerializeField]
    Vector3 offset = new Vector3(0f,0f,-10f);



    // Update is called once per frame
    void LateUpdate() {
        Vector3 targetPos = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, delay * Time.deltaTime);
    }


}
