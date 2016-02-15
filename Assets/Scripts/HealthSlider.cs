﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HealthSlider : MonoBehaviour {

    Slider healthSlider;
    Text healthText;
    PlayerStats ps;
    Canvas canv;
    [SerializeField]
    Camera cam;

    // Use this for initialization
    void Start() {
        healthSlider = this.gameObject.GetComponentInChildren<Slider>();
        healthText = healthSlider.GetComponentInChildren<Text>();
        ps = gameObject.GetComponentInParent<PlayerStats>();
        canv = gameObject.GetComponentInParent<Canvas>();
    }

    // Update is called once per frame
    void Update() {
        healthText.text = (int)System.Math.Ceiling(ps.health) + "/" + ps.maxHealth;
        healthSlider.value = (int)((float)(ps.health / ps.maxHealth) * 100);
        if(cam != null) canv.transform.LookAt(cam.transform);
        canv.transform.Rotate(new Vector3(0f, 180f, 0f));
    }

    public void setCamera(Camera camera) {
        cam = camera;
    }
}