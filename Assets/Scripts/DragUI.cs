using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragUI : UIBehaviour, IBeginDragHandler, IDragHandler {
    /// <summary>
    /// The RectTransform that we are able to drag around.
    /// if null: the transform this Component is attatched to is used.
    /// </summary>
    public RectTransform dragObject;

    /// <summary>
    /// The area in which we are able to move the dragObject around.
    /// if null: canvas is used
    /// </summary>
    public RectTransform dragArea;

    public bool dragAreaIsCircle = false;

    private Vector2 originalLocalPointerPosition;
    private Vector3 originalPanelLocalPosition;

    public Vector3 getPosition() {
        return dragObjectInternal.localPosition;
    }

    public Rect getAreaSize() {
        return dragAreaInternal.rect;
    }

    private RectTransform dragObjectInternal {
        get {
            if (dragObject == null)
                return (transform as RectTransform);
            else
                return dragObject;
        }
    }

    private RectTransform dragAreaInternal {
        get {
            if (dragArea == null) {
                RectTransform canvas = transform as RectTransform;
                while (canvas.parent != null && canvas.parent is RectTransform) {
                    canvas = canvas.parent as RectTransform;
                }
                return canvas;
            } else
                return dragArea;
        }
    }

    public void OnBeginDrag(PointerEventData data) {
        originalPanelLocalPosition = dragObjectInternal.localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragAreaInternal, data.position, data.pressEventCamera, out originalLocalPointerPosition);
    }

    public void OnDrag(PointerEventData data) {
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(dragAreaInternal, data.position, data.pressEventCamera, out localPointerPosition)) {
            Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
            dragObjectInternal.localPosition = originalPanelLocalPosition + offsetToOriginal;
        }

        ClampToArea();
    }
    
    // Clamp panel to dragArea
    private void ClampToArea() {
        Vector3 pos = dragObjectInternal.localPosition;

        Vector3 minPosition = dragAreaInternal.rect.min - dragObjectInternal.rect.min;
        Vector3 maxPosition = dragAreaInternal.rect.max - dragObjectInternal.rect.max;

        pos.x = Mathf.Clamp(dragObjectInternal.localPosition.x, minPosition.x, maxPosition.x);
        pos.y = Mathf.Clamp(dragObjectInternal.localPosition.y, minPosition.y, maxPosition.y);

        if(dragAreaIsCircle) { //Constrain to Circle
            double magn = System.Math.Sqrt(pos.x * pos.x + pos.y * pos.y);
            float newx = (float)(magn > maxPosition.x ? (pos.x / magn) * maxPosition.x : pos.x);
            float newy = (float)(magn > maxPosition.y ? (pos.y / magn) * maxPosition.y : pos.y);
            pos = new Vector3(newx, newy, pos.z);
        }

        dragObjectInternal.localPosition = pos;
    }
}