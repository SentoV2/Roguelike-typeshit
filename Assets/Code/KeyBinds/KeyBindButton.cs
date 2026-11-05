using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyBindButton : MonoBehaviour {

    [SerializeField]
    Button resetButton;

    [SerializeField]
    Button hotKeyButton;

    [SerializeField]
    TMP_Text keybindText;

    KeyBind keyBind;

    bool rebinding = false;

    public void init(KeyBind key) {
        keyBind = key;
        keybindText.text = keyBind.getName();
        updatetext();
        resetButton.onClick.AddListener(reset);
        hotKeyButton.onClick.AddListener(rebind);

    }

    private void Update() {
        if (!rebinding) {
            return;
        }

        foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
            if (Input.GetKey(vKey)) {
                //your code here
                keyBind.setBind(vKey);
                updatetext();
                rebinding = false;
            }
        }

    }


    void rebind() {
        rebinding = true;

        hotKeyButton.GetComponentInChildren<TMP_Text>().text = "Rebinding";
    }

    void reset() {
        keyBind.reset();
        updatetext();
    }

    void updatetext() {
        hotKeyButton.GetComponentInChildren<TMP_Text>().text = keyBind.getBind().ToString();
    }

}
