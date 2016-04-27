using UnityEngine;
using System.Collections;

public class ToolTips : MonoBehaviour {
    [SerializeField]
    string toolTipText = "";
    [SerializeField]
    Texture2D backgroundTexture;

    string currentToolTipText = "";
    GUIStyle guiStyleFore;
    GUIStyle guiStyleBack;

    int clickLeft = 0;
    bool mouseIsOnIT = false;

    void Start() {
        guiStyleFore = new GUIStyle();
        guiStyleFore.normal.textColor = Color.white;
        guiStyleFore.alignment = TextAnchor.UpperCenter;
        guiStyleFore.wordWrap = true;
        guiStyleBack = new GUIStyle();
        guiStyleBack.normal.background = backgroundTexture;
        guiStyleBack.normal.textColor = Color.black;
        guiStyleBack.alignment = TextAnchor.UpperCenter;
        guiStyleBack.wordWrap = true;
    }

    void OnMouseEnter() {
        mouseIsOnIT = true;
        currentToolTipText = toolTipText;
    }

    void OnMouseExit() {
        mouseIsOnIT = false;
        currentToolTipText = "";
    }

    void OnGUI() {
        if (currentToolTipText != "") {
            var x = Event.current.mousePosition.x;
            var y = Event.current.mousePosition.y;
            GUI.Label(new Rect(x, y - 101, 100, 100), currentToolTipText, guiStyleBack);
            GUI.Label(new Rect(x - 1, y - 100, 100, 100), currentToolTipText, guiStyleFore);
        }
    }
}
