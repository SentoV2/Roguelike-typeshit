using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SignEditor : MonoBehaviour {
    private const int WordLimit = 300;
    private static readonly string[] ColorNames = { "White", "Black", "Red", "Orange", "Yellow", "Green", "Blue", "Purple" };
    private static readonly Color[] Colors = { Color.white, Color.black, Color.red, new Color(1f, .45f, .1f), Color.yellow, new Color(.2f, .8f, .3f), new Color(.2f, .55f, 1f), new Color(.7f, .35f, 1f) };
    private static readonly string[] SizeNames = { "Tiny", "Small", "Medium", "Large", "Huge" };
    private static readonly float[] Sizes = { .055f, .07f, .09f, .12f, .15f };

    [Header("Sign Text")]
    [SerializeField] private string savedText = "Click to edit this sign";
    [SerializeField] private TextMesh textDisplay;
    [SerializeField] private int colorIndex, sizeIndex = 2;

    private string editText, status = "";
    private int oldColor, oldSize;
    private bool editorOpen;
    private GUIStyle titleStyle, labelStyle, buttonStyle;

    private void Awake() {
        colorIndex = Mathf.Clamp(colorIndex, 0, Colors.Length - 1);
        sizeIndex = Mathf.Clamp(sizeIndex, 0, Sizes.Length - 1);
        if (textDisplay == null) CreateTextDisplay();
        editText = savedText;
        UpdateSign();
    }

    // Unity öppnar editorn när skylten klickas.
    private void OnMouseDown() {
        if (editorOpen || AdminCommands.ConsoleOpen) return;
        editorOpen = true;
        editText = savedText;
        oldColor = colorIndex;
        oldSize = sizeIndex;
        status = "";
    }

    private void OnGUI() {
        if (!editorOpen) return;
        MakeStyles();
        Event key = Event.current;
        if (key.type == EventType.KeyDown && key.keyCode == KeyCode.Escape) { CancelEdit(); key.Use(); return; }

        Rect panel = new Rect(24f, 24f, 430f, 450f);
        Color before = GUI.color;
        GUI.color = new Color(.1f, .12f, .16f, .98f);
        GUI.Box(panel, GUIContent.none);
        GUI.color = before;
        GUILayout.BeginArea(new Rect(panel.x + 16f, panel.y + 12f, panel.width - 32f, panel.height - 20f));
        GUILayout.Label("SIGN EDITOR", titleStyle);
        GUILayout.Label(CountWords(editText) + " / " + WordLimit + " words", labelStyle);

        GUI.SetNextControlName("SignTextInput");
        string changedText = GUILayout.TextArea(editText, GUILayout.Height(205f));
        if (CountWords(changedText) <= WordLimit) { editText = changedText; textDisplay.text = editText; }
        else status = "Word limit reached.";

        GUILayout.Space(8f);
        GUILayout.Label("TEXT COLOR", labelStyle);
        GUILayout.BeginHorizontal();
        for (int i = 0; i < Colors.Length; i++) {
            Color bg = GUI.backgroundColor, fg = GUI.contentColor;
            GUI.backgroundColor = Colors[i];
            GUI.contentColor = i == 1 || i == 4 ? Color.black : Color.white;
            if (GUILayout.Button(ColorNames[i], buttonStyle, GUILayout.Width(46f), GUILayout.Height(26f))) { colorIndex = i; UpdateSign(); }
            GUI.backgroundColor = bg;
            GUI.contentColor = fg;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(6f);
        GUILayout.Label("TEXT SIZE", labelStyle);
        GUILayout.BeginHorizontal();
        for (int i = 0; i < SizeNames.Length; i++)
            if (GUILayout.Toggle(sizeIndex == i, SizeNames[i], GUI.skin.button) && sizeIndex != i) { sizeIndex = i; UpdateSign(); }
        GUILayout.EndHorizontal();

        GUILayout.Space(8f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save", buttonStyle)) SaveText();
        if (GUILayout.Button("Cancel", buttonStyle)) CancelEdit();
        GUILayout.EndHorizontal();
        GUILayout.Label(status, labelStyle);
        GUILayout.EndArea();
    }

    private void CreateTextDisplay() {
        GameObject child = new GameObject("Sign Text");
        child.transform.SetParent(transform, false);
        child.transform.localPosition = new Vector3(0f, 0f, -.01f);
        textDisplay = child.AddComponent<TextMesh>();
        textDisplay.anchor = TextAnchor.MiddleCenter;
        textDisplay.alignment = TextAlignment.Center;
        MeshRenderer mesh = child.GetComponent<MeshRenderer>();
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) { mesh.sortingLayerID = sprite.sortingLayerID; mesh.sortingOrder = sprite.sortingOrder + 1; }
    }

    private void UpdateSign() {
        if (textDisplay == null) return;
        textDisplay.text = editorOpen ? editText : savedText;
        textDisplay.color = Colors[colorIndex];
        textDisplay.fontSize = 64;
        textDisplay.characterSize = Sizes[sizeIndex];
    }

    private void SaveText() { savedText = editText; UpdateSign(); editorOpen = false; }
    private void CancelEdit() { editText = savedText; colorIndex = oldColor; sizeIndex = oldSize; UpdateSign(); editorOpen = false; }
    private int CountWords(string text) => string.IsNullOrWhiteSpace(text) ? 0 : text.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries).Length;

    private void MakeStyles() {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(.8f, .84f, .9f) } };
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 11, fixedHeight = 28f };
    }
}