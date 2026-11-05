using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMeny : MonoBehaviour {

    [SerializeField]
    Button startButton;

    [SerializeField]
    Button settingButton;

    [SerializeField]
    GameObject settingMeny;

    [SerializeField]
    string gameScenName = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        startButton.onClick.AddListener(loadGame);
        settingButton.onClick.AddListener(openSettings);
    }

    void loadGame() {
        SceneManager.LoadScene(gameScenName, LoadSceneMode.Single);
    }

    void openSettings() {
        gameObject.SetActive(false);
        settingMeny.SetActive(true);
    }
}
