using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
public class ToolTips : MonoBehaviour {
    [SerializeField]
    public string toolTipText = "";
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
        guiStyleFore.alignment = TextAnchor.MiddleCenter;
        guiStyleFore.wordWrap = true;
        guiStyleBack = new GUIStyle();
        guiStyleBack.normal.background = backgroundTexture;
        guiStyleBack.normal.textColor = Color.black;
        guiStyleBack.alignment = TextAnchor.MiddleCenter;
        guiStyleBack.wordWrap = true;
    }

    public void OnMouseEnter() {
        mouseIsOnIT = true;
        currentToolTipText = toolTipText;
    }

    public void OnMouseExit() {
        mouseIsOnIT = false;
        currentToolTipText = "";
    }

    void OnGUI() {
        if (currentToolTipText != "") {
            var x = Event.current.mousePosition.x;
            var y = Event.current.mousePosition.y;
            GUI.Label(new Rect(x -150, y - 121, 150, 120), currentToolTipText, guiStyleBack);
            GUI.Label(new Rect(x - 151, y - 120, 150, 120), currentToolTipText, guiStyleFore);
        }
    }
}
