using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class VisualizeTeam : NetworkBehaviour {
    [SerializeField]
    GameObject fish;
    [SerializeField]
	GameObject banana;
	[SerializeField]
	Material bananterial;
	[SerializeField]
	Material fishterial;


    public void ToggleForeheadItem(int team, bool show = true) {
//		GetComponent<PlayerStats> ().standardMat.mainTexture = Resources.Load("Materials/Textures/" + (team == 1 ? "monguinUV2_banana" : "monguinUV2_fish")) as Texture;
		GetComponent<PlayerStats>().standardMat= team == 1 ? bananterial : fishterial;
		GetComponent<PlayerStats> ().ChangeMaterial (false);
       // banana.SetActive(show ? team == 1 : false);
      //  fish.SetActive(show ? team == 2 : false);
    }
}
