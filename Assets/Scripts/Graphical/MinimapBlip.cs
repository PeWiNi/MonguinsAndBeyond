using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MinimapBlip : MonoBehaviour {
    public Transform Target;
    public bool KeepInBounds = true;
    public bool LockScale = false;
    public bool LockRotation = false;
    public float MinScale = 10f;

    public Minimap map;
    RectTransform myRectTransform;

    [SerializeField]
    Sprite borderSprite;

    Sprite defaultSprite;

    void Start() {
        defaultSprite = GetComponent<Image>().sprite;
        map = GetComponentInParent<Minimap>();
        myRectTransform = GetComponent<RectTransform>();

        float scale = Mathf.Max(MinScale, map.ZoomLevel);
        myRectTransform.localScale = new Vector3(scale, scale, 1);
    }

    void LateUpdate() {
        bool lRot = LockRotation;
        if (!Target) {
            Destroy(gameObject);
            return;
        }
        Vector2 newPosition = map.TransformPosition(Target.position);

        if (!LockScale) {
            float scale = Mathf.Max(MinScale, map.ZoomLevel);
            myRectTransform.localScale = new Vector3(scale, scale, 1);
        }

        if (KeepInBounds) {
            Vector2 pos = map.MoveInside(newPosition);
            if (pos != newPosition) {
                GetComponent<Image>().sprite = borderSprite;
                myRectTransform.localEulerAngles = new Vector3(0, 0, 90 - (Mathf.Rad2Deg * Mathf.Atan2(newPosition.y - pos.y, -newPosition.x - -pos.x)));
                newPosition = pos;
                lRot = true;
                myRectTransform.localScale *= 2;
                myRectTransform.pivot = new Vector2(0.5f, 1);
                GetComponent<Image>().color = Color.green;
            } else {
                GetComponent<Image>().sprite = defaultSprite;
                myRectTransform.pivot = new Vector2(0.5f, 0.5f);
                GetComponent<Image>().color = Color.white;
            }
        }

        if (!lRot)
            myRectTransform.localEulerAngles = map.TransformRotation(Target.eulerAngles);

        myRectTransform.localPosition = newPosition;
    }

    public void SetDefaultSprite(Sprite sprite) {
        defaultSprite = sprite;
        GetComponent<Image>().sprite = sprite;
    }
}
