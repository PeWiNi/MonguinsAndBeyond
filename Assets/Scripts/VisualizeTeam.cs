﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class VisualizeTeam : NetworkBehaviour {
    [SerializeField]
    GameObject defHat;
    [SerializeField]
    GameObject attHat;
    [SerializeField]
    GameObject supHat;
    [SerializeField]
	Material bananterial;
	[SerializeField]
	Material fishterial;

    public void TeamMaterialChange(int team, bool show = true) {
        //GetComponent<PlayerStats> ().standardMat.mainTexture = Resources.Load("Materials/Textures/" + (team == 1 ? "monguinUV2_banana" : "monguinUV2_fish")) as Texture;
		GetComponent<PlayerStats>().SetStandardMaterial(team == 1 ? bananterial : team == 2 ? fishterial : Resources.Load("Materials/monguin") as Material);
        //banana.SetActive(show ? team == 1 : false);
        //fish.SetActive(show ? team == 2 : false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="role">1 is defender, 2 is attacker and 3 is supporter (0 is off)</param>
    public void SetHat(int role) {
        if (role == 0) { defHat.SetActive(false); attHat.SetActive(false); supHat.SetActive(false); }
        if (role == 1) { defHat.SetActive(true);  attHat.SetActive(false); supHat.SetActive(false); }
        if (role == 2) { defHat.SetActive(false); attHat.SetActive(true);  supHat.SetActive(false); }
        if (role == 3) { defHat.SetActive(false); attHat.SetActive(false); supHat.SetActive(true);  }
    }
}
