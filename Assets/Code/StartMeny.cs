using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMeny : MonoBehaviour {

    [SerializeField]
    Button startButton;

    [SerializeField]
    string gameScenName = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        startButton.onClick.AddListener(loadGame);
    }

    void loadGame() {
        SceneManager.LoadScene(gameScenName, LoadSceneMode.Single);
    }
}
