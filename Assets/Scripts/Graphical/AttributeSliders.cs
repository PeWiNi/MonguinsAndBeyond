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

    public void TextToSlider(int slider) {
        sliders[slider].value = int.Parse(GameObject.Find("InputField (" + slider + ")").GetComponent<InputField>().text);
        SliderDragging(slider);
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

    void RoleSliding(int main, int secondary, int minor, int slider) {
        float points = 100;
        if (slider == main) {
            sliders[main].value = Mathf.Clamp(sliders[main].value, 65, 100);
            points -= sliders[main].value;
            sliders[secondary].value = Mathf.Clamp(sliders[secondary].value, 0, Mathf.Clamp(35, 0, points));
            points -= sliders[secondary].value;
            sliders[minor].value = Mathf.Clamp(sliders[minor].value, points, Mathf.Clamp(10, 0, points));
        }
        if (slider == secondary) {
            sliders[secondary].value = Mathf.Clamp(sliders[secondary].value, 0, 35);
            points -= sliders[secondary].value;
            if (sliders[main].value >= 75) {
                sliders[minor].value = Mathf.Clamp(sliders[minor].value, 0, Mathf.Clamp(10, 0, points));
                points -= sliders[minor].value;
                sliders[main].value = Mathf.Clamp(sliders[main].value, 65, Mathf.Clamp(100, 65, points));
            } else {
                sliders[main].value = Mathf.Clamp(sliders[main].value, 65, Mathf.Clamp(100, 65, points));
                points -= sliders[main].value;
                sliders[minor].value = Mathf.Clamp(sliders[minor].value, points, Mathf.Clamp(10, 0, points));
            }
        }
        if (slider == minor) {
            sliders[minor].value = Mathf.Clamp(sliders[minor].value, 0, 10);
            points -= sliders[minor].value;
            if (sliders[main].value >= 75) {
                sliders[secondary].value = Mathf.Clamp(sliders[secondary].value, 0, Mathf.Clamp(35, 0, points));
                points -= sliders[secondary].value;
                sliders[main].value = Mathf.Clamp(sliders[main].value, 65, Mathf.Clamp(100, 65, points));
            } else {
                sliders[main].value = Mathf.Clamp(sliders[main].value, 65, Mathf.Clamp(100, 65, points));
                points -= sliders[main].value;
                sliders[secondary].value = Mathf.Clamp(sliders[secondary].value, points, Mathf.Clamp(35, 0, points));
            }
        }
    }

    void RoleSliding(int role, int slider) {
        switch (role) {
            case (0):
                RoleSliding(0, 1, 2, slider);
                break;
            case (1):
                RoleSliding(1, 2, 0, slider);
                break;
            case (2):
                RoleSliding(2, 0, 1, slider);
                break;
            default:
                FreeModeSliding(slider);
                break;
        } for (int i = 0; i < sliders.Length; i++) {
            attributes[i] = (int)Mathf.Ceil(sliders[i].value);
            texts[i].text = "" + attributes[i];
        }
    }

    public Hashtable getAttributes() {
        Hashtable ht = new Hashtable();
        int i = 0;
        foreach (int att in attributes)
            ht.Add(transform.Find("Button (" + i++ + ")").GetComponent<Button>().GetComponentInChildren<Text>().text, att);
        return ht;
    }

    public void SetRole(int role) {
        RoleSelection = RoleSelection == role ? -1 : role;
        Button roleButton = GameObject.Find("Button (" + role + ")").GetComponent<Button>();
        ColorBlock cb = roleButton.colors;
        cb.normalColor = RoleSelection == role ? cb.highlightedColor : cb.pressedColor;
        roleButton.colors = cb;
        if (RoleSelection >= 0) {
            sliders[RoleSelection].value = 100;
            RoleSliding(RoleSelection, RoleSelection);
        } else
            FreeModeSliding(role);
    }
}
