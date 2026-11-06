using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField]
    GameObject pauseMeny;

    [SerializeField]
    GameObject settingsMeny;

    [SerializeField]
    GameManager deathMeny;

    private static bool gamePaused;

    private static GameObject staticSettingMeny;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        staticSettingMeny = settingsMeny;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (!gamePaused) {
                if (getSettingMeny().activeSelf == true) {
                    return;
                }
                pauseMeny.SetActive(true);
                Time.timeScale = 0;
                gamePaused = true;
            }
            else {
                unPause();
            }
        }

    }

    public void unPause() {
        if (pauseMeny.activeSelf == true) {
            pauseMeny.SetActive(false);
        }
        Time.timeScale = 1;
        gamePaused = false;
    }

    public static bool getGamePaused() {
        return gamePaused;
    }
    
    public static GameObject getSettingMeny() {
        return staticSettingMeny;
    }

}
