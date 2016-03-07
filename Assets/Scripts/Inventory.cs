using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {
    [SerializeField]
    int bananas = 10;
    //THOUGHT: Cap/Duration/whatever?

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public string bananaCount() {
        return "" + bananas;
    }

    public bool useBanana(int count = 1) {
        if (bananas == 0)
            return false;
        bananas -= count;
        return true;
    }

    public void pickupBanana(int count = 1) {
        bananas += count;
        transform.FindChild("Banana").GetComponentInChildren<Text>().text = bananaCount();
    }
}
