using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
public class ToolTips : MonoBehaviour {
    [SerializeField]
    public string toolTipText = "";
    [SerializeField]
    Texture2D backgroundTexture;
    
    [SerializeField]
    float delayMe = 0;

    string currentToolTipText = "";
    GUIStyle guiStyleFore;
    GUIStyle guiStyleBack;
    GUIStyle guiStyleHead;

    int clickLeft = 0;
    bool mouseIsOnIT = false;
    public bool isOn { get { return mouseIsOnIT && currentToolTipText != ""; } }

    void Start() {
        guiStyleFore = new GUIStyle();
        guiStyleFore.normal.textColor = new Color(248f / 255f, 190f / 255f, 2f / 255f);
        guiStyleFore.alignment = TextAnchor.MiddleCenter;
        guiStyleFore.wordWrap = true;
        //guiStyleFore.margin = new RectOffset(5, 5, 5, 5);
        guiStyleFore.padding = new RectOffset(5, 5, 5, 5);
        guiStyleBack = new GUIStyle();
        guiStyleBack.normal.background = backgroundTexture;
        guiStyleBack.normal.textColor = Color.black;
        guiStyleBack.alignment = TextAnchor.MiddleCenter;
        guiStyleBack.wordWrap = true;
        //guiStyleBack.margin = new RectOffset(5, 5, 5, 5);
        guiStyleBack.padding = new RectOffset(5, 5, 5, 5);
        guiStyleHead = new GUIStyle();
        guiStyleFore.normal.textColor = new Color(248f / 255f, 190f / 255f, 2f / 255f);
        guiStyleHead.fontStyle = FontStyle.Bold;
        guiStyleHead.alignment = TextAnchor.MiddleCenter;

        Color darker = new Color(0, 0, 0, .5f);
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.black);
        guiStyleHead.normal.background = tex;
    }

    public void OnMouseEnter() {
        mouseIsOnIT = false;
        currentToolTipText = "";
        StartCoroutine(delayMePlx());
        guiStyleFore.fontSize = Mathf.CeilToInt(Screen.height * 0.02f);
        guiStyleBack.fontSize = Mathf.CeilToInt(Screen.height * 0.02f);
        guiStyleHead.fontSize = Mathf.CeilToInt(Screen.height * 0.03f);
    }

    public void OnMouseExit() {
        mouseIsOnIT = false;
        currentToolTipText = "";
    }

    IEnumerator delayMePlx() {
        mouseIsOnIT = true;
        yield return new WaitForSeconds(delayMe);
        currentToolTipText = toolTipText;
    }

    void OnGUI() {
        if (currentToolTipText != "" && mouseIsOnIT) {
            string[] subpar = currentToolTipText.Split(':');
            string title = subpar[0];
            string text = "";
            for (int i = 1; i < subpar.Length; i++)
                text += subpar[i];

            var x = Event.current.mousePosition.x;
            var y = Event.current.mousePosition.y;

            float width = (Screen.height / 100) * (30);
            float height = (Screen.height / 100) * (16);
            float headHeight = (Screen.height / 100) * (4);

            #region Stay inside Screen
            //Vector2 pos = new Vector2(x, y);
            Vector2 pos = new Vector2(x, y - (height + headHeight));
            var distPastX = pos.x + width - Screen.width;
            if (distPastX > 0)
                pos = new Vector2(pos.x - distPastX, pos.y);
            //var distPastY = pos.y + height - Screen.height;
            //if (distPastY > 0)
            var distPastY = pos.y;
            if (distPastY < 0)
                pos = new Vector2(pos.x, pos.y - distPastY);
            #endregion

            GUI.Label(new Rect(pos.x, pos.y, width, headHeight), title, guiStyleHead);
            GUI.Label(new Rect(pos.x, pos.y + headHeight - 1, width, height), text, guiStyleBack);
            GUI.Label(new Rect(pos.x - 1, pos.y + headHeight, width, height), text, guiStyleFore);
        }
    }
}
