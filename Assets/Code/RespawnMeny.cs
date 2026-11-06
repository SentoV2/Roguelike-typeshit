using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RespawnMeny : MonoBehaviour {

    [SerializeField]
    Button respawnButton;

    [SerializeField]
    Button exitButton;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        exitButton.onClick.AddListener(exit);
        respawnButton.onClick.AddListener(respawn);
    }

    void respawn() {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    void exit() {
        Time.timeScale = 1;
        SceneManager.LoadScene("start", LoadSceneMode.Single);
    }
}
