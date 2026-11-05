using System.Collections.Generic;
using UnityEngine;

public static class KeyBindManager {

    private static List<KeyBind> keybinds = new List<KeyBind>() {
        new KeyBind(KeyCode.A, "left"),
        new KeyBind(KeyCode.D, "right"),
        new KeyBind(KeyCode.Space, "jump"),
        new KeyBind(KeyCode.LeftShift, "dash"),
        new KeyBind(KeyCode.Q, "updash")
    };

    public static List<KeyBind> GetKeyBinds() {
        return keybinds;
    }

    public static KeyCode getKeyCode(string key) {
        KeyCode keyCode = KeyCode.None;
        foreach (KeyBind keyBind in keybinds) {
            if (keyBind.getName().Equals(key)) {
                keyCode = keyBind.getBind();
                break;
            }
        }

        return keyCode;
    }

    public static KeyBind GetKeyBind(string key) {
        KeyBind keyBind1 = null;
        foreach (KeyBind keyBind in keybinds) {
            if (keyBind.getName().Equals(key)) {
                keyBind1 = keyBind;
                break;
            }
        }

        return keyBind1;
    }

    public static void setKeyBind(string key, KeyCode keyCode) {
        foreach (KeyBind keyBind in keybinds) {
            if (keyBind.getName().Equals(key)) {
                keyBind.setBind(keyCode);
                break;
            }
        }
    }
    

}
