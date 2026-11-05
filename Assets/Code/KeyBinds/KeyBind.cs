using UnityEngine;

public class KeyBind {
    private KeyCode defaltKey;

    private KeyCode keybind;

    private string keyName = "";

    public KeyBind(KeyCode keyCode, string keyName) {
        this.defaltKey = keyCode;
        this.keybind = keyCode;
        this.keyName = keyName;
    }

    public void setBind(KeyCode key) {
        keybind = key;
    }

    public KeyCode getBind() {
        return keybind;
    }

    public string getName() {
        return keyName;
    }

    public void reset() {
        keybind = defaltKey;
    }

}