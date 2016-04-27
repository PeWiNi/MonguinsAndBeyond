using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HealthSlider : MonoBehaviour {

    Slider healthSlider;
    Text healthText;
    PlayerStats ps;
    Canvas canv;
    [SerializeField]
    Camera cam;
    Image fill;
    [SerializeField]
    Text nameText;

    // Use this for initialization
    void Start() {
        healthSlider = GetComponentInChildren<Slider>();
        healthText = healthSlider.GetComponentInChildren<Text>();
        ps = gameObject.GetComponentInParent<PlayerStats>();
        canv = gameObject.GetComponentInParent<Canvas>();
        fill = healthSlider.transform.FindChild("Fill Area").FindChild("Fill").GetComponent<Image>();
        nameText = transform.parent.GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update() {
        healthText.text = (int)System.Math.Ceiling(ps.health) + "/" + System.Math.Ceiling(ps.maxHealth);
        healthSlider.value = (int)((float)(ps.health / ps.maxHealth) * 100);
        if(cam != null) canv.transform.LookAt(cam.transform);
        canv.transform.Rotate(new Vector3(0f, 180f, 0f));
    }

    public void setCamera(Camera camera, bool team = false) {
        nameText.text = ps.playerName;
        cam = camera;
        // Placeholder Friend/Foe Colors
        fill.color = team ? new Color(0, 1, 0) : new Color(1, 0, 0);
        healthText.color = team ?  new Color(0, 0, 0) : new Color(1, 1, 0);
    }
}