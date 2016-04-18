using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AttributeSliders : MonoBehaviour {
    int[] attributes;
    bool[] set;
    public Slider[] sliders;
    public InputField[] texts;
    public int RoleSelection = -1;
    // Use this for initialization
    void Start() {
        attributes = new int[3];
        set = new bool[3];
    }

    // Update is called once per frame
    void Update() {
    }

    public void SliderDragging(int slider) {
        if (RoleSelection < 0)
            FreeModeSliding(slider);
        else
            RoleSliding(RoleSelection, slider);
    }

    void FreeModeSliding(int slider) {
        int availablePoints = 100;
        attributes[slider] = (int)Mathf.Ceil(sliders[slider].value);
        texts[slider].text = "" + attributes[slider];
        set[slider] = true;
        int setCount = 0;
        for (int i = 0; i < sliders.Length; i++) {
            if (set[i]) {
                availablePoints -= (int)Mathf.Ceil(sliders[i].value);
                setCount++;
            }
        }
        if (availablePoints < 0) {
            for (int i = 0; i < sliders.Length; i++) {
                if (i == slider) continue;
                if (attributes[i] == 0) continue;
                attributes[i] += availablePoints / (setCount - 1);
                sliders[i].value = attributes[i];
                texts[i].text = "" + attributes[i];
            }
        }
    }

    void RoleSliding(int role, int slider) {
        float points = 100;
        switch (role) {
            case (0):
                if(slider == 0) {
                    sliders[0].value = Mathf.Clamp(sliders[0].value, 65, 100);
                    points -= sliders[0].value;
                    sliders[1].value = Mathf.Clamp(sliders[1].value, 0, Mathf.Clamp(35, 0, points));
                    points -= sliders[1].value;
                    sliders[2].value = Mathf.Clamp(sliders[2].value, points, Mathf.Clamp(10, 0, points));
                } if(slider == 1) {
                    sliders[1].value = Mathf.Clamp(sliders[1].value, 0, 35);
                    points -= sliders[1].value;
                    sliders[0].value = Mathf.Clamp(sliders[0].value, 65, Mathf.Clamp(100, 65, points));
                    points -= sliders[0].value;
                    sliders[2].value = Mathf.Clamp(sliders[2].value, points, Mathf.Clamp(10, 0, points));
                } if(slider == 2) {
                    sliders[2].value = Mathf.Clamp(sliders[2].value, 0, 10);
                    points -= sliders[2].value;
                    sliders[0].value = Mathf.Clamp(sliders[0].value, 65, Mathf.Clamp(100, 65, points));
                    points -= sliders[0].value;
                    sliders[1].value = Mathf.Clamp(sliders[1].value, points, Mathf.Clamp(35, 0, points));
                }
                break;
            case (1):
                if (slider == 1) {
                    sliders[1].value = Mathf.Clamp(sliders[1].value, 65, 100);
                    points -= sliders[1].value;
                    sliders[2].value = Mathf.Clamp(sliders[2].value, 0, Mathf.Clamp(35, 0, points));
                    points -= sliders[2].value;
                    sliders[0].value = Mathf.Clamp(sliders[0].value, points, Mathf.Clamp(10, 0, points));
                }
                if (slider == 2) {
                    sliders[2].value = Mathf.Clamp(sliders[2].value, 0, 35);
                    points -= sliders[2].value;
                    sliders[1].value = Mathf.Clamp(sliders[1].value, 65, Mathf.Clamp(100, 65, points));
                    points -= sliders[1].value;
                    sliders[0].value = Mathf.Clamp(sliders[0].value, points, Mathf.Clamp(10, 0, points));
                }
                if (slider == 0) {
                    sliders[0].value = Mathf.Clamp(sliders[0].value, 0, 10);
                    points -= sliders[0].value;
                    sliders[1].value = Mathf.Clamp(sliders[1].value, 65, Mathf.Clamp(100, 65, points));
                    points -= sliders[1].value;
                    sliders[2].value = Mathf.Clamp(sliders[2].value, points, Mathf.Clamp(35, 0, points));
                }
                break;
            case (2):
                if (slider == 2) {
                    sliders[2].value = Mathf.Clamp(sliders[2].value, 65, 100);
                    points -= sliders[2].value;
                    sliders[0].value = Mathf.Clamp(sliders[0].value, 0, Mathf.Clamp(35, 0, points));
                    points -= sliders[0].value;
                    sliders[1].value = Mathf.Clamp(sliders[1].value, points, Mathf.Clamp(10, 0, points));
                }
                if (slider == 0) {
                    sliders[0].value = Mathf.Clamp(sliders[0].value, 0, 35);
                    points -= sliders[0].value;
                    sliders[2].value = Mathf.Clamp(sliders[2].value, 65, Mathf.Clamp(100, 65, points));
                    points -= sliders[2].value;
                    sliders[1].value = Mathf.Clamp(sliders[1].value, points, Mathf.Clamp(10, 0, points));
                }
                if (slider == 1) {
                    sliders[1].value = Mathf.Clamp(sliders[1].value, 0, 10);
                    points -= sliders[1].value;
                    sliders[2].value = Mathf.Clamp(sliders[2].value, 65, Mathf.Clamp(100, 65, points));
                    points -= sliders[2].value;
                    sliders[0].value = Mathf.Clamp(sliders[0].value, points, Mathf.Clamp(35, 0, points));
                }
                break;
            default:
                FreeModeSliding(slider);
                break;
        } for (int i = 0; i< sliders.Length; i++) {
            attributes[i] = (int)Mathf.Ceil(sliders[i].value);
            texts[i].text = "" + attributes[i];
        }
    }

    public int[] getAttributes() {
        return attributes;
    }

    public void SetRole(int role) {
        RoleSelection = RoleSelection == role ? -1 : role;
        if (RoleSelection >= 0) {
            sliders[RoleSelection].value = 100;
            RoleSliding(RoleSelection, RoleSelection);
        }
    }
}
