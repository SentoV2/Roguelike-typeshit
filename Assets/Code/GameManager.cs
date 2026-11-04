using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField]
    GameObject pauseMeny;

    private static bool gamePaused;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        
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
    

}
