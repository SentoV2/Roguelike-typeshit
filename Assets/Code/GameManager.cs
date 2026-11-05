using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField]
    GameObject pauseMeny;

    [SerializeField]
    GameObject settingsMeny;

    private static bool gamePaused;

    private static GameObject staticSettingMeny;
    private static GameObject staticPauseMeny;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        staticPauseMeny = pauseMeny;
        staticSettingMeny = settingsMeny;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (!gamePaused) {
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
        pauseMeny.SetActive(false);
        Time.timeScale = 1;
        gamePaused = false;
    }

    public static bool getGamePaused() {
        return gamePaused;
    }
    
    public static GameObject getPauseMeny() {
        return staticPauseMeny;
    }
    public static GameObject getSettingMeny() {
        return staticSettingMeny;
    }

}
