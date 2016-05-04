using UnityEngine;
using System.Collections;

public class MinimapBlip : MonoBehaviour {
    public Transform Target;
    public bool KeepInBounds = true;
    public bool LockScale = false;
    public bool LockRotation = false;
    public float MinScale = 1f;

    Minimap map;
    RectTransform myRectTransform;

    void Start() {
        map = GetComponentInParent<Minimap>();
        myRectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate() {
        Vector2 newPosition = map.TransformPosition(Target.position);

        if (KeepInBounds)
            newPosition = map.MoveInside(newPosition);

        if (!LockScale) {
            float scale = Mathf.Max(MinScale, map.ZoomLevel);
            myRectTransform.localScale = new Vector3(scale, scale, 1);
        }

        if (!LockRotation)
            myRectTransform.localEulerAngles = map.TransformRotation(Target.eulerAngles);

        myRectTransform.localPosition = newPosition;
    }
}
