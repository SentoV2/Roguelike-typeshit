using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMeny : MonoBehaviour {

    [SerializeField]
    Button resumeButton;
    [SerializeField]
    Button settingButton;

    [SerializeField]
    Button exitButton;

    [SerializeField]
    string startSceneName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        GameManager gameManager = FindFirstObjectByType<GameManager>();

        resumeButton.onClick.AddListener(gameManager.unPause);
        settingButton.onClick.AddListener(openSettings);
        exitButton.onClick.AddListener(exitGame);
    }

    void exitGame() {
        Time.timeScale = 1;
        SceneManager.LoadScene(startSceneName, LoadSceneMode.Single);
    }

    void openSettings() {
        gameObject.SetActive(false);
        GameManager.getSettingMeny().SetActive(true);
    }
}
