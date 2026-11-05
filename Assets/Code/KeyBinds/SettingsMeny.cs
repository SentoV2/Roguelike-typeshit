using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMeny : MonoBehaviour {

    [SerializeField]
    Transform scrollviewObject;
    [SerializeField]
    GameObject keybindButtonPrefab;

    [SerializeField]
    Button backButton;

    [SerializeField]
    GameObject pauseMeny; 



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start() {
        backButton.onClick.AddListener(back);


        int index = 0;
        foreach (KeyBind keyBind in KeyBindManager.GetKeyBinds()) {

            KeyBindButton keyBindButton = Instantiate(keybindButtonPrefab, new Vector3(0,-125*index,0), Quaternion.identity).GetComponent<KeyBindButton>();
            keyBindButton.gameObject.transform.SetParent(scrollviewObject, false);

            keyBindButton.init(keyBind);
            index++;
        }
    }

    void back() {
        gameObject.SetActive(false);
        pauseMeny.SetActive(true);
    }

}
