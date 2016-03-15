using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {
    [SerializeField]
    int bananas = 10;
    [SerializeField]
    int sticks = 2;
    public int bananaCount { get { return bananas; } }
    public int stickCount { get { return sticks; } }
    //THOUGHT: Cap/Duration/whatever?

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public bool useBanana(int count = 1) {
        if (bananas == 0)
            return false;
        bananas -= count;
        return true;
    }

    public bool useSticks(int count = 1) {
        if (sticks == 0)
            return false;
        sticks -= count;
        return true;
    }

    public void pickupBanana(int count = 1) {
        bananas += count;
        transform.FindChild("Banana").GetComponentInChildren<Text>().text = "" + bananaCount;
    }

    public void pickupSticks(int count = 1) {
        sticks += count;
        transform.FindChild("Sticks").GetComponentInChildren<Text>().text = "" + stickCount;
    }
}
